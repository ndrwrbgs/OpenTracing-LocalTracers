namespace OpenTracing.Contrib.LocalTracers.Config.System_Configuration.Utils
{
    using System;
    using System.Configuration;
    using System.Xml;

    using JetBrains.Annotations;

    [PublicAPI]
    public class ConfigurationTextElement<T> : ConfigurationElement
    {
        protected override void DeserializeElement(
            XmlReader reader,
            bool serializeCollectionKey)
        {
            if (typeof(T).IsEnum)
            {
                var strValue = (string) reader.ReadElementContentAs(typeof(string), null);
                this.Value = (T) Enum.Parse(typeof(T), strValue, ignoreCase: true);
                return;
            }

            this.Value = (T) reader.ReadElementContentAs(typeof(T), null);
        }

        public T Value { get; private set; }

        public override string ToString()
        {
            return this.Value?.ToString() ?? string.Empty;
        }

        public static implicit operator T(ConfigurationTextElement<T> elem)
        {
            return elem.Value;
        }
    }
}