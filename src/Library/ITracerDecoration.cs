using OpenTracing.Contrib.Decorators;

namespace OpenTracing.Contrib.LocalTracers
{
    /// <summary>
    /// Bridge between the delegate based mechanisms of <see cref="OpenTracing.Contrib.Decorators"/> and
    /// ObjectOriented classes
    /// </summary>
    public interface ITracerDecoration
    {
        OnSpanLog OnSpanLog { get; }
        OnSpanSetTag OnSpanSetTag { get; }
        OnSpanFinished OnSpanFinished { get; }
        OnSpanStarted OnSpanStarted { get; }
        OnSpanActivated OnSpanActivated { get; }
        OnSpanStartedWithFinishCallback OnSpanStartedWithFinishCallback { get; }
    }
}