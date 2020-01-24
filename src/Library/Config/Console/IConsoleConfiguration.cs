namespace OpenTracing.Contrib.LocalTracers.Config.Console
{
    using System;

    using JetBrains.Annotations;

    [PublicAPI]
    public interface IConsoleConfiguration
    {
        bool Enabled { get; }
        ColorMode ColorMode { get; }
        string Format { get; }
        IPerTraceCategoryConfiguration<bool> OutputSpanNameOnCategory { get; }
        bool OutputDurationOnFinished { get; }
        IDataSerializationConfiguration DataSerialization { get; }

        /// <summary>
        /// Only valid for <see cref="ColorMode"/> == BasedOnCategory
        /// </summary>
        IPerTraceCategoryConfiguration<ConsoleColor> ColorsForTheBasedOnCategoryColorMode { get; }
    }
}