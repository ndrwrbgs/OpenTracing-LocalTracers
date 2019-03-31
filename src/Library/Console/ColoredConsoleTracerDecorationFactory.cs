using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OpenTracing.Contrib.LocalTracers.Config.Console;

namespace OpenTracing.Contrib.LocalTracers.Console
{
    using System.Buffers;
    using System.Runtime.CompilerServices;

    public static class ColoredConsoleTracerDecorationFactory
    {
        [NotNull]
        [PublicAPI]
        public static TracerDecoration Create(ConsoleElement config)
        {
            if (!config.Enabled)
            {
                return NoopTracerDecorationFactory.Instance.ToPublicType();
            }

            ColoredConsoleTracerDecoration.ColorChooser colorChooser = GetColorChooser(config.ColorMode, () => config.ColorsForCategoryTypeColorMode);
            ColoredConsoleTracerDecoration.TextFormatter textFormatter = GetTextFormatter(config.Format, config.OutputSpanNameOnCategory);

            ColoredConsoleTracerDecoration.LogSerializer logSerializer = GetLogSerializer(config.DataSerialization.Log);
            ColoredConsoleTracerDecoration.SetTagSerializer setTagSerializer = GetSetTagSerializer(config.DataSerialization.SetTag);

            return new ColoredConsoleTracerDecoration(
                    colorChooser,
                    logSerializer,
                    textFormatter,
                    setTagSerializer)
                .ToPublicType();
        }

        private static ColoredConsoleTracerDecoration.ColorChooser GetColorChooser(
            ColorMode configColorMode,
            Func<PerCategoryElement<ConsoleColor>> configColorsForLogCategoryTypeColorMode)
        {
            switch (configColorMode)
            {
                case ColorMode.BasedOnCategory:
                    PerCategoryElement<ConsoleColor> configForMode = configColorsForLogCategoryTypeColorMode();
                    return GetLogCategoryTypeColorChooser(configForMode);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ColoredConsoleTracerDecoration.ColorChooser GetLogCategoryTypeColorChooser(PerCategoryElement<ConsoleColor> configForMode)
        {
            return (span, operationName, outputCategory) => configForMode.PerLogCategoryElementToValue(outputCategory);
        }

        private static T PerLogCategoryElementToValue<T>(this PerCategoryElement<T> element, ColoredConsoleTracerDecoration.OutputCategory outputCategory)
        {
            switch (outputCategory)
            {
                case ColoredConsoleTracerDecoration.OutputCategory.Log:
                    return element.Log.Value;
                case ColoredConsoleTracerDecoration.OutputCategory.SetTag:
                    return element.SetTag.Value;
                case ColoredConsoleTracerDecoration.OutputCategory.Activated:
                    return element.Activated.Value;
                case ColoredConsoleTracerDecoration.OutputCategory.Finished:
                    return element.Finished.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outputCategory), outputCategory, null);
            }
        }

        private static ColoredConsoleTracerDecoration.TextFormatter GetTextFormatter(string configFormat, PerCategoryElement<bool> configOutputSpanNameOnCategory)
        {
            int maxSpanIdLengthSeenSoFar = 0;
            object spanIdLengthLock = new object();

            int maxLengthLogCategory = Enum.GetValues(typeof(ColoredConsoleTracerDecoration.OutputCategory))
                .OfType<ColoredConsoleTracerDecoration.OutputCategory>()
                .Select(e => e.ToString())
                .Max(s => s.Length);

            return (spanId, operationName, outputCategory, outputText) =>
            {
                bool outputSpanName = configOutputSpanNameOnCategory.PerLogCategoryElementToValue(outputCategory);

                // Date replace is too complicated for StringBuilder
                var configWithDateReplaced = Regex.Replace(
                    configFormat,
                    @"\{date\:(.*?)\}",
                    match =>
                    {
                        var format = match.Groups[1].Value;
                        return DateTime.Now.ToString(format);
                    });

                StringBuilder value = new StringBuilder(configWithDateReplaced);

                value = value.Replace("{spanId}", spanId);

                if (configFormat.Contains("{spanIdFloatPadding}"))
                {
                    if (spanId.Length > maxSpanIdLengthSeenSoFar)
                    {
                        lock (spanIdLengthLock)
                        {
                            // Double-check locking
                            if (spanId.Length > maxSpanIdLengthSeenSoFar)
                            {
                                maxSpanIdLengthSeenSoFar = spanId.Length;
                            }
                        }
                    }

                    value = value.Replace("{spanIdFloatPadding}", new string(' ', maxSpanIdLengthSeenSoFar - spanId.Length));
                }

                value = value.Replace("{logCategory}", outputCategory.ToString());
                value = value.Replace("{logCategoryPadding}", new string(' ', maxLengthLogCategory - outputCategory.ToString().Length));

                if (outputSpanName)
                {
                    outputText = string.IsNullOrEmpty(outputText)
                        ? operationName
                        : (operationName + " " + outputText);
                }

                value = value.Replace("{outputData}", outputText);

                return value;
            };
        }

