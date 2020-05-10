using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTracing.Contrib.Decorators;

namespace OpenTracing.Contrib.DebounceTracerLibrary
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Threading;

    using OpenTracing.Contrib.StronglyTyped;
    using OpenTracing.Propagation;
    using OpenTracing.Util;

    /// <summary>
    /// We want to emit a span if it takes longer than X, or has an error, or any of its children do
    /// We can either measure the time by kicking off timers on start, or in some background processing task - make options
    /// We can either emit the fail/long spans with their initial time explicitly, or pretend the time is when it triggers the condition
    /// (you normally want explicit time, but some of MY libraries falsely don't allow that)
    /// 
    /// 
    /// 
    /// our 'or any children' could produce a lot of garbage. We basically want a reverse AsyncLocal
    /// we pass on context to a child at start
    /// we set error on our own context later
    /// we want that set error/time to percolate up the stack, and then IN ORDER going down trigger emitting each (if it hasn't emitted already)
    /// 
    /// ONLY works on onebox tracing
    /// </summary>
    /// <remarks>
    /// TODO: also, I could just delay the whole sequence for that amount of time so they come in order. E.g. if you only want spans longer than 2 seconds you get all logs 2 seconds late
    ///       that might've been a smarter appraoch...
    /// </remarks>
    public static class DebounceTracerFactory
    {
        // Instead of trying to impl OpenTracing's APIs, let's try to convert OT into a stream of events we can then filter on
        public static ITracer CreateTracer(
            ITracer underlying,
            Action<(ISpan span, string operationName, TagKeyValue tagKeyValue)> emitSetTagEvent,
            Action<(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)> emitLogEvent,
            Action<(ISpan span, string operationName, DateTimeOffset whenActivated)> emitSpan,
            TimeSpan emissionTime)
        {
            // TODO: Perf - faster to find the keys if they are shorter strings
            // TODO: We should convert most of these to using SpanId and TraceId instead (but not all providers impl those right)
            const string HasBeenEmittedKey = "DebounceTracerFactory.HasBeenEmitted";
            const string ParentReferenceKey = "DebounceTracerFactory.ParentSpanGuid";
            const string WhenActivatedKey = "DebounceTracerFactory.WhenActivated";
            const string EmitWhenTooLongTaskKey = "DebounceTracerFactory.EmitWhenTooLongTaskGuid";
            const string SpanNameKey = "DebounceTracerFactory.SpanName";
            const string SetTagsKey = "DebounceTracerFactory.SetTagsGuid";
            const string LogsKey = "DebounceTracerFactory.LogsKeyGuid";

            object emissionLock = new object();

            void TriggerParentsAndSelfEmission(ISpan span)
            {
                if (!HasBeenEmitted(span))
                lock (emissionLock)
                if (!HasBeenEmitted(span))
                {
                    // parents first
                    var parentSpan = GetParentSpanFromBaggageOkayToNotBeReproducible(span);
                    if (parentSpan != null)
                    {
                        TriggerParentsAndSelfEmission(parentSpan);
                    }

                    var whenActivated = span.GetBaggageItem(WhenActivatedKey);
                    var parsedWhenActivated = DateTimeOffset.Parse(whenActivated);
                    var spanName = span.GetBaggageItem(SpanNameKey);
                    emitSpan((span, spanName, parsedWhenActivated));

                    foreach (var setTagEventArgs in GetSavedSetTagEventsForLater(span))
                    {
                        emitSetTagEvent(setTagEventArgs);
                    }

                    foreach (var logEventArgs in GetSavedLogEventsForLater(span))
                    {
                        emitLogEvent(logEventArgs);
                    }

                    span.SetBaggageItem(HasBeenEmittedKey, true.ToString(CultureInfo.InvariantCulture));
                }
            }

            ConcurrentDictionary<string, ISpan> storedSpans = new ConcurrentDictionary<string, ISpan>();
            void StoreParentSpanOnBaggage(ISpan childSpan, ISpan parentToStore)
            {
                var guidForSpanToStore = Guid.NewGuid().ToString();
                storedSpans[guidForSpanToStore] = parentToStore;
                childSpan.SetBaggageItem(ParentReferenceKey, guidForSpanToStore);
            }

            ISpan GetParentSpanFromBaggageOkayToNotBeReproducible(ISpan spanStoredOn)
            {
                string baggageItem = spanStoredOn.GetBaggageItem(ParentReferenceKey);
                if (!storedSpans.TryRemove(baggageItem, out ISpan value))
                {
                    // bug race condition - cleanup could beat us to the timer
                    // throw new InvalidOperationException("NOT reproducible -- did you properly instrument with OpenTracing or did you orphan/double-Finish/improperly parent a follows-from span somewhere?");
                    return null;
                }
                return value;
            }

            void RemoveParentSpanFromBaggageForCleanup(ISpan spanStoredOn)
            {
                string baggageItem = spanStoredOn.GetBaggageItem(ParentReferenceKey);
                storedSpans.TryRemove(baggageItem, out ISpan _);
            }

            void EmitLogEvent(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)
            {
                emitLogEvent((span, operationName, timestamp, logKeyValues));
            }

            void EmitSetTagEvent(ISpan span, string operationName, TagKeyValue tagKeyValue)
            {
                emitSetTagEvent((span, operationName, tagKeyValue));
            }
            
            // TODO: Correctness - List isn't thread safe, normally not an issue for OT - and ConcurrentBag isn't accessible in my project RN for some reason... sigh.net
            ConcurrentDictionary<string, List<(ISpan, string, TagKeyValue)>> setTagsDictionary = new ConcurrentDictionary<string, List<(ISpan, string, TagKeyValue)>>();
            IEnumerable<(ISpan span, string operationName, TagKeyValue tagKeyValue)> GetSavedSetTagEventsForLater(ISpan span)
            {
                string key;
                try
                {
                    key = span.GetBaggageItem(SetTagsKey);
                    if (!setTagsDictionary.ContainsKey(key)) // working around the parent context passing to the child bug
                    {
                        return new (ISpan span, string operationName, TagKeyValue tagKeyValue)[0];
                    }
                }
                catch (KeyNotFoundException)
                {
                    return new (ISpan span, string operationName, TagKeyValue tagKeyValue)[0];
                }

                return setTagsDictionary[key];
            }

            void RemoveSetTagsCacheItem(ISpan span)
            {
                string key;
                try
                {
                    key = span.GetBaggageItem(SetTagsKey);
                    setTagsDictionary.TryRemove(key, out _);
                }
                catch (KeyNotFoundException)
                {
                }
            }


            void SaveSetTagEventForLater(ISpan span, string operationName, TagKeyValue tagKeyValue)
            {
                string key;
                try
                {
                    key = span.GetBaggageItem(SetTagsKey);
                }
                catch (KeyNotFoundException)
                {
                    key = Guid.NewGuid().ToString();
                    span.SetBaggageItem(SetTagsKey, key);
                }

                if (!setTagsDictionary.TryGetValue(key, out var list))
                {
                    list = setTagsDictionary[key] = new List<(ISpan, string, TagKeyValue)>();
                }

                list.Add((span, operationName, tagKeyValue));
            }

            // returns true if the span start has been emitted
            bool HasBeenEmitted(ISpan span)
            {
                lock (emissionLock) // TODO: PERF - reader writer lock could be more performant since we will NORMALLY be reading
                    try
                    {
                        return bool.TryParse(span.GetBaggageItem(HasBeenEmittedKey), out bool value) && value;
                    }
                    catch (KeyNotFoundException)
                    {
                        // TODO: Which is slower though, exception handling or enumerating what we expect to be no more than maybe 15ish kvps?
                        /* there exists no 'key exists' on the Get api, and the enumeration api is slow */
                        return false;
                    }
            }

            bool ShouldTriggerEmission(ISpan span, string operationName, TagKeyValue tagKeyValue)
            {
                // TODO: Better to read the key from OpenTracing.SemanticConventions
                return tagKeyValue.key.StartsWith("error");
            }
            
            bool ShouldLogTriggerEmission(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)
            {
                return false;
            }
            
            // TODO: Correctness - List isn't thread safe, normally not an issue for OT - and ConcurrentBag isn't accessible in my project RN for some reason... sigh.net
            ConcurrentDictionary<string, List<(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)>> logsDictionary = new ConcurrentDictionary<string, List<(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)>>();
            IEnumerable<(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)> GetSavedLogEventsForLater(ISpan span)
            {
                string key;
                try
                {
                    key = span.GetBaggageItem(LogsKey);
                    if (!logsDictionary.ContainsKey(key)) // working around the parent context passing to the child bug
                    {
                        return new (ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)[0];
                    }
                }
                catch (KeyNotFoundException)
                {
                    return new (ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)[0];
                }

                return logsDictionary[key];
            }

            void RemoveLogsCacheItem(ISpan span)
            {
                string key;
                try
                {
                    key = span.GetBaggageItem(LogsKey);
                    logsDictionary.TryRemove(key, out _);
                }
                catch (KeyNotFoundException)
                {
                }
            }

            void SaveLogEventForLater(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)
            {
                string key;
                try
                {
                    key = span.GetBaggageItem(LogsKey);
                }
                catch (KeyNotFoundException)
                {
                    key = Guid.NewGuid().ToString();
                    span.SetBaggageItem(LogsKey, key);
                }

                if (!logsDictionary.TryGetValue(key, out var list))
                {
                    list = logsDictionary[key] = new List<(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)>();
                }

                list.Add((span, operationName, timestamp, logKeyValues));
            }

            // TODO: Much better for performance if we are evaluating this on a timer against sorted collection of known active items with start times rather than kicking off a task for each
            // doing this just for testing b/c easy to implement-ish
            ConcurrentDictionary<string, (CancellationTokenSource, Task)> emitWhenTooLongTasks = new ConcurrentDictionary<string, (CancellationTokenSource, Task)>();
            void KickOffEmitOnTooLongTask(ISpan span)
            {
                var key = Guid.NewGuid().ToString();
                var cts = new CancellationTokenSource();
                var task = Task.Run(
                    async () =>
                    {
                        await Task.Delay(emissionTime, cts.Token);
                        TriggerParentsAndSelfEmission(span);
                    });
                emitWhenTooLongTasks[key] = (cts, task);
                span.SetBaggageItem(EmitWhenTooLongTaskKey, key);
            }
            
            void CancelEmitOnTooLongTask(ISpan span)
            {
                var key = span.GetBaggageItem(EmitWhenTooLongTaskKey);
                // Just cleaning up garbage
                if (!emitWhenTooLongTasks.TryRemove(key, out (CancellationTokenSource, Task) data))
                {
                    // BUG: Fody seems to call Finish() twice. Omitting throw for now
                    return;
                    throw new InvalidOperationException("Some instrumentation is not instrumented as it should be");
                }

                data.Item1.Cancel();
                data.Item1.Dispose();
                // we just orphan the task <<< given we just orphan them, there's no need for us to hold onto them in the dictionary. oh well
                ////_ = data.Item2;
            }

            return new TracerDecoratorBuilder(underlying)
                .OnSpanFinished(
                    (span, operationName) =>
                    {
                        // Just cleaning up garbage
                        RemoveParentSpanFromBaggageForCleanup(span);
                        RemoveSetTagsCacheItem(span);
                        RemoveLogsCacheItem(span);
                        CancelEmitOnTooLongTask(span);
                    })
                .OnSpanStarted(
                    (span, operationName) =>
                    {
                        // otherwise it'll inherit from the parent
                        span.SetBaggageItem(SetTagsKey, span.Context.SpanId); // just trying to 'clear' it from parent #sigh
                        span.SetBaggageItem(HasBeenEmittedKey, false.ToString());
                        // TODO: BUG/COMPLETENESS - Actually who the parent is also depends on references that have been added
                        StoreParentSpanOnBaggage(span, underlying.ScopeManager?.Active?.Span);
                    })
                .OnSpanActivated(
                    (span, operationName) =>
                    {
                        // Kick off 'maybe emit later' tasks
                        span.SetBaggageItem(WhenActivatedKey, DateTimeOffset.Now.ToString("O"));
                        KickOffEmitOnTooLongTask(span);

                        // moved from OnSpanStarted since each child was having the same name - we must start before copying context?
                        span.SetBaggageItem(SpanNameKey, operationName);
                    })
                .OnSpanSetTag(
                    (span, operationName, tagKeyValue) =>
                    {
                        if (ShouldTriggerEmission(span, operationName, tagKeyValue))
                        {
                            TriggerParentsAndSelfEmission(span);
                            EmitSetTagEvent(span, operationName, tagKeyValue);
                            return;
                        }

                        if (HasBeenEmitted(span))
                        {
                            EmitSetTagEvent(span, operationName, tagKeyValue);
                        }
                        else
                        {
                            SaveSetTagEventForLater(span, operationName, tagKeyValue);
                        }
                    })
                .OnSpanLog(
                    (span, operationName, timestamp, logKeyValues) =>
                    {
                        if (ShouldLogTriggerEmission(span, operationName, timestamp, logKeyValues))
                        {
                            TriggerParentsAndSelfEmission(span);
                            EmitLogEvent(span, operationName, timestamp, logKeyValues);
                            return;
                        }

                        if (HasBeenEmitted(span))
                        {
                            EmitLogEvent(span, operationName, timestamp, logKeyValues);
                        }
                        else
                        {
                            SaveLogEventForLater(span, operationName, timestamp, logKeyValues);
                        }
                    })
                .Build();
        }
    }

    /////// <summary>
    ///////     We take ownership of talking to your scope manager - do not touch it yourself please!
    /////// </summary>
    ////internal sealed class DebounceTracer : StronglyTypedTracer<DebounceSpanBuilder, DebounceSpanContext, IScopeManager, DebounceSpan>
    ////{
    ////    private readonly ITracer wrapTarget;

    ////    public DebounceTracer(ITracer wrapTarget)
    ////    {
    ////        this.wrapTarget = wrapTarget;
    ////    }

    ////    public override void Inject<TCarrier>(DebounceSpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
    ////    {
    ////        // It is going across the wire, so we force emission of it and it's parents
    ////        spanContext.TriggerEmission();
    ////        this.wrapTarget.Inject(spanContext.WrappedSpanContext, format, carrier);
    ////    }

    ////    public override DebounceSpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
    ////    {
    ////        ISpanContext spanContext = this.wrapTarget.Extract(format, carrier);
    ////        return new DebounceSpanContext(spanContext);
    ////    }

    ////    public override IScopeManager ScopeManager => wrapTarget.ScopeManager;

    ////    public override DebounceSpanBuilder BuildSpan(string operationName)
    ////    {
    ////        return new DebounceSpanBuilder(operationName);
    ////    }

    ////    public override DebounceSpan ActiveSpan => (DebounceSpan) this.ScopeManager.Active?.Span;
    ////}

    ////internal class DebounceScope : StronglyTypedScope<DebounceSpan>
    ////{
    ////    public DebounceScope(ISpan wrappedSpan, bool finishSpanOnDispose)
    ////    {
    ////        throw new NotImplementedException();
    ////    }
    ////}

    ////internal class DebounceSpan : StronglyTypedSpan<DebounceSpan, DebounceSpanContext>
    ////{
    ////    public ISpan WrappedSpan { get; set; }
    ////}

    ////internal class DebounceSpanContext : StronglyTypedSpanContext
    ////{
    ////    public DebounceSpanContext(ISpanContext spanContext)
    ////    {
    ////        throw new NotImplementedException();
    ////    }

    ////    public void TriggerEmission()
    ////    {
    ////        throw new NotImplementedException();
    ////    }

    ////    public ISpanContext WrappedSpanContext { get; set; }
    ////}

    ////internal class DebounceSpanBuilder : StronglyTypedSpanBuilder<DebounceSpanBuilder, DebounceSpanContext, DebounceSpan, DebounceScope>
    ////{
    ////    public DebounceSpanBuilder(string operationName)
    ////    {
    ////        throw new NotImplementedException();
    ////    }
    ////}
}
