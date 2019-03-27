using System.Configuration;
using JetBrains.Annotations;
using OpenTracing.Contrib.LocalTracers.Config.Utils;

namespace OpenTracing.Contrib.LocalTracers.Config.Console
{
    [PublicAPI]
    public class PerCategoryElement<T> : ConfigurationElement
    {
        [ConfigurationProperty("Activated", IsRequired = false)]
        public ConfigurationTextElement<T> Activated => (ConfigurationTextElement<T>) base["Activated"];

        [ConfigurationProperty("Finished", IsRequired = false)]
        public ConfigurationTextElement<T> Finished => (ConfigurationTextElement<T>) base["Finished"];

        [ConfigurationProperty("SetTag", IsRequired = false)]
        public ConfigurationTextElement<T> SetTag => (ConfigurationTextElement<T>) base["SetTag"];

        [ConfigurationProperty("Log", IsRequired = false)]
        public ConfigurationTextElement<T> Log => (ConfigurationTextElement<T>) base["Log"];
    }
}