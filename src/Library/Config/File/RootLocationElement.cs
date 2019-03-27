using System.Configuration;
using JetBrains.Annotations;
using OpenTracing.Contrib.LocalTracers.Config.Utils;

namespace OpenTracing.Contrib.LocalTracers.Config.File
{
    [PublicAPI]
    public class RootLocationElement : ConfigurationElement
    {
        [ConfigurationProperty("path")]
        public ConfigurationTextElement<string> Path => (ConfigurationTextElement<string>) base["path"];

        [ConfigurationProperty("createIfNotExists")]
        public bool CreateIfNotExists => (bool) base["createIfNotExists"];
    }
}