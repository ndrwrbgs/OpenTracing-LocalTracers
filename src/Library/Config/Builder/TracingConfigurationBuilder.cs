namespace OpenTracing.Contrib.LocalTracers.Config.Builder
{
    using System;

    using JetBrains.Annotations;

    using OpenTracing.Contrib.LocalTracers.Config.Builder.File;
    using OpenTracing.Contrib.LocalTracers.Config.Console;
    using OpenTracing.Contrib.LocalTracers.Config.File;

    public sealed class TracingConfigurationBuilder
    {
        public static TracingConfigurationBuilder Instance => new TracingConfigurationBuilder();

        [NotNull] private readonly ConsoleConfigurationBuilder consoleConfigurationBuilder;
        private readonly string fileTracingPath;
        private readonly bool fileTracingCreateIfNotExists;
        private readonly OutputMode fileTracingOutputMode;

        public TracingConfigurationBuilder()
            : this(
                new ConsoleConfigurationBuilder(),
                null,
                false,
                OutputMode.Csv) { }

        internal TracingConfigurationBuilder(
            [NotNull] ConsoleConfigurationBuilder consoleConfigurationBuilder,
            string fileTracingPath,
            bool fileTracingCreateIfNotExists,
            OutputMode fileTracingOutputMode)
        {
            this.consoleConfigurationBuilder = consoleConfigurationBuilder;
            this.fileTracingPath = fileTracingPath;
            this.fileTracingCreateIfNotExists = fileTracingCreateIfNotExists;
            this.fileTracingOutputMode = fileTracingOutputMode;
        }

        public IConsoleConfiguration BuildConsoleConfiguration()
        {
            return this.consoleConfigurationBuilder.Build();
        }

        public IFileConfiguration BuildFileConfiguration()
        {
            return new BasicFileConfiguration(
                this.fileTracingPath != null,
                new BasicRootLocationConfiguration(
                    this.fileTracingPath,
                    this.fileTracingCreateIfNotExists),
                this.fileTracingOutputMode);
        }

        public TracingConfigurationBuilder WithFileTracing(
            string path,
            bool createIfNotExists = true,
            OutputMode outputMode = OutputMode.Csv)
        {
            return new TracingConfigurationBuilder(
                this.consoleConfigurationBuilder,
                path,
                createIfNotExists,
                outputMode);
        }

        public TracingConfigurationBuilder WithConsoleTracing(
            string format,
            Func<ConsoleConfigurationBuilder, ConsoleConfigurationBuilder> settings)
        {
            ConsoleConfigurationBuilder consoleBuilder = settings(
                new ConsoleConfigurationBuilder()
                    .WithEnabled(true)
                    .WithFormat(format));
            return new TracingConfigurationBuilder(
                consoleBuilder,
                this.fileTracingPath,
                this.fileTracingCreateIfNotExists,
                this.fileTracingOutputMode);
        }
    }
}