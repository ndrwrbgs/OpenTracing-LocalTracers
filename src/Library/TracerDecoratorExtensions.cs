using JetBrains.Annotations;
using OpenTracing.Contrib.Decorators;

namespace OpenTracing.Contrib.LocalTracers
{
    /// <summary>
    /// Extensions for helping work with <see cref="TracerDecoratorBuilder"/> and <see cref="ITracerDecoration"/>
    /// </summary>
    [PublicAPI]
    public static class TracerDecoratorExtensions
    {
        [NotNull]
        public static ITracer Decorate(
            [NotNull] this ITracer source,
            [CanBeNull] ITracerDecoration decoration)
        {
            if (decoration == null)
            {
                return source;
            }

            var builder = new TracerDecoratorBuilder(source);

            // Checking for null to avoid adding them if they're not implemented since the underlying library has optimizations if not subscribed
            if (decoration.OnSpanStarted != null)
            {
                builder = builder.OnSpanStarted(decoration.OnSpanStarted);
            }

            if (decoration.OnSpanFinished != null)
            {
                builder = builder.OnSpanFinished(decoration.OnSpanFinished);
            }

            if (decoration.OnSpanLog != null)
            {
                builder = builder.OnSpanLog(decoration.OnSpanLog);
            }

            if (decoration.OnSpanSetTag != null)
            {
                builder = builder.OnSpanSetTag(decoration.OnSpanSetTag);
            }

            if (decoration.OnSpanActivated != null)
            {
                builder = builder.OnSpanActivated(decoration.OnSpanActivated);
            }

            if (decoration.OnSpanStartedWithFinishCallback != null)
            {
                builder = builder.OnSpanStartedWithCallback(decoration.OnSpanStartedWithFinishCallback);
            }

            return builder
                .Build();
        }
    }
}