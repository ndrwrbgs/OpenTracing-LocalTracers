using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing.Contrib.Decorators;

namespace OpenTracing.Contrib.LocalTracers.File
{
    internal sealed class CsvFileTracerDecoration : ITracerDecoration
    {
        public delegate void WriteToFile(string maybeInvalidFileName, string text);

        private readonly WriteToFile writeToFile;

        public CsvFileTracerDecoration(
            WriteToFile writeToFile)
        {
            this.writeToFile = writeToFile;
        }
            
        OnSpanLog ITracerDecoration.OnSpanLog =>
            (span, operationName, timestamp, fields) =>
            {
                this.WriteLine(
                    span,
                    "Log",
                    SeparateSpanLogFieldsForCsv(fields));
            };

        OnSpanSetTag ITracerDecoration.OnSpanSetTag =>
            (span, operationName, value) =>
            {
                this.WriteLine(
                    span,
                    "Tag",
                    value.key,
                    value.value?.ToString());
            };

        OnSpanFinished ITracerDecoration.OnSpanFinished =>
            (span, operationName) =>
            {
                this.WriteLine(
                    span,
                    "Finish");
            };

        OnSpanActivated ITracerDecoration.OnSpanActivated =>
            (span, operationName) =>
            {
                this.WriteLine(
                    span,
                    "Start",
                    operationName);
            };

        OnSpanStarted ITracerDecoration.OnSpanStarted => null;
        OnSpanStartedWithFinishCallback ITracerDecoration.OnSpanStartedWithFinishCallback => null;

        private static string[] SeparateSpanLogFieldsForCsv(LogKeyValue[] fields)
        {
            var fieldsSeparatedForCsv = new string[fields.Length * 2];
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                fieldsSeparatedForCsv[index * 2] = field.key;
                fieldsSeparatedForCsv[index * 2 + 1] = field.value?.ToString();
            }

            return fieldsSeparatedForCsv;
        }

        private void WriteLine(
            ISpan span,
            string type,
            params string[] items)
        {
            string forTraceId = span.Context.TraceId;
            string fileName = forTraceId + ".csv";

            string fullLine = this.GetSerializedOutput(span, type, items);

            this.writeToFile(fileName, fullLine);
        }

        private string GetSerializedOutput(ISpan span, string type, string[] items)
        {
            string spanId = span.Context.SpanId;

            IList<string> allItems = new List<string>();
            allItems.Add(DateTime.Now.ToString("O"));
            allItems.Add(spanId);

            allItems.Add(type);

            foreach (var item in items) allItems.Add(item);

            string fullLine = String.Join(
                ",",
                allItems.Select(CsvEscape));
            return fullLine;
        }

        private static string CsvEscape(string item)
        {
            // hi -> hi
            // foo,"bar -> "foo,""bar"
            if (item.Contains(",") || item.Contains("\"") || item.Contains("\n"))
            {
                // Contains special character, escape double quotes and surround with quotes
                return "\"" + item.Replace("\"", "\"\"") + "\"";
            }

            return item;
        }
    }
}