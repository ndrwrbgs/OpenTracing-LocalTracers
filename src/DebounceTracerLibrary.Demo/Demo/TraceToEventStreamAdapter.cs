
namespace Demo
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using OneOf;

    using OpenTracing;
    using OpenTracing.Contrib.Decorators;

    public static class TraceToEventStreamAdapter
    {
        /// <summary>
        ///     Would be better if we didn't need an input tracer, but there isn't a BasicTracer
        ///     in OpenTracing for C# quite yet, and we need SOMEONE to be supplying us a working
        ///     <see cref="IScopeManager"/> and <see cref="ISpanContext"/>.
        /// </summary>
        public static (ITracer connectedTracer, IObservable<TraceEvent> eventStream) CreateTracerEventStreamPair(
            ITracer tracer)
        {
            ISubject<TraceEvent> subject = new Subject<TraceEvent>();

            var connectedTracer = new TracerDecoratorBuilder(tracer)
                .OnSpanActivated((span, operationName) => subject.OnNext(new TraceEvent.ActivatedEvent(span, operationName)))
                .OnSpanFinished((span, operationName) => subject.OnNext(new TraceEvent.FinishedEvent(span, operationName)))
                .OnSpanSetTag((span, operationName, tagKeyValue) => subject.OnNext(new TraceEvent.SetTagsEvent(span, operationName, tagKeyValue)))
                .OnSpanLog((span, operationName, timestamp, logKeyValues) => subject.OnNext(new TraceEvent.LogEvent(span, operationName, timestamp, logKeyValues)))
                //.OnSpanStarted((span, operationName) => new TraceEvent.StartedEvent())
                .Build();
            
            return (connectedTracer, eventStream: subject.AsObservable());
        }
    }

    public sealed class TraceEvent
        : OneOfBase<
            TraceEvent.SetTagsEvent,
            TraceEvent.LogEvent,
            TraceEvent.ActivatedEvent,
            TraceEvent.FinishedEvent>
    {

        #region Override as named methods

        public new void Switch(
            Action<SetTagsEvent> setTagsEvent,
            Action<LogEvent> logEvent,
            Action<ActivatedEvent> activatedEvent,
            Action<FinishedEvent> finishedEvent)
        {
            base.Switch(setTagsEvent, logEvent, activatedEvent, finishedEvent);
        }

        public new TResult Match<TResult>(
            Func<SetTagsEvent, TResult> setTagsEvent,
            Func<LogEvent, TResult> logEvent,
            Func<ActivatedEvent, TResult> activatedEvent,
            Func<FinishedEvent, TResult> finishedEvent)
        {
            return base.Match(setTagsEvent, logEvent, activatedEvent, finishedEvent);
        }

        public bool TryPickSetTagsEvent(out SetTagsEvent value, out OneOf<LogEvent, ActivatedEvent, FinishedEvent> remainder)
        {
            return base.TryPickT0(out value, out remainder);
        }

        public bool TryPickLogEvent(out LogEvent value, out OneOf<SetTagsEvent, ActivatedEvent, FinishedEvent> remainder)
        {
            return base.TryPickT1(out value, out remainder);
        }

        public bool TryPickActivatedEvent(out ActivatedEvent value, out OneOf<SetTagsEvent, LogEvent, FinishedEvent> remainder)
        {
            return base.TryPickT2(out value, out remainder);
        }

        public bool TryPickFinishedEvent(out FinishedEvent value, out OneOf<SetTagsEvent, LogEvent, ActivatedEvent> remainder)
        {
            return base.TryPickT3(out value, out remainder);
        }

        public bool IsSetTagsEvent => base.IsT0;

        public SetTagsEvent AsSetTagsEvent => base.AsT0;

        public bool IsLogEvent => base.IsT1;

        public LogEvent AsLogEvent => base.AsT1;

        public bool IsActivatedEvent => base.IsT2;

        public ActivatedEvent AsActivatedEvent => base.AsT2;

        public bool IsFinishedEvent => base.IsT3;

        public FinishedEvent AsFinishedEvent => base.AsT3;

        #endregion


        public TraceEvent(int index, SetTagsEvent value0 = null, LogEvent value1 = null, ActivatedEvent value2 = null, FinishedEvent value3 = null)
            : base(index, value0, value1, value2, value3) { }

        public sealed class SetTagsEvent
        {
            public ISpan Span { get; }
            public string OperationName { get; }
            public TagKeyValue TagKeyValue { get; }

            public SetTagsEvent(ISpan span, string operationName, TagKeyValue tagKeyValue)
            {
                this.Span = span;
                this.OperationName = operationName;
                this.TagKeyValue = tagKeyValue;
            }

            public static implicit operator TraceEvent(SetTagsEvent self)
            {
                return new TraceEvent(0, self, default, default, default);
            }
        }

        public sealed class LogEvent
        {
            public ISpan Span { get; }
            public string OperationName { get; }
            public DateTimeOffset Timestamp { get; }
            public LogKeyValue[] LogKeyValues { get; }

            public LogEvent(ISpan span, string operationName, DateTimeOffset timestamp, LogKeyValue[] logKeyValues)
            {
                this.Span = span;
                this.OperationName = operationName;
                this.Timestamp = timestamp;
                this.LogKeyValues = logKeyValues;
            }

            public static implicit operator TraceEvent(LogEvent self)
            {
                return new TraceEvent(1, default, self, default, default);
            }
        }

        public sealed class ActivatedEvent
        {
            public ISpan Span { get; }
            public string OperationName { get; }

            public ActivatedEvent(ISpan span, string operationName)
            {
                this.Span = span;
                this.OperationName = operationName;
            }

            public static implicit operator TraceEvent(ActivatedEvent self)
            {
                return new TraceEvent(2, default, default, self, default);
            }
        }

        public sealed class FinishedEvent
        {
            public ISpan Span { get; }
            public string OperationName { get; }

            public FinishedEvent(ISpan span, string operationName)
            {
                this.Span = span;
                this.OperationName = operationName;
            }

            public static implicit operator TraceEvent(FinishedEvent self)
            {
                return new TraceEvent(3, default, default, default, self);
            }
        }
    }
}