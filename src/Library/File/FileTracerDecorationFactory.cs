namespace OpenTracing.Contrib.LocalTracers.File
{
    using System;
    using System.IO;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.File;

    public static class FileTracerDecorationFactory
    {
        [NotNull]
        [PublicAPI]
        public static TracerDecoration Create(IFileConfiguration config)
        {
            if (!config.Enabled)
            {
                return NoopTracerDecorationFactory.Instance.ToPublicType();
            }

            if (!Directory.Exists(config.RootLocation.Path))
            {
                if (!config.RootLocation.CreateIfNotExists)
                {
                    // No output location and configured not to make it, return noop
                    return NoopTracerDecorationFactory.Instance.ToPublicType();
                }
                else
                {
                    Directory.CreateDirectory(config.RootLocation.Path);
                }
            }

            var fileOutputHelper = new FileOutputHelper(config.RootLocation.Path);
            switch (config.OutputMode)
            {
                case OutputMode.Csv:
                    return new CsvFileTracerDecoration(
                            fileOutputHelper.WriteToFile)
                        .ToPublicType();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}