using System.Configuration;
using JetBrains.Annotations;
using OpenTracing.Contrib.LocalTracers.Config.Utils;

namespace OpenTracing.Contrib.LocalTracers.Config.File
{
    [PublicAPI]
    public class FileElement : ConfigurationElement
    {
        [ConfigurationProperty("enabled", IsRequired = true)]
        public ConfigurationTextElement<bool> Enabled => (ConfigurationTextElement<bool>) base["enabled"];

        [ConfigurationProperty("rootLocation", IsRequired = true)]
        public RootLocationElement RootLocation => (RootLocationElement) base["rootLocation"];

        [ConfigurationProperty("outputMode", IsRequired = true)]
        public ConfigurationTextElement<OutputMode> OutputMode => (ConfigurationTextElement<OutputMode>) base["outputMode"];
    }
}