using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace OpenTracing.Contrib.LocalTracers.File
{
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class FileOutputHelper
    {
        private readonly MemoryCache fileOutputStreamCache = new MemoryCache("FileOutputStreamCache");
        private readonly string rootLocation;

        /// <summary>
        /// Copied from <see cref="StreamWriter"/>'s default
        /// </summary>
        internal static Encoding UTF8NoBOM { get; } = new UTF8Encoding(false, true);

        public FileOutputHelper(string rootLocation)
        {
            this.rootLocation = rootLocation;
        }

        public void WriteToFile(string maybeInvalidFileName, string fullLine)
        {
            var outputFileName = CleanForWindowsFileName(maybeInvalidFileName);
            var filePath = Path.Combine(rootLocation, outputFileName);

            FileStream fileStream = GetCachedFileStream(filePath);
            using (var streamWriter = new StreamWriter(fileStream, UTF8NoBOM, 1024, leaveOpen: true))
            {
                lock (fileStream)
                {
                    streamWriter.WriteLine(fullLine);
                    streamWriter.Flush();
                }
            }
        }

        private FileStream GetCachedFileStream(string filePath)
        {
            // Don't want to actually open the stream multiple times, but AddOrGetExisting does not support a factory method
            // we can't simply do !Contains then Add either since the eviction of the cache could happen between those statements
            // Therefore, using a Lazy since we can create multiple of those no worries
            Lazy<FileStream> lazyFileStream;
            lock (fileOutputStreamCache)
            {
                var valueIfNotExists = new Lazy<FileStream>(
                    () => System.IO.File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                lazyFileStream = (Lazy<FileStream>) fileOutputStreamCache
                    .AddOrGetExisting(
                        filePath,
                        valueIfNotExists,
                        new CacheItemPolicy
                        {
                            // Close the file if it's not touched for 30 seconds
                            SlidingExpiration = TimeSpan.FromSeconds(30),
                        });
                // AddOGetExisting returns null if it adds, what is this class....
                lazyFileStream = lazyFileStream ?? valueIfNotExists;
            }

            return lazyFileStream.Value;
        }

        private static string CleanForWindowsFileName(string fileName)
        {
            IEnumerable<char> invalidCharacters = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars());
            foreach (var c in invalidCharacters)
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }
    }
}