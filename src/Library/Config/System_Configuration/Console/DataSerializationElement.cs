namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Console
{
    using System.Configuration;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.Console;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Utils;

    [PublicAPI]
    public class DataSerializationElement : ConfigurationElement, IDataSerializationConfiguration
    {
        [ConfigurationProperty("SetTag", IsRequired = false)]
        public ConfigurationTextElement<SetTagDataSerialization> SetTag => (ConfigurationTextElement<SetTagDataSerialization>) base["SetTag"];

        [ConfigurationProperty("Log", IsRequired = false)]
        public ConfigurationTextElement<LogDataSerialization> Log => (ConfigurationTextElement<LogDataSerialization>) base["Log"];

        SetTagDataSerialization IDataSerializationConfiguration.SetTag => this.SetTag;

        LogDataSerialization IDataSerializationConfiguration.Log => this.Log;
    }
}