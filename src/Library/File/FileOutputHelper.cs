namespace OpenTracing.Contrib.LocalTracers.File
{
    using System;
    using System.IO;
    using System.Runtime.Caching;
    using System.Text;

    internal sealed class FileOutputHelper
    {
        private readonly MemoryCache fileOutputStreamCache = new MemoryCache("FileOutputStreamCache");
        private readonly string rootLocation;

        /// <summary>
        /// Copied from <see cref="StreamWriter"/>'s default
        /// </summary>
        internal static Encoding Utf8NoBom { get; } = new UTF8Encoding(false, true);

        public FileOutputHelper(string rootLocation)
        {
            this.rootLocation = rootLocation;
        }

        public void WriteToFile(string maybeInvalidFileName, string fullLine)
        {
            var outputFileName = CleanForWindowsFileName(maybeInvalidFileName);
            var filePath = Path.Combine(this.rootLocation, outputFileName);

            FileStream fileStream = this.GetCachedFileStream(filePath);
            using (var streamWriter = new StreamWriter(fileStream, Utf8NoBom, 1024, leaveOpen: true))
            {
                // We need to lock since we are using Streams rather than, say Console that does locking for us, otherwise
                // various lines may get intermingled (another thread's line starts in the middle of outputting the previous)
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
            lock (this.fileOutputStreamCache)
            {
                var valueIfNotExists = new Lazy<FileStream>(
                    () =>
                    {
                        // TODO: This is really nooooot the way we want to do this, but it's faster to code right now
                        var baseDir = Path.GetDirectoryName(filePath);
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        var guid = Guid.NewGuid().ToString("N");
                        var full = Path.Combine(baseDir, fileName, guid + Path.GetExtension(filePath));
                        Directory.CreateDirectory(Path.GetDirectoryName(full));
                        return File.Open(full, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    });
                lazyFileStream = (Lazy<FileStream>) this.fileOutputStreamCache
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
            // Borrowed from https://stackoverflow.com/a/23182807, https://stackoverflow.com/a/12800424, and https://stackoverflow.com/a/13617375 (though the last de-dupes consecutive invalid chars)
            var invalidCharacters = Path.GetInvalidFileNameChars();
            string newName = string.Join("_", fileName.Split(invalidCharacters));
            return newName;
        }
    }
}