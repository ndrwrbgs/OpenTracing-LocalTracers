namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration.File
{
    using System.Configuration;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.File;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Utils;

    [PublicAPI]
    public class RootLocationElement : ConfigurationElement, IRootLocationConfiguration
    {
        [ConfigurationProperty("path")]
        public ConfigurationTextElement<string> Path => (ConfigurationTextElement<string>) base["path"];

        [ConfigurationProperty("createIfNotExists")]
        public bool CreateIfNotExists => (bool) base["createIfNotExists"];

        string IRootLocationConfiguration.Path => this.Path;
    }
}