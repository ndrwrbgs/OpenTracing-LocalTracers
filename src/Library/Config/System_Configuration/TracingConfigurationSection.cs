namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration
{
    using System.Configuration;

    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Console;
    using OpenTracing.Contrib.LocalTracers.Config.System_Configuration.File;

    public class TracingConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("console", IsRequired = true)]
        public ConsoleElement Console
        {
            get { return (ConsoleElement) base["console"]; }
        }

        [ConfigurationProperty("file")]
        public FileElement File => (FileElement) base["file"];
    }
}