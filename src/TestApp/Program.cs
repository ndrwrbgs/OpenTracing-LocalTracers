using System;
using System.Configuration;
using OpenTracing.Contrib.LocalTracers;
using OpenTracing.Contrib.LocalTracers.Console;
using OpenTracing.Contrib.LocalTracers.File;
using OpenTracing.Mock;

namespace TestApp
{
    using System.Collections.Generic;

    using OpenTracing.Contrib.LocalTracers.Config;
    using OpenTracing.Contrib.LocalTracers.Config.Builder;
    using OpenTracing.Contrib.LocalTracers.Config.Console;
    using OpenTracing.Contrib.LocalTracers.Config.File;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var tracingConfigurationSection = ((TracingConfigurationSection) ConfigurationManager.GetSection("tracing"));
            IConsoleConfiguration consoleConfiguration = tracingConfigurationSection.Console;
            IFileConfiguration fileElement = tracingConfigurationSection.File;

            var builder = TracingConfigurationBuilder.Instance
                // no file tracing
                //.WithFileTracing
                .WithConsoleTracing(
                    "[{date:h:mm:ss tt}] {spanId}{spanIdFloatPadding} | {logCategory}{logCategoryPadding} | {outputData}",
                    settings => settings
                        .WithColorMode(ColorMode.BasedOnCategory)
                        .WithColorsForTheBasedOnCategoryColorMode(
                            ConsoleColor.Green,
                            ConsoleColor.Red,
                            ConsoleColor.Magenta,
                            ConsoleColor.Blue)
                        .WithOutputSpanNameOnCategory(
                            activated: true,
                            finished: true)
                        .WithOutputDurationOnFinished(true)
                        .WithDataSerialization(
                            SetTagDataSerialization.Simple,
                            LogDataSerialization.Json));
            consoleConfiguration = builder.BuildConsoleConfiguration();
            fileElement = builder.BuildFileConfiguration();

            var tracer = new MockTracer()
                .Decorate(ColoredConsoleTracerDecorationFactory.Create(consoleConfiguration))
                .Decorate(FileTracerDecorationFactory.Create(fileElement));

            using (tracer.BuildSpan("test").StartActive())
            {
                tracer.ActiveSpan.Log(
                    new[]
                    {
                        new KeyValuePair<string, object>("a", "b"), 
                        new KeyValuePair<string, object>("b", "b"), 
                        new KeyValuePair<string, object>("c", "b"), 
                        new KeyValuePair<string, object>("d", "b"), 
                    });

                Console.WriteLine("Inside span");
                using (tracer.BuildSpan("inner")
                    .WithTag("foo", "bar,\nhi")
                    .StartActive())
                {
                    Console.WriteLine("Inside inner");
                }

                List<object> selfReferencingObject = new List<object>();
                selfReferencingObject.Add(selfReferencingObject);
                tracer.ActiveSpan.Log(
                    new Dictionary<string, object>
                    {
                        [nameof(selfReferencingObject)] = selfReferencingObject
                    });
            }
        }
    }
}