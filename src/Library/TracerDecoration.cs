namespace OpenTracing.Contrib.LocalTracers
{
    using OpenTracing.Contrib.Decorators;

    /// <summary>
    /// Sealed class instead of interface so that we can hide the implementation of <see cref="ITracerDecoration"/>
    /// and reduce public API surface area that would need to change with releases.
    ///
    /// Does not expose ctor or any methods/fields, but does permit us to define extension methods internally
    /// </summary>
    public sealed class TracerDecoration : ITracerDecoration
    {
        private readonly OnSpanLog onSpanLog;
        private readonly OnSpanSetTag onSpanSetTag;
        private readonly OnSpanFinished onSpanFinished;
        private readonly OnSpanStarted onSpanStarted;
        private readonly OnSpanActivated onSpanActivated;
        private readonly OnSpanStartedWithFinishCallback onSpanStartedWithFinishCallback;

        OnSpanLog ITracerDecoration.OnSpanLog => this.onSpanLog;

        OnSpanSetTag ITracerDecoration.OnSpanSetTag => this.onSpanSetTag;

        OnSpanFinished ITracerDecoration.OnSpanFinished => this.onSpanFinished;

        OnSpanStarted ITracerDecoration.OnSpanStarted => this.onSpanStarted;

        OnSpanActivated ITracerDecoration.OnSpanActivated => this.onSpanActivated;

        OnSpanStartedWithFinishCallback ITracerDecoration.OnSpanStartedWithFinishCallback => this.onSpanStartedWithFinishCallback;

        internal TracerDecoration(
            OnSpanLog onSpanLog,
            OnSpanSetTag onSpanSetTag,
            OnSpanFinished onSpanFinished,
            OnSpanStarted onSpanStarted,
            OnSpanActivated onSpanActivated,
            OnSpanStartedWithFinishCallback onSpanStartedWithFinishCallback)
        {
            this.onSpanLog = onSpanLog;
            this.onSpanSetTag = onSpanSetTag;
            this.onSpanFinished = onSpanFinished;
            this.onSpanStarted = onSpanStarted;
            this.onSpanActivated = onSpanActivated;
            this.onSpanStartedWithFinishCallback = onSpanStartedWithFinishCallback;
        }
    }
}