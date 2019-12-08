namespace OpenTracing.Contrib.LocalTracers.Config.Builder.File
{
    using OpenTracing.Contrib.LocalTracers.Config.File;

    internal sealed class BasicFileConfiguration : IFileConfiguration
    {
        public BasicFileConfiguration(bool enabled, IRootLocationConfiguration rootLocation, OutputMode outputMode)
        {
            this.Enabled = enabled;
            this.RootLocation = rootLocation;
            this.OutputMode = outputMode;
        }

        public bool Enabled { get; internal set; }
        public IRootLocationConfiguration RootLocation { get; internal set; }
        public OutputMode OutputMode { get; internal set; }
    }
}