namespace OpenTracing.Contrib.LocalTracers.Config.Builder.Console
{
    using OpenTracing.Contrib.LocalTracers.Config.Console;

    internal sealed class BasicDataSerializationConfiguration : IDataSerializationConfiguration
    {
        public BasicDataSerializationConfiguration(SetTagDataSerialization tag, LogDataSerialization log)
        {
            this.SetTag = tag;
            this.Log = log;
        }

        public SetTagDataSerialization SetTag { get; internal set; }
        public LogDataSerialization Log { get; internal set; }
    }
}