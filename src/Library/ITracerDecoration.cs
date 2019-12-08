namespace OpenTracing.Contrib.LocalTracers
{
    using OpenTracing.Contrib.Decorators;

    /// <summary>
    /// Bridge between the delegate based mechanisms of <see cref="OpenTracing.Contrib.Decorators"/> and
    /// ObjectOriented classes
    /// </summary>
    internal interface ITracerDecoration
    {
        OnSpanLog OnSpanLog { get; }
        OnSpanSetTag OnSpanSetTag { get; }
        OnSpanFinished OnSpanFinished { get; }
        OnSpanStarted OnSpanStarted { get; }
        OnSpanActivated OnSpanActivated { get; }
        OnSpanStartedWithFinishCallback OnSpanStartedWithFinishCallback { get; }
    }
}