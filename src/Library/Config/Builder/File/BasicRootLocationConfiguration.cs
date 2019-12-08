namespace OpenTracing.Contrib.LocalTracers.Config.Builder.File
{
    using OpenTracing.Contrib.LocalTracers.Config.File;

    internal sealed class BasicRootLocationConfiguration : IRootLocationConfiguration
    {
        public BasicRootLocationConfiguration(string path, bool ifNotExists)
        {
            this.Path = path;
            this.CreateIfNotExists = ifNotExists;
        }

        public string Path { get; internal set; }
        public bool CreateIfNotExists { get; internal set; }
    }
}