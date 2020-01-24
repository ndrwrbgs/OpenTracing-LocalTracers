namespace OpenTracing.Contrib.LocalTracers.Config.Builder.Console
{
    using System;

    using OpenTracing.Contrib.LocalTracers.Config.Console;

    internal sealed class BasicConsoleConfiguration : IConsoleConfiguration
    {
        public BasicConsoleConfiguration(
            bool enabled,
            ColorMode colorMode,
            string format,
            BasicPerTraceCategoryConfiguration<bool> outputSpanNameOnCategory,
            bool outputDurationOnFinished,
            BasicDataSerializationConfiguration dataSerialization,
            BasicPerTraceCategoryConfiguration<ConsoleColor> colorsForTheBasedOnCategoryColorMode)
        {
            this.Enabled = enabled;
            this.ColorMode = colorMode;
            this.Format = format;
            this.OutputSpanNameOnCategory = outputSpanNameOnCategory;
            this.OutputDurationOnFinished = outputDurationOnFinished;
            this.DataSerialization = dataSerialization;
            this.ColorsForTheBasedOnCategoryColorMode = colorsForTheBasedOnCategoryColorMode;
        }

        public bool Enabled { get; internal set; }
        public ColorMode ColorMode { get; internal set; }
        public string Format { get; internal set; }
        public BasicPerTraceCategoryConfiguration<bool> OutputSpanNameOnCategory { get; internal set; }
        public bool OutputDurationOnFinished { get; internal set; }
        public BasicDataSerializationConfiguration DataSerialization { get; internal set; }
        public BasicPerTraceCategoryConfiguration<ConsoleColor> ColorsForTheBasedOnCategoryColorMode { get; internal set; }
        IPerTraceCategoryConfiguration<bool> IConsoleConfiguration.OutputSpanNameOnCategory => this.OutputSpanNameOnCategory;
        IDataSerializationConfiguration IConsoleConfiguration.DataSerialization => this.DataSerialization;
        IPerTraceCategoryConfiguration<ConsoleColor> IConsoleConfiguration.ColorsForTheBasedOnCategoryColorMode => this.ColorsForTheBasedOnCategoryColorMode;
    }
}