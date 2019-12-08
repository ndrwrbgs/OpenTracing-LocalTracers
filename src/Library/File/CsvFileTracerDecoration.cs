namespace OpenTracing.Contrib.LocalTracers.File
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text;

    using CsvHelper;

    using OpenTracing.Contrib.Decorators;

    internal sealed class CsvFileTracerDecoration : ITracerDecoration
    {
        public delegate void WriteToFile(string maybeInvalidFileName, string text);

        private readonly WriteToFile writeToFile;

        public CsvFileTracerDecoration(
            WriteToFile writeToFile)
        {
            this.writeToFile = writeToFile;
        }

        // Can shard if we see this in performance traces, but it's not highly likely... it serves it's goal (of fewer memory allocations)
        private static readonly ArrayPool<string> arrayPool = ArrayPool<string>.Shared;

        OnSpanLog ITracerDecoration.OnSpanLog =>
            (span, operationName, timestamp, fields) =>
            {
                var fieldsSeparatedForCsv = arrayPool.Rent(fields.Length * 2);
                try
                {
                    for (var index = 0; index < fields.Length; index++)
                    {
                        var field = fields[index];
                        fieldsSeparatedForCsv[index * 2] = field.key;
                        fieldsSeparatedForCsv[index * 2 + 1] = field.value?.ToString();
                    }

                    this.WriteLine(
                        span,
                        "Log",
                        fieldsSeparatedForCsv,
                        fields.Length * 2);
                }
                finally
                {
                    arrayPool.Return(fieldsSeparatedForCsv);
                }
            };

        OnSpanSetTag ITracerDecoration.OnSpanSetTag =>
            (span, operationName, value) =>
            {
                var array = arrayPool.Rent(2);
                try
                {
                    array[0] = value.key;
                    array[1] = value.value?.ToString();
                    this.WriteLine(
                        span,
                        "Tag",
                        array,
                        2);
                }
                finally
                {
                    arrayPool.Return(array);
                }
            };

        OnSpanFinished ITracerDecoration.OnSpanFinished =>
            (span, operationName) =>
            {
                this.WriteLine(
                    span,
                    "Finish");
            };

        OnSpanActivated ITracerDecoration.OnSpanActivated =>
            (span, operationName) =>
            {
                this.WriteLine(
                    span,
                    "Start",
                    operationName);
            };

        OnSpanStarted ITracerDecoration.OnSpanStarted => null;
        OnSpanStartedWithFinishCallback ITracerDecoration.OnSpanStartedWithFinishCallback => null;

        private void WriteLine(
            ISpan span,
            string type,
            string[] items,
            int itemsCount)
        {
            string forTraceId = span.Context.TraceId;
            string fileName = forTraceId + ".csv";

            string fullLine = this.GetSerializedOutput(span, type, items, itemsCount);

            this.writeToFile(fileName, fullLine);
        }

        // TODO: The quick-and-dirty "make this performant" made this copy-paste code. Fix that
        private void WriteLine(
            ISpan span,
            string type,
            string item)
        {
            string forTraceId = span.Context.TraceId;
            string fileName = forTraceId + ".csv";

            string fullLine = this.GetSerializedOutput(span, type, item);

            this.writeToFile(fileName, fullLine);
        }

        private void WriteLine(
            ISpan span,
            string type)
        {
            string forTraceId = span.Context.TraceId;
            string fileName = forTraceId + ".csv";

            string fullLine = this.GetSerializedOutput(span, type);

            this.writeToFile(fileName, fullLine);
        }

        private string GetSerializedOutput(ISpan span, string type, string[] items, int itemsCount)
        {
            string spanId = span.Context.SpanId;

            StringBuilder resultBuilder = new StringBuilder();
            using (var writer = new StringWriter(resultBuilder))
            {
                using (var csv = new CsvWriter(writer) {Configuration = {SanitizeForInjection = false}})
                {
                    csv.WriteField(DateTime.Now.ToString("O"));
                    csv.WriteField(spanId);
                    csv.WriteField(type);

                    for (var index = 0; index < itemsCount; index++)
                    {
                        var item = items[index];
                        csv.WriteField(item);
                    }
                }
            }

            string fullLine = resultBuilder.ToString();
            return fullLine;
        }

        private string GetSerializedOutput(ISpan span, string type, string item)
        {
            string spanId = span.Context.SpanId;

            StringBuilder resultBuilder = new StringBuilder();
            using (var writer = new StringWriter(resultBuilder))
            {
                using (var csv = new CsvWriter(writer) {Configuration = {SanitizeForInjection = false}})
                {
                    csv.WriteField(DateTime.Now.ToString("O"));
                    csv.WriteField(spanId);
                    csv.WriteField(type);
                    csv.WriteField(item);
                }
            }

            string fullLine = resultBuilder.ToString();
            return fullLine;
        }

        private string GetSerializedOutput(ISpan span, string type)
        {
            string spanId = span.Context.SpanId;

            StringBuilder resultBuilder = new StringBuilder();
            using (var writer = new StringWriter(resultBuilder))
            {
                using (var csv = new CsvWriter(writer) {Configuration = {SanitizeForInjection = false}})
                {
                    csv.WriteField(DateTime.Now.ToString("O"));
                    csv.WriteField(spanId);
                    csv.WriteField(type);
                }
            }

            string fullLine = resultBuilder.ToString();
            return fullLine;
        }
    }
}