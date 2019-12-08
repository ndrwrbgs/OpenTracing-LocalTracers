namespace OpenTracing.Contrib.LocalTracers.Config.File
{
    public interface IRootLocationConfiguration
    {
        string Path { get; }
        bool CreateIfNotExists { get; }
    }
}