        private static ColoredConsoleTracerDecoration.SetTagSerializer GetSetTagSerializer(SetTagDataSerialization mode)
        {
            switch (mode)
            {
                case SetTagDataSerialization.Simple:
                    return tag => $"{tag.key} = {tag.value}";
                case SetTagDataSerialization.Json:
                    return tag => JsonConvert.SerializeObject(tag);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static readonly ArrayPool<string> arrayPool = ArrayPool<string>.Create();

        private static ColoredConsoleTracerDecoration.LogSerializer GetLogSerializer(LogDataSerialization mode)
        {
            switch (mode)
            {
                case LogDataSerialization.Simple:
                    // Translation of the following, for performance, could import something like FastLinq and my array pool utilities for string.Join, but let's avoid the dependencies
                    ////return fields => string.Join(", ", fields.Select(field => $"{field.key} = {field.value}"));
                    return fields =>
                    {
                        // From simple benchmarks, this is about 50% faster and less memory than string.Join approach
                        const int partsPerField = 4;
                        int stringPartsElementCount = fields.Length * partsPerField - 1; // No separator for the last entry

                        var arrayOfStringParts = arrayPool.Rent(stringPartsElementCount);
                        try
                        {
                            for (int i = 0; i < fields.Length; i++)
                            {
                                var field = fields[i];

                                // Breaking apart the transformation $"{field.key} = {field.value}"
                                var indexInArrayOfStringParts = i * partsPerField;
                                arrayOfStringParts[indexInArrayOfStringParts] = field.key;
                                arrayOfStringParts[indexInArrayOfStringParts + 1] = " = ";
                                arrayOfStringParts[indexInArrayOfStringParts + 2] = field.value?.ToString();

                                // If more items, add separator
                                if (i < fields.Length - 1)
                                {
                                    arrayOfStringParts[indexInArrayOfStringParts + 3] = ", ";
                                }
                            }

                            // Faster than string.Concat for some concerning reason
                            return string.Join(string.Empty, arrayOfStringParts, 0, stringPartsElementCount);
                        }
                        finally
                        {
                            arrayPool.Return(arrayOfStringParts);
                        }
                    };
                case LogDataSerialization.Json:
                    return JsonConvert.SerializeObject;
                case LogDataSerialization.SimplifySingleKvpAndEventsOtherwiseJson:
                    return fields =>
                    {
                        if (fields.Length == 1)
                        {
                            string toReturn;

                            // Special OpenTracing convention for a single line log
                            if (fields[0].key == "event")
                            {
                                // Omit the key
                                toReturn = string.Empty;
                            }
                            else
                            {
                                toReturn = fields[0].key + " = ";
                            }

                            if (fields[0].value is string valueAsString)
                            {
                                toReturn += valueAsString;
                            }
                            else
                            {
                                toReturn += JsonConvert.SerializeObject(fields[0].value);
                            }

                            return toReturn;
                        }

                        return JsonConvert.SerializeObject(fields);
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}