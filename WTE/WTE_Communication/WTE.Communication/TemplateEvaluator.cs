using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WTE.Communication
{
    /// <summary>
    /// Class for loading a template and performing tag replacements
    /// Tags are in the format of {{[tag]}}
    /// 
    /// Ex:
    /// 
    /// TemplateEvaluator template = new TemplateEvaluator();
    /// 
    /// Dictionary<string, object> fields = new Dictionary<string, object>();
    /// 
    /// fields.Add("from", fromAddress.EMail);
    /// 
    /// fields.Add("to", toAddress.EMail);
    /// 
    /// StringReader sr = new StringReader("<html><body><h1>Communication test template.</h1><br /> Email sent <br />from: {{from}} <br />to: {{to}}.<br /></body></html>");
    /// 
    /// template.Load(sr);
    /// 
    /// string body = template.Eval(TemplateFieldAccessors.DictionaryAccessor(fields));
    /// 
    /// </summary>
    public sealed class TemplateEvaluator
    {
        #region private fields
        /// <summary>
        /// Regex for finding tags in template. Tags are in the format {{[tag]}}
        /// </summary>
        private static Regex _rxFields = new Regex(
            @"\{{2}\s*(?<field>((?<parts>\w+)\.)*(?<parts>\w+))(?([,:])(?'format'[,:][^\]]+))\s*\}{2}",
            RegexOptions.IgnoreCase |
            RegexOptions.ExplicitCapture |
            RegexOptions.Compiled);

        private string _source = string.Empty;
        private TemplateWriter _writer = DefaultWriter;
        private List<string> _fieldNames = null;

        #endregion private fields

        #region public interface

        /// <summary>
        /// Default constructor
        /// </summary>
        public TemplateEvaluator() { }

        /// <summary>
        /// Create evaluator from source text and parse it in preparation for tag replacement
        /// </summary>
        /// <param name="source"></param>
        public TemplateEvaluator(string source)
        {
            _source = source;
            Parse();
        }

        /// <summary>
        /// The template source
        /// </summary>
        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                Parse();
            }
        }

        /// <summary>
        /// Loads a template from a file.
        /// </summary>
        /// <param name="fileName">The full path to the file containing the template source.</param>
        public void Load(string fileName)
        {
            using (StreamReader reader = File.OpenText(fileName))
            {
                Load(reader);
            }
        }

        /// <summary>
        /// load a template from a text reader
        /// </summary>
        /// <param name="reader"></param>
        public void Load(TextReader reader)
        {
            _source = reader.ReadToEnd();
            Parse();
        }

        /// <summary>
        /// list of tags found in template
        /// </summary>
        public IList<string> FieldNames { get { return _fieldNames as IList<string>; } }

        /// <summary>
        /// Evaluate parsed template with TemplateFieldAccessor and return result
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns></returns>
        public string Eval(TemplateFieldAccessor accessor)
        {
            StringWriter sw = new StringWriter();
            _writer(sw, accessor);
            return sw.ToString();
        }

        /// <summary>
        /// Evaluate parsed template with TemplateFieldAccessor and return result in writer parameter
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="accessor"></param>
        public void Eval(TextWriter writer, TemplateFieldAccessor accessor)
        {
            _writer(writer, accessor);
        }

        #endregion public interface

        #region private helpers

        /// <summary>
        /// Parses the template using a regular expression and converts it
        /// to a series of delegate calls that render the template. After this
        /// method is executed the member variable _writer will reference the
        /// root level writer that renders the template.
        /// </summary>
        private void Parse()
        {
            MatchCollection sections = _rxFields.Matches(_source);
            Dictionary<string, TemplateWriter> writerLookup = new Dictionary<string, TemplateWriter>();
            List<TemplateWriter> writerList = new List<TemplateWriter>();
            _fieldNames = new List<string>(sections.Count);

            int curIndex = 0;
            foreach (Match m in sections)
            {
                _fieldNames.Add(m.Groups["field"].Value);

                if (m.Index > curIndex)
                {
                    // create a literal from curIndex to m.Index
                    string literal = _source.Substring(curIndex, m.Index - curIndex);
                    writerList.Add(LiteralWriter(literal));
                }

                string field = m.Value;
                TemplateWriter writer;
                if (!writerLookup.TryGetValue(field, out writer))
                {
                    CaptureCollection parts = m.Groups["parts"].Captures;
                    string format = m.Groups["format"].Value.Trim();
                    if (string.IsNullOrEmpty(format))
                        writer = FieldWriter(parts);
                    else
                    {
                        format = "{0" + format + "}";
                        writer = FieldWriter(parts, format);
                    }
                    writerLookup.Add(field, writer);
                }
                writerList.Add(writer);

                curIndex = m.Index + m.Length;
            }

            if (curIndex < _source.Length)
            {
                // generate a literal for the remaining text
                string literal = _source.Substring(curIndex);
                writerList.Add(LiteralWriter(literal));
            }

            // Create an aggregate writer that calls all the
            // writers that make up the template
            TemplateWriter[] writers = writerList.ToArray();
            _writer = delegate(TextWriter textWriter, TemplateFieldAccessor root)
            {
                for (int i = 0; i < writers.Length; i++)
                    writers[i](textWriter, root);
            };
        }

        /// <summary>
        /// Creates a delegate that writes a literal string to a TextWriter
        /// </summary>
        /// <param name="literal">The literal string to write.</param>
        /// <returns>A new writer delegate.</returns>
        private static TemplateWriter LiteralWriter(string literal)
        {
            return delegate(TextWriter writer, TemplateFieldAccessor root)
            {
                writer.Write(literal);
            };
        }

        /// <summary>
        /// Creates a delegate that writes the value of a template field to a TextWriter
        /// </summary>
        /// <param name="parts">The parsed parts of the field name.</param>
        /// <param name="format">The format to be used when converting the field value to a string.</param>
        /// <returns>A new writer delegate.</returns>
        private static TemplateWriter FieldWriter(CaptureCollection parts, string format)
        {
            return delegate(TextWriter writer, TemplateFieldAccessor root)
            {
                writer.Write(string.Format(format, GetFieldValue(root, parts)));
            };
        }

        private static TemplateWriter FieldWriter(CaptureCollection parts)
        {
            return delegate(TextWriter writer, TemplateFieldAccessor root)
            {
                object fieldValue = GetFieldValue(root, parts);
                if (fieldValue != null)
                    writer.Write(fieldValue.ToString());
            };
        }

        private static object GetFieldValue(TemplateFieldAccessor root, CaptureCollection parts)
        {
            object value;
            TemplateFieldAccessor accessor = root;
            for (int i = 0; i < parts.Count - 1; i++)
            {
                Capture part = parts[i];
                value = accessor(part.Value);

                // if the accessor didnt return an accessor then
                // wrap the value in an object accessor
                if (!(value is TemplateFieldAccessor))
                    value = TemplateFieldAccessors.PropertyAccessor(value);

                accessor = (TemplateFieldAccessor)value;
            }

            value = accessor(parts[parts.Count - 1].Value);

            return value;
        }

        /// <summary>
        /// Represents a function that renders a compiled template.
        /// </summary>
        /// <param name="writer">The writer to render the template to.</param>
        /// <param name="root">A delegate used to retrieve field values.</param>
        private delegate void TemplateWriter(TextWriter writer, TemplateFieldAccessor root);

        private static void DefaultWriter(TextWriter writer, TemplateFieldAccessor accessor) { }

        #endregion private helpers
    }
}