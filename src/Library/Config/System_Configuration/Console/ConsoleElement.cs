namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Console
{
    using System;
    using System.Configuration;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.Console;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Utils;

    [PublicAPI]
    public class ConsoleElement : ConfigurationElement, IConsoleConfiguration
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

        ColorMode IConsoleConfiguration.ColorMode => this.ColorMode;

        string IConsoleConfiguration.Format => this.Format;

        IPerTraceCategoryConfiguration<bool> IConsoleConfiguration.OutputSpanNameOnCategory => this.OutputSpanNameOnCategory;

        IDataSerializationConfiguration IConsoleConfiguration.DataSerialization => this.DataSerialization;

        IPerTraceCategoryConfiguration<ConsoleColor> IConsoleConfiguration.ColorsForTheBasedOnCategoryColorMode => this.ColorsForCategoryTypeColorMode;

        bool IConsoleConfiguration.Enabled => this.Enabled;
    }
}