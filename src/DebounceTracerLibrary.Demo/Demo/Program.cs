using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    using System.IO;
    using System.Threading;

    using OpenTracing;
    using OpenTracing.Contrib.ConsoleCvJsonTracer;
    using OpenTracing.Contrib.DebounceTracerLibrary;
    using OpenTracing.Contrib.Decorators;

    class Program
    {
        static void Main(string[] args)
        {
            var tracer = DebounceTracerFactory.CreateTracer(
                // This tracer is used for the Active/Inject/Extract/Context implementations
                CvTextWriterTracerFactory.CreateTracer(TextWriter.Null),
                EmitSetTagEvent,
                EmitLogEvent,
                EmitSpan,
                TimeSpan.FromSeconds(0.4));

            // Parent, will be emitted eventually
            Console.WriteLine("Build parent");
            using (tracer.BuildSpan("parent")
                .WithTag("Sample", true)
                .StartActive())
            {
                // Kick off a bunch of short tasks, should not be emitted ever
                Console.WriteLine("Kick off a bunch of short tasks, should not be emitted ever");
                for (int i = 0; i < 100; i++)
                {
                    using (tracer.BuildSpan("dontEmit." + i).StartActive())
                    {
                    }
                }

                // Wait for parent to emit
                Console.WriteLine("Wait for parent to emit");
                Thread.Sleep(TimeSpan.FromSeconds(0.5));

                // Practice a nested child forcing emission due to Log
                Console.WriteLine("Practice a nested child forcing emission due to Log");
                using (tracer.BuildSpan("logParent").StartActive())
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    using (tracer.BuildSpan("logger").StartActive())
                    {
                        tracer.ActiveSpan.Log("some event");
                    }
                }

                // Practice a nested child forcing emission due to SetTag.error
                Console.WriteLine("Practice a nested child forcing emission due to SetTag.error");
                using (tracer.BuildSpan("tagParent").StartActive())
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    using (tracer.BuildSpan("tagger").StartActive())
                    {
                        tracer.ActiveSpan.SetTag("error.kind", nameof(InvalidOperationException));
                    }
                }

                // Testing that parent-forced emission emits the previous tags
                Console.WriteLine("Testing that parent-forced emission emits the previous tags");
                using (tracer.BuildSpan("tagParentWithTags")
                    .WithTag("some tag", "some value")
                    .StartActive())
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    using (tracer.BuildSpan("tagger").StartActive())
                    {
                        tracer.ActiveSpan.SetTag("error.kind", nameof(Exception));
                    }
                }
            }
        }

        private static void EmitSetTagEvent((ISpan span, string operationName, TagKeyValue tagKeyValue) obj)
        {
            Console.WriteLine($"TAG: {DateTimeOffset.Now:O}\t{obj.operationName}\t{obj.tagKeyValue.key} = {obj.tagKeyValue.value}");
        }

        private static void EmitLogEvent((ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues) obj)
        {
            foreach (var kvp in obj.logKeyValues)
            {
                Console.WriteLine($"LOG: {DateTimeOffset.Now:O}\t@ {obj.timestamp:O}\t{obj.operationName}\t{kvp.key} = {kvp.value}");
            }
        }

        private static void EmitSpan((ISpan span, string operationName, DateTimeOffset whenActivated) obj)
        {
            // TODO: Probably we want span name maybe possibly?
            Console.WriteLine($"SRT: {DateTimeOffset.Now:O}\t@ {obj.whenActivated:O}\t{obj.operationName}");
        }
    }
}
