namespace OpenTracing.Contrib.LocalTracers.Console
{
    using System;
    using System.Text;

    using OpenTracing.Contrib.Decorators;

    /// <summary>
    /// A basic <see cref="ITracerDecoration"/> class with an attempt to keep everything configurable as input.
    /// Compresses the 4 decoration calls into one <see cref="Write"/> call by using a <see cref="OutputCategory"/>
    /// </summary>
    internal sealed class ColoredConsoleTracerDecoration : ITracerDecoration
    {
        public delegate string LogSerializer(LogKeyValue[] fields);

        public delegate string SetTagSerializer(TagKeyValue value);

        public delegate ConsoleColor ColorChooser(ISpan span, string operationName, OutputCategory category);

        public delegate StringBuilder TextFormatter(string spanId, string operationName, OutputCategory category, string outputText);

        private readonly ColorChooser colorChooser;
        private readonly LogSerializer logSerializer;
        private readonly SetTagSerializer setTagSerializer;
        private readonly TextFormatter textFormatter;

        private readonly object consoleLock = new object();

        public ColoredConsoleTracerDecoration(ColorChooser colorChooser, LogSerializer logSerializer, TextFormatter textFormatter, SetTagSerializer setTagSerializer)
        {
            this.colorChooser = colorChooser;
            this.logSerializer = logSerializer;
            this.textFormatter = textFormatter;
            this.setTagSerializer = setTagSerializer;
        }

        internal enum OutputCategory
        {
            Log,
            SetTag,
            Activated,
            Finished
        }

        OnSpanLog ITracerDecoration.OnSpanLog =>
            (span, operationName, timestamp, fields) => { this.Write(span, operationName, OutputCategory.Log, this.logSerializer(fields)); };

        OnSpanSetTag ITracerDecoration.OnSpanSetTag =>
            (span, operationName, value) => { this.Write(span, operationName, OutputCategory.SetTag, this.setTagSerializer(value)); };

        OnSpanFinished ITracerDecoration.OnSpanFinished =>
            (span, operationName) => { this.Write(span, operationName, OutputCategory.Finished, null); };

        OnSpanActivated ITracerDecoration.OnSpanActivated =>
            (span, operationName) => { this.Write(span, operationName, OutputCategory.Activated, null); };

        OnSpanStarted ITracerDecoration.OnSpanStarted => null; // We only care about Activated
        OnSpanStartedWithFinishCallback ITracerDecoration.OnSpanStartedWithFinishCallback => null;

        private void Write(ISpan span, string operationName, OutputCategory category, string outputText)
        {
            ConsoleColor foregroundColor = this.colorChooser(span, operationName, category);
            var spanId = span.Context.SpanId;

            var formattedText = this.textFormatter(spanId, operationName, category, outputText);

            lock (this.consoleLock)
            {
                var prev = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
                try
                {
                    Console.WriteLine(formattedText);
                }
                finally
                {
                    Console.ForegroundColor = prev;
                }
            }
        }
    }
}