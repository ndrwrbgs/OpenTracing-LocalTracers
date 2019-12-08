namespace OpenTracing.Contrib.LocalTracers.Config.File
{
    public interface IFileConfiguration
    {
        bool Enabled { get; }
        IRootLocationConfiguration RootLocation { get; }
        OutputMode OutputMode { get; }
    }
}