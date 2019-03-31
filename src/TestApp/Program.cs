using System;
using System.Configuration;
using OpenTracing.Contrib.LocalTracers;
using OpenTracing.Contrib.LocalTracers.Console;
using OpenTracing.Contrib.LocalTracers.File;
using OpenTracing.Mock;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var tracingConfigurationSection = ((TracingConfigurationSection) ConfigurationManager.GetSection("tracing"));
            var tracer = new MockTracer()
                .Decorate(ColoredConsoleTracerDecorationFactory.Create(tracingConfigurationSection.Console))
                .Decorate(FileTracerDecorationFactory.Create(tracingConfigurationSection.File));

            using (tracer.BuildSpan("test").StartActive())
            {
                Console.WriteLine("Inside span");
                using (tracer.BuildSpan("inner")
                    .WithTag("foo", "bar,\nhi")
                    .StartActive())
                {
                    Console.WriteLine("Inside inner");
                }
            }
        }
    }
}