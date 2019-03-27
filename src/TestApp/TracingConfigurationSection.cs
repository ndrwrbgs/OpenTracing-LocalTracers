using System.Configuration;
using OpenTracing.Contrib.LocalTracers.Config.Console;
using OpenTracing.Contrib.LocalTracers.Config.File;

namespace TestApp
{
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