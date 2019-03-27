using System.Configuration;
using JetBrains.Annotations;
using OpenTracing.Contrib.LocalTracers.Config.Utils;

namespace OpenTracing.Contrib.LocalTracers.Config.Console
{
    [PublicAPI]
    public class DataSerializationElement : ConfigurationElement
    {
        [ConfigurationProperty("SetTag", IsRequired = false)]
        public ConfigurationTextElement<SetTagDataSerialization> SetTag => (ConfigurationTextElement<SetTagDataSerialization>) base["SetTag"];

        [ConfigurationProperty("Log", IsRequired = false)]
        public ConfigurationTextElement<LogDataSerialization> Log => (ConfigurationTextElement<LogDataSerialization>) base["Log"];
    }
}