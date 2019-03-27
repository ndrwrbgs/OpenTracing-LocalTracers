using System;
using System.Configuration;
using JetBrains.Annotations;
using OpenTracing.Contrib.LocalTracers.Config.Utils;

namespace OpenTracing.Contrib.LocalTracers.Config.Console
{
    [PublicAPI]
    public class ConsoleElement : ConfigurationElement
    {
        [ConfigurationProperty("enabled", IsRequired = true)]
        public ConfigurationTextElement<bool> Enabled => (ConfigurationTextElement<bool>) base["enabled"];

        [ConfigurationProperty("colorMode", IsRequired = true)]
        public ConfigurationTextElement<ColorMode> ColorMode => (ConfigurationTextElement<ColorMode>) base["colorMode"];

        [ConfigurationProperty("format", IsRequired = true)]
        public ConfigurationTextElement<string> Format => (ConfigurationTextElement<string>) base["format"];

        [ConfigurationProperty("outputSpanNameOnLogTypes", IsRequired = true)]
        public PerCategoryElement<bool> OutputSpanNameOnCategory => (PerCategoryElement<bool>) base["outputSpanNameOnLogTypes"];

        [ConfigurationProperty("dataSerialization", IsRequired = true)]
        public DataSerializationElement DataSerialization => (DataSerializationElement) base["dataSerialization"];

        [ConfigurationProperty("colorsForBasedOnCategoryColorMode", IsRequired = false)]
        public PerCategoryElement<ConsoleColor> ColorsForCategoryTypeColorMode => (PerCategoryElement<ConsoleColor>) base["colorsForBasedOnCategoryColorMode"];
    }
}