namespace OpenTracing.Contrib.LocalTracers
{
    using JetBrains.Annotations;

    using OpenTracing.Contrib.Decorators;

    /// <summary>
    /// Extensions for helping work with <see cref="TracerDecoratorBuilder"/> and <see cref="ITracerDecoration"/>
    /// </summary>
    [PublicAPI]
    public static class TracerDecoratorExtensions
    {
        [NotNull]
        public static ITracer Decorate(
            [NotNull] this TracerDecoration source,
            [NotNull] ITracer target)
        {
            return Decorate(target, source);
        }

        [NotNull]
        public static ITracer Decorate(
            [NotNull] this ITracer source,
            [NotNull] TracerDecoration publicDecoration)
        {
            var decoration = publicDecoration as ITracerDecoration;

            var builder = new TracerDecoratorBuilder(source);

            // For performance, to avoid a level of indirection when not needed
            bool bypassBuilder = true;

            // Checking for null to avoid adding them if they're not implemented since the underlying library has optimizations if not subscribed
            if (decoration.OnSpanStarted != null)
            {
                builder = builder.OnSpanStarted(decoration.OnSpanStarted);
                bypassBuilder = false;
            }

            if (decoration.OnSpanFinished != null)
            {
                builder = builder.OnSpanFinished(decoration.OnSpanFinished);
                bypassBuilder = false;
            }

            if (decoration.OnSpanLog != null)
            {
                builder = builder.OnSpanLog(decoration.OnSpanLog);
                bypassBuilder = false;
            }

            if (decoration.OnSpanSetTag != null)
            {
                builder = builder.OnSpanSetTag(decoration.OnSpanSetTag);
                bypassBuilder = false;
            }

            if (decoration.OnSpanActivated != null)
            {
                builder = builder.OnSpanActivated(decoration.OnSpanActivated);
                bypassBuilder = false;
            }

            if (decoration.OnSpanStartedWithFinishCallback != null)
            {
                builder = builder.OnSpanStartedWithCallback(decoration.OnSpanStartedWithFinishCallback);
                bypassBuilder = false;
            }

            if (bypassBuilder)
            {
                return source;
            }
            else
            {
                return builder
                    .Build();
            }
        }

        internal static TracerDecoration ToPublicType(this ITracerDecoration interfaceType)
        {
            return new TracerDecoration(
                interfaceType.OnSpanLog,
                interfaceType.OnSpanSetTag,
                interfaceType.OnSpanFinished,
                interfaceType.OnSpanStarted,
                interfaceType.OnSpanActivated,
                interfaceType.OnSpanStartedWithFinishCallback);
        }
    }
}