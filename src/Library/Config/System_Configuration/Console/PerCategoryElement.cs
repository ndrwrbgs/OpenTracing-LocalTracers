namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Console
{
    using System.Configuration;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.Console;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Utils;

    [PublicAPI]
    public class PerCategoryElement<T> : ConfigurationElement, IPerTraceCategoryConfiguration<T>
    {
        [ConfigurationProperty("Activated", IsRequired = false)]
        public ConfigurationTextElement<T> Activated => (ConfigurationTextElement<T>) base["Activated"];

        [ConfigurationProperty("Finished", IsRequired = false)]
        public ConfigurationTextElement<T> Finished => (ConfigurationTextElement<T>) base["Finished"];

        [ConfigurationProperty("SetTag", IsRequired = false)]
        public ConfigurationTextElement<T> SetTag => (ConfigurationTextElement<T>) base["SetTag"];

        [ConfigurationProperty("Log", IsRequired = false)]
        public ConfigurationTextElement<T> Log => (ConfigurationTextElement<T>) base["Log"];

        T IPerTraceCategoryConfiguration<T>.Activated => this.Activated;

        T IPerTraceCategoryConfiguration<T>.Finished => this.Finished;

        T IPerTraceCategoryConfiguration<T>.SetTag => this.SetTag;

        T IPerTraceCategoryConfiguration<T>.Log => this.Log;
    }
}