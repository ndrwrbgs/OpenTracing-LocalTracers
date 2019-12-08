namespace OpenTracing.Contrib.LocalTracers.Config.Console
{
    public interface IDataSerializationConfiguration
    {
        SetTagDataSerialization SetTag { get; }
        LogDataSerialization Log { get; }
    }
}