using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    using System.IO;
    using System.Reactive.Linq;
    using System.Threading;

    using OpenTracing;
    using OpenTracing.Contrib.ConsoleCvJsonTracer;
    using OpenTracing.Contrib.DebounceTracerLibrary;
    using OpenTracing.Contrib.Decorators;

    class Program
    {
        static void Main(string[] args)
        {
            //var tracer = DebounceTracerFactory.CreateTracer(
            //    // This tracer is used for the Active/Inject/Extract/Context implementations
            //    CvTextWriterTracerFactory.CreateTracer(TextWriter.Null),
            //    EmitSetTagEvent,
            //    EmitLogEvent,
            //    EmitSpan,
            //    TimeSpan.FromSeconds(0.4));

            (ITracer connectedTracer, IObservable<TraceEvent> eventStream) = TraceToEventStreamAdapter.CreateTracerEventStreamPair(
                // This tracer is used for the Active/Inject/Extract/Context implementations
                // TODO: UUUUUGh I expose the cV directly which does NOT maintain a consistent SpanId thanks to cV - need to chop off the ends
                CvTextWriterTracerFactory.CreateTracer(TextWriter.Null));

            HashSet<string> finishedSpans = new HashSet<string>();
            HashSet<string> startEmittedSpans = new HashSet<string>();

            string GetSpanId(ISpan span)
            {
                return span.Context.SpanId.Substring(0, span.Context.SpanId.LastIndexOf('.'));
            }

            TimeSpan minimumSpanLength = TimeSpan.FromSeconds(0.4);
            eventStream
                // TODO: Do this or make the collections parallel
                //.ObserveOn(/* something single threaded for simplicity */ )
                // Record event info when it first happens
                .Do(
                    item =>
                    {
                        item.Switch(
                            // we COULD emit tags/logs now if the span start was already emitted, but lets keep all emission in sync
                            _ => { },
                            _ => { },
                            _ => { },
                            finishedEvent => { finishedSpans.Add(GetSpanId(finishedEvent.Span)); });
                    })
                .Delay(minimumSpanLength/* todo: add scheduler */)
                .Subscribe(
                    item =>
                    {
                        item.Switch(
                            setTagsEvent =>
                            {
                                if (startEmittedSpans.Contains(GetSpanId(setTagsEvent.Span)))
                                {
                                    EmitSetTagEvent((setTagsEvent.Span, setTagsEvent.OperationName, setTagsEvent.TagKeyValue));
                                    return;
                                }

                                if (setTagsEvent.TagKeyValue.key.StartsWith("error"))
                                {
                                    // we emit a start event and the tag
                                    // TODO: We should have recorded the start time for the span
                                    DateTimeOffset spanStartTime = DateTimeOffset.Now;
                                    if (!startEmittedSpans.Contains(GetSpanId(setTagsEvent.Span)))
                                    {
                                        EmitSpan((setTagsEvent.Span, setTagsEvent.OperationName, spanStartTime));
                                        startEmittedSpans.Add(GetSpanId(setTagsEvent.Span));
                                    }

                                    EmitSetTagEvent((setTagsEvent.Span, setTagsEvent.OperationName, setTagsEvent.TagKeyValue));
                                }
                            },
                            logEvent =>
                            {
                                // Right now the code expects log to force emission
                                if (!startEmittedSpans.Contains(GetSpanId(logEvent.Span)))
                                {
                                    // we emit a start event and the log
                                    // TODO: We should have recorded the start time for the span
                                    DateTimeOffset spanStartTime = DateTimeOffset.Now;
                                    EmitSpan((logEvent.Span, logEvent.OperationName, spanStartTime));
                                    startEmittedSpans.Add(GetSpanId(logEvent.Span));
                                }
                                EmitLogEvent((logEvent.Span, logEvent.OperationName, logEvent.Timestamp, logEvent.LogKeyValues));
                                //if (startEmittedSpans.Contains(logEvent.Span))
                                //{
                                //    EmitLogEvent((logEvent.Span, logEvent.OperationName, logEvent.Timestamp, logEvent.LogKeyValues));
                                //    return;
                                //}
                            },
                            activatedEvent =>
                            { 
                                // It was already finished by the time we got the delayed start notification
                                if (finishedSpans.Contains(GetSpanId(activatedEvent.Span)))
                                {
                                    // Too short, ignore
                                    return;
                                }

                                // Long enough, emit
                                if (!startEmittedSpans.Contains(GetSpanId(activatedEvent.Span)))
                                {
                                    EmitSpan((activatedEvent.Span, activatedEvent.OperationName, DateTimeOffset.Now));
                                    startEmittedSpans.Add(GetSpanId(activatedEvent.Span));
                                }
                            },
                            finishedEvent =>
                            {
                                if (startEmittedSpans.Contains(GetSpanId(finishedEvent.Span)))
                                {
                                    Console.WriteLine($"{finishedEvent.OperationName} finished");
                                    // Clean up garbage
                                    startEmittedSpans.Remove(GetSpanId(finishedEvent.Span));
                                }
                                
                                // Clean up garbage
                                finishedSpans.Remove(GetSpanId(finishedEvent.Span));
                            });
                    },
                    // Subscribe FOREVER (alternatively, drop this and get back an IDisposable)
                    CancellationToken.None);

            var tracer = connectedTracer;


            // Parent, will be emitted eventually
            Console.WriteLine("Build parent");
            using (tracer.BuildSpan("parent")
                .WithTag("Sample", true)
                .StartActive())
            {
                // Kick off a bunch of short tasks, should not be emitted ever
                tracer.ActiveSpan.Log("Kick off a bunch of short tasks, should not be emitted ever");
                for (int i = 0; i < 100; i++)
                {
                    using (tracer.BuildSpan("dontEmit." + i).StartActive())
                    {
                    }
                }

                // Wait for parent to emit
                tracer.ActiveSpan.Log("Wait for slow child to emit");
                using (tracer.BuildSpan("slow child").StartActive())
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));

                // Practice a nested child forcing emission due to Log
                tracer.ActiveSpan.Log("Practice a nested child forcing emission due to Log");
                using (tracer.BuildSpan("logParent").StartActive())
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    using (tracer.BuildSpan("logger").StartActive())
                    {
                        tracer.ActiveSpan.Log("some event");
                    }
                }

                // Practice a nested child forcing emission due to SetTag.error
                tracer.ActiveSpan.Log("Practice a nested child forcing emission due to SetTag.error");
                using (tracer.BuildSpan("tagParent").StartActive())
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    using (tracer.BuildSpan("tagger").StartActive())
                    {
                        tracer.ActiveSpan.SetTag("error.kind", nameof(InvalidOperationException));
                    }
                }

                // Testing that parent-forced emission emits the previous tags
                tracer.ActiveSpan.Log("Testing that parent-forced emission emits the previous tags");
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

            // an 'end signal' and to wait for it to come through
            Console.WriteLine(" +++ Flushing telemetry");
            // TODO: Really we should emit a signal to our subscription and wait for that signal to come through to here
            Thread.Sleep(TimeSpan.FromTicks((long) (minimumSpanLength.Ticks * 1.5)));
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
