namespace OpenTracing.Contrib.LocalTracers
{
    using OpenTracing.Contrib.Decorators;

    /// <summary>
    /// Use factory instead of type directly to hide the concrete type
    /// and encourage programming to the interface
    /// </summary>
    internal static class NoopTracerDecorationFactory
    {
        public static ITracerDecoration Instance = new NoopTracerDecoration();

        private sealed class NoopTracerDecoration : ITracerDecoration
        {
            public OnSpanLog OnSpanLog { get; } = null;
            public OnSpanSetTag OnSpanSetTag { get; } = null;
            public OnSpanFinished OnSpanFinished { get; } = null;
            public OnSpanStarted OnSpanStarted { get; } = null;
            public OnSpanActivated OnSpanActivated { get; } = null;
            public OnSpanStartedWithFinishCallback OnSpanStartedWithFinishCallback { get; } = null;
        }
    }
}