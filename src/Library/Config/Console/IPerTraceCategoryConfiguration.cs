namespace OpenTracing.Contrib.LocalTracers.Config.Console
{
    public interface IPerTraceCategoryConfiguration<T>
    {
        T Activated { get; }
        T Finished { get; }
        T SetTag { get; }
        T Log { get; }
    }
}