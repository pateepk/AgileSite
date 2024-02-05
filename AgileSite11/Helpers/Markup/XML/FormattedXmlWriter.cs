using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace CMS.Helpers
{
    /// <summary>
    /// Custom XML writer to nicely format the document
    /// </summary>
    internal class FormattedXmlWriter : XmlWriter
    {
        #region "Non-public static methods"

        /// <summary>
        /// Returns infinite collections of indent strings
        /// </summary>
        /// <remarks>This collection will never stop yielding next items, thus should only be used to obtain finite number of its items</remarks>
        /// <param name="indentChars">The character string to use when indenting. This can be set to any string value. However, to ensure valid XML, you should specify only valid white space characters, such as space characters, tabs, carriage returns, or line feeds. The default is two spaces.</param>
        protected static IEnumerable<string> GetInfiniteIndent(string indentChars)
        {
            while (true)
            {
                yield return indentChars;
            }
        }


        /// <summary>
        /// Ensures not-null XmlWriterSettings object is returned. If null settings are provided, result of <see cref="GetDefaultSettings"/> method is used.
        /// </summary>
        /// <param name="settings">Provided settings</param>
        /// <returns>XmlWriterSettings object, never null</returns>
        protected static XmlWriterSettings EnsureSettings(XmlWriterSettings settings)
        {
            return settings ?? GetDefaultSettings();
        }


        /// <summary>
        /// Returns true if provided text contains no line characters
        /// </summary>
        /// <param name="text">Text to check</param>
        protected static bool IsSingleLine(string text)
        {
            return text.IndexOf('\n') < 0;
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Gets the default writer settings
        /// </summary>
        public static XmlWriterSettings GetDefaultSettings()
        {
            return new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.Unicode
            };
        }

        #endregion


        #region "Variables"

        // Current indentation level
        private int indentLevel;


        // If true, CData block was written
        protected bool cDataWritten;


        // Underlying system XML writer
        protected readonly XmlWriter writer;


        private bool mIndentCData = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the CDATA elements are automatically indented to their own line
        /// </summary>
        public bool IndentCData
        {
            get
            {
                return mIndentCData;
            }
            set
            {
                mIndentCData = value;
            }
        }

        #endregion


        #region "Simple encapsulated properties"

        /// <summary>
        /// Specifies a set of features to support on the System.Xml.XmlWriter object created by the Overload:System.Xml.XmlWriter.Create method.
        /// </summary>
        public override XmlWriterSettings Settings
        {
            get
            {
                return writer.Settings;
            }
        }


        /// <summary>
        /// Gets the state of the writer.
        /// </summary>
        public override WriteState WriteState
        {
            get
            {
                return writer.WriteState;
            }
        }


        /// <summary>
        /// When overridden in a derived class, gets the current xml:lang scope.
        /// </summary>
        public override string XmlLang
        {
            get
            {
                return writer.XmlLang;
            }
        }


        /// <summary>
        /// When overridden in a derived class, gets an XmlSpace representing the current xml:space scope.
        /// </summary>
        public override XmlSpace XmlSpace
        {
            get
            {
                return writer.XmlSpace;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stringBuilder">Output string builder</param>
        /// <param name="settings">XML Writer settings</param>
        public FormattedXmlWriter(StringBuilder stringBuilder, XmlWriterSettings settings = null)
        {
            settings = EnsureSettings(settings);
            writer = Create(stringBuilder, settings);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="settings">XML Writer settings</param>
        public FormattedXmlWriter(System.IO.Stream stream, XmlWriterSettings settings = null)
        {
            settings = EnsureSettings(settings);
            writer = Create(stream, settings);
        }

        #endregion


        #region "Non-public methods"

        /// <summary>
        /// Based on settings increases current indention level.
        /// </summary>
        protected void IncreaseIndent()
        {
            if (!Settings.Indent)
            {
                return;
            }

            indentLevel++;
        }


        /// <summary>
        /// Based on settings decreases current indention level.
        /// </summary>
        protected void DecreaseIndent()
        {
            if (!Settings.Indent)
            {
                return;
            }

            indentLevel--;
        }


        /// <summary>
        /// Writes out the current indentation.
        /// </summary>
        protected virtual void WriteIndent()
        {
            if (!Settings.Indent)
            {
                return;
            }

            GetInfiniteIndent(Settings.IndentChars)
                .Take(indentLevel)
                .ToList()
                .ForEach(WriteRaw);
        }


        /// <summary>
        /// Writes out a new line.
        /// </summary>
        protected virtual void WriteNewLine()
        {
            WriteRaw(Environment.NewLine);            
        }


        /// <summary>
        /// Writes out the current indentation at the end of a CData block.
        /// </summary>
        private void WriteCDataIndentEnd()
        {
            if (!IndentCData || !cDataWritten)
            {
                return;
            }

            WriteNewLine();
            WriteIndent();
            cDataWritten = false;
        }
        
        #endregion


        #region "Custom methods"

        /// <summary>
        /// Writes out a <![CDATA[...]]> block containing the specified text.
        /// </summary>
        /// <param name="text">The text to place inside the CDATA block.</param>
        public override void WriteCData(string text)
        {
            if (IndentCData)
            {
                WriteNewLine();
                if (IsSingleLine(text))
                {
                    // Apply indentation before CDATA section only for single-line text
                    WriteIndent();
                }
                else
                {
                    // For multi-line text, separate CDATA brackets from the content by introducing extra line breaks before and after
                    text = Environment.NewLine
                        + text
                        + Environment.NewLine;
                }
            }

            writer.WriteCData(text);
            cDataWritten = true;
        }


        /// <summary>
        /// Writes the specified start tag and associates it with the given name space and prefix.
        /// </summary>
        /// <param name="prefix">The name space prefix of the element.</param>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="namespace">The name space URI to associate with the element.</param>
        public override void WriteStartElement(string prefix, string localName, string @namespace)
        {
            writer.WriteStartElement(prefix, localName, @namespace);

            IncreaseIndent();
        }


        /// <summary>
        /// Closes one element and pops the corresponding name space scope.
        /// </summary>
        public override void WriteEndElement()
        {
            DecreaseIndent();
            WriteCDataIndentEnd();

            writer.WriteEndElement();
        }


        /// <summary>
        /// Closes one element and pops the corresponding name space scope.
        /// </summary>
        public override void WriteFullEndElement()
        {
            DecreaseIndent();
            WriteCDataIndentEnd();

            writer.WriteFullEndElement();
        }

        #endregion

        
        #region "Simple encapsulated methods"

        /// <summary>
        /// Writes the XML declaration with the version
        /// </summary>
        public override void WriteStartDocument()
        {
            writer.WriteStartDocument();
        }


        /// <summary>
        /// Writes the XML declaration with the version "1.0" and the standalone attribute.
        /// </summary>
        /// <param name="standalone">If true, it writes "standalone=yes"; if false, it writes "standalone=no"</param>
        public override void WriteStartDocument(bool standalone)
        {
            writer.WriteStartDocument(standalone);
        }


        /// <summary>
        /// Closes any open elements or attributes and puts the writer back in the Start state.
        /// </summary>
        public override void WriteEndDocument()
        {
            writer.WriteEndDocument();
        }


        /// <summary>
        /// Writes an element with the specified local name and value.
        /// </summary>
        /// <param name="name">The name of the DOCTYPE. This must be non-empty.</param>
        /// <param name="publicId">If non-null it also writes PUBLIC "publicId" "systemId" where publicId and systemId are replaced with the value of the given arguments.</param>
        /// <param name="systemId">If publicId is null and systemId is non-null it writes SYSTEM "systemId" where publicId is replaced with the value of this argument.</param>
        /// <param name="subset">If non-null it writes [subset] where subset is replaced with the value of this argument.</param>
        public override void WriteDocType(string name, string publicId, string systemId, string subset)
        {
            writer.WriteDocType(name, publicId, systemId, subset);
        }


        /// <summary>
        /// Writes the start of an attribute with the specified prefix, local name, and name space URI.
        /// </summary>
        /// <param name="prefix">Name space prefix of the attribute.</param>
        /// <param name="localName">LocalName of the attribute.</param>
        /// <param name="namespace">Name space URI of the attribute</param>
        public override void WriteStartAttribute(string prefix, string localName, string @namespace)
        {
            writer.WriteStartAttribute(prefix, localName, @namespace);
        }


        /// <summary>
        /// Closes the previous System.Xml.XmlWriter.WriteStartAttribute(System.String,System.String) call.
        /// </summary>
        public override void WriteEndAttribute()
        {
            writer.WriteEndAttribute();
        }


        /// <summary>
        /// Writes out a comment <!--...--> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            writer.WriteComment(text);
        }


        /// <summary>
        /// Writes out a processing instruction with a space between the name and text as follows: <?name text?>.
        /// </summary>
        /// <param name="name">The name of the processing instruction.</param>
        /// <param name="text">The text to include in the processing instruction.</param>
        public override void WriteProcessingInstruction(string name, string text)
        {
            writer.WriteProcessingInstruction(name, text);
        }


        /// <summary>
        /// Writes out an entity reference as &amp;name;.
        /// </summary>
        /// <param name="name">The name of the entity reference.</param>
        public override void WriteEntityRef(string name)
        {
            writer.WriteEntityRef(name);
        }


        /// <summary>
        /// Forces the generation of a character entity for the specified Unicode character value.
        /// </summary>
        /// <param name="character">The Unicode character for which to generate a character entity.</param>
        public override void WriteCharEntity(char character)
        {
            writer.WriteCharEntity(character);
        }


        /// <summary>
        /// Writes out the given white space.
        /// </summary>
        /// <param name="whiteSpace">The string of white space characters.</param>
        public override void WriteWhitespace(string whiteSpace)
        {
            writer.WriteWhitespace(whiteSpace);
        }


        /// <summary>
        /// Writes the given text content.
        /// </summary>
        /// <param name="text">The string of white space characters.</param>
        public override void WriteString(string text)
        {
            writer.WriteString(text);
        }


        /// <summary>
        /// Generates and writes the surrogate character entity for the surrogate character pair.
        /// </summary>
        /// <param name="lowChar">The low surrogate. This must be a value between 0xDC00 and 0xDFFF.</param>
        /// <param name="highChar">The high surrogate. This must be a value between 0xD800 and 0xDBFF.</param>
        /// <exception cref="ArgumentException">An invalid surrogate character pair was passed.</exception>
        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            writer.WriteSurrogateCharEntity(lowChar, highChar);
        }


        /// <summary>
        /// Writes text one buffer at a time.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position in the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteChars(char[] buffer, int index, int count)
        {
            writer.WriteChars(buffer, index, count);
        }


        /// <summary>
        /// Writes raw markup manually from a character buffer.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position within the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteRaw(char[] buffer, int index, int count)
        {
            writer.WriteRaw(buffer, index, count);
        }


        /// <summary>
        /// Writes raw markup manually from a string.
        /// </summary>
        /// <param name="data">String containing the text to write.</param>
        public override void WriteRaw(string data)
        {
            writer.WriteRaw(data);
        }


        /// <summary>
        /// Encodes the specified binary bytes as Base64 and writes out the resulting text.
        /// </summary>
        /// <param name="buffer">Byte array to encode.</param>
        /// <param name="index">The position in the buffer indicating the start of the bytes to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            writer.WriteBase64(buffer, index, count);
        }


        /// <summary>
        /// Closes this stream and the underlying stream.
        /// </summary>
        public override void Close()
        {
            writer.Close();
        }


        /// <summary>
        /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            writer.Flush();
        }


        /// <summary>
        /// Returns the closest prefix defined in the current name space scope for the name space URI.
        /// </summary>
        /// <param name="namespace">The name space URI whose prefix you want to find.</param>
        public override string LookupPrefix(string @namespace)
        {
            return writer.LookupPrefix(@namespace);
        }


        /// <summary>
        /// When overridden in a derived class, writes out all the attributes found at the current position in the XmlReader.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> from which to copy the attributes.</param>
        /// <param name="copyDefaultAttributes">If true copy the default attributes from the <see cref="System.Xml.XmlReader"/>; otherwise, false.</param>
        public override void WriteAttributes(XmlReader reader, bool copyDefaultAttributes)
        {
            writer.WriteAttributes(reader, copyDefaultAttributes);
        }


        /// <summary>
        /// When overridden in a derived class, encodes the specified binary bytes as BinHex and writes out the resulting text.
        /// </summary>
        /// <param name="buffer">Byte array to encode.</param>
        /// <param name="index">The position in the buffer indicating the start of the bytes to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            writer.WriteBinHex(buffer, index, count);
        }


        /// <summary>
        /// When overridden in a derived class, writes out the specified name, ensuring it is a valid name according to the W3C XML 1.0 recommendation (<a href="http://www.w3.org/TR/1998/REC-xml-19980210#NT-Name">http://www.w3.org/TR/1998/REC-xml-19980210#NT-Name</a>).
        /// </summary>
        /// <param name="name">The name to write.</param>
        public override void WriteName(string name)
        {
            writer.WriteName(name);
        }


        /// <summary>
        /// When overridden in a derived class, writes out the specified name, ensuring it is a valid NmToken according to the W3C XML 1.0 recommendation (<a href="http://www.w3.org/TR/1998/REC-xml-19980210#NT-Name">http://www.w3.org/TR/1998/REC-xml-19980210#NT-Name</a>).
        /// </summary>
        /// <param name="name">The name to write.</param>
        public override void WriteNmToken(string name)
        {
            writer.WriteNmToken(name);
        }


        /// <summary>
        /// Copies everything from the <see cref="System.Xml.XPath.XPathNavigator"/> object to the writer. The position of the <see cref="System.Xml.XPath.XPathNavigator"/> remains unchanged.
        /// </summary>
        /// <param name="navigator">The <see cref="System.Xml.XPath.XPathNavigator"/> to read from.</param>
        /// <param name="copyDefaultAttributes">If true copy the default attributes from the <see cref="System.Xml.XmlReader"/>; otherwise, false.</param>
        public override void WriteNode(XPathNavigator navigator, bool copyDefaultAttributes)
        {
            writer.WriteNode(navigator, copyDefaultAttributes);
        }


        /// <summary>
        /// When overridden in a derived class, copies everything from the reader to the writer and moves the reader to the start of the next sibling.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> to read from.</param>
        /// <param name="copyDefaultAttributes">If true copy the default attributes from the <see cref="System.Xml.XmlReader"/>; otherwise, false.</param>
        public override void WriteNode(XmlReader reader, bool copyDefaultAttributes)
        {
            writer.WriteNode(reader, copyDefaultAttributes);
        }


        /// <summary>
        /// When overridden in a derived class, writes out the name-space-qualified name. This method looks up the prefix that is in scope for the given name space.
        /// </summary>
        /// <param name="localName">The local name to write.</param>
        /// <param name="namespace">The name space URI for the name.</param>
        public override void WriteQualifiedName(string localName, string @namespace)
        {
            writer.WriteQualifiedName(localName, @namespace);
        }


        /// <summary>
        /// Writes a <see cref="System.DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes a <see cref="System.Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.Boolean"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes a <see cref="System.Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes a <see cref="System.Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes a single-precision floating-point number.
        /// </summary>
        /// <param name="value">The <see cref="System.Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes an <see cref="System.Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes an <see cref="System.Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes the object value.
        /// </summary>
        /// <param name="value">The object value to write.</param>
        public override void WriteValue(object value)
        {
            writer.WriteValue(value);
        }


        /// <summary>
        /// Writes a <see cref="System.String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="System.String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            writer.WriteValue(value);
        }

        #endregion
    }
}