namespace OpenTracing.Contrib.LocalTracers.Config.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OpenTracing.Contrib.LocalTracers.Config.Builder.Console;
    using OpenTracing.Contrib.LocalTracers.Config.Console;

    public sealed class ConsoleConfigurationBuilder
    {
        private readonly IList<Action<BasicConsoleConfiguration>> factoryMethods;

        internal ConsoleConfigurationBuilder(
            IList<Action<BasicConsoleConfiguration>> factoryMethods = null)
        {
            this.factoryMethods = factoryMethods ?? new List<Action<BasicConsoleConfiguration>>();
        }

        public IConsoleConfiguration Build()
        {
            var defaultValue = CreateDefaultConfiguration();

            var value = defaultValue;
            foreach (var modifier in this.factoryMethods)
            {
                modifier(value);
            }

            return value;
        }

        private static BasicConsoleConfiguration CreateDefaultConfiguration()
        {
            return new BasicConsoleConfiguration(
                false,
                ColorMode.BasedOnCategory,
                string.Empty,
                new BasicPerTraceCategoryConfiguration<bool>(
                    false,
                    false,
                    false,
                    false),
                new BasicDataSerializationConfiguration(
                    SetTagDataSerialization.Simple,
                    LogDataSerialization.Simple),
                new BasicPerTraceCategoryConfiguration<ConsoleColor>(
                    ConsoleColor.Gray,
                    ConsoleColor.Gray,
                    ConsoleColor.Gray,
                    ConsoleColor.Gray));
        }

        internal ConsoleConfigurationBuilder WithEnabled(bool enabled)
        {
            return this.With(
                config => config.Enabled = enabled);
        }

        public ConsoleConfigurationBuilder WithColorMode(ColorMode colorMode)
        {
            return this.With(
                config => config.ColorMode = colorMode);
        }

        private ConsoleConfigurationBuilder With(Action<BasicConsoleConfiguration> action)
        {
            return new ConsoleConfigurationBuilder(
                this.factoryMethods.Concat(
                        new[]
                        {
                            action
                        })
                    .ToList());
        }

        public ConsoleConfigurationBuilder WithFormat(string format)
        {
            return this.With(
                config => config.Format = format);
        }

        public ConsoleConfigurationBuilder WithOutputSpanNameOnCategory(
            bool? activated = null,
            bool? finished = null,
            bool? setTag = null,
            bool? log = null)
        {
            var ret = this;
            if (activated != null)
            {
                ret = ret.With(
                    config => config.OutputSpanNameOnCategory.Activated = activated.Value);
            }

            if (finished != null)
            {
                ret = ret.With(
                    config => config.OutputSpanNameOnCategory.Finished = finished.Value);
            }

            if (setTag != null)
            {
                ret = ret.With(
                    config => config.OutputSpanNameOnCategory.SetTag = setTag.Value);
            }

            if (log != null)
            {
                ret = ret.With(
                    config => config.OutputSpanNameOnCategory.Log = log.Value);
            }

            return ret;
        }

        public ConsoleConfigurationBuilder WithDataSerialization(
            SetTagDataSerialization? setTag = null,
            LogDataSerialization? log = null)
        {
            var ret = this;
            if (setTag != null)
            {
                ret = ret.With(
                    config => config.DataSerialization.SetTag = setTag.Value);
            }

            if (log != null)
            {
                ret = ret.With(
                    config => config.DataSerialization.Log = log.Value);
            }

            return ret;
        }

        public ConsoleConfigurationBuilder WithColorsForTheBasedOnCategoryColorMode(
            ConsoleColor? activated = null,
            ConsoleColor? finished = null,
            ConsoleColor? setTag = null,
            ConsoleColor? log = null)
        {
            var ret = this;
            if (activated != null)
            {
                ret = ret.With(
                    config => config.ColorsForTheBasedOnCategoryColorMode.Activated = activated.Value);
            }

            if (finished != null)
            {
                ret = ret.With(
                    config => config.ColorsForTheBasedOnCategoryColorMode.Finished = finished.Value);
            }

            if (setTag != null)
            {
                ret = ret.With(
                    config => config.ColorsForTheBasedOnCategoryColorMode.SetTag = setTag.Value);
            }

            if (log != null)
            {
                ret = ret.With(
                    config => config.ColorsForTheBasedOnCategoryColorMode.Log = log.Value);
            }

            return ret;
        }
    }
}