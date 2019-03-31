using System;
using System.IO;
using JetBrains.Annotations;
using OpenTracing.Contrib.LocalTracers.Config.File;

namespace OpenTracing.Contrib.LocalTracers.File
{
    public static class FileTracerDecorationFactory
    {
        [NotNull]
        [PublicAPI]
        public static ITracerDecoration Create(FileElement config)
        {
            if (!config.Enabled)
            {
                return NoopTracerDecorationFactory.Instance;
            }

            if (!Directory.Exists(config.RootLocation.Path))
            {
                if (!config.RootLocation.CreateIfNotExists)
                {
                    // No output location and configured not to make it, return noop
                    return NoopTracerDecorationFactory.Instance;
                }
                else
                {
                    Directory.CreateDirectory(config.RootLocation.Path);
                }
            }
            
            var fileOutputHelper = new FileOutputHelper(config.RootLocation.Path);
            switch (config.OutputMode.Value)
            {
                case OutputMode.Csv:
                    return new CsvFileTracerDecoration(
                        fileOutputHelper.WriteToFile);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}