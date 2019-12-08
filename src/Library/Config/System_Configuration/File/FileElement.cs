namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration.File
{
    using System.Configuration;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.File;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Utils;

    [PublicAPI]
    public class FileElement : ConfigurationElement, IFileConfiguration
    {
        [ConfigurationProperty("enabled", IsRequired = true)]
        public ConfigurationTextElement<bool> Enabled => (ConfigurationTextElement<bool>) base["enabled"];

        [ConfigurationProperty("rootLocation", IsRequired = true)]
        public RootLocationElement RootLocation => (RootLocationElement) base["rootLocation"];

        [ConfigurationProperty("outputMode", IsRequired = true)]
        public ConfigurationTextElement<OutputMode> OutputMode => (ConfigurationTextElement<OutputMode>) base["outputMode"];

        bool IFileConfiguration.Enabled => this.Enabled;

        IRootLocationConfiguration IFileConfiguration.RootLocation => this.RootLocation;

        OutputMode IFileConfiguration.OutputMode => this.OutputMode;
    }
}