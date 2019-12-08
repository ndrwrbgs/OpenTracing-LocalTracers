namespace OpenTracing.Contrib.LocalTracers.Config.Builder.Console
{
    using OpenTracing.Contrib.LocalTracers.Config.Console;

    internal sealed class BasicPerTraceCategoryConfiguration<T> : IPerTraceCategoryConfiguration<T>
    {
        public BasicPerTraceCategoryConfiguration(T activated, T finished, T tag, T log)
        {
            this.Activated = activated;
            this.Finished = finished;
            this.SetTag = tag;
            this.Log = log;
        }

        public T Activated { get; internal set; }
        public T Finished { get; internal set; }
        public T SetTag { get; internal set; }
        public T Log { get; internal set; }
    }
}