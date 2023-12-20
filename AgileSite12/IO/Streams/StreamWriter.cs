using System;
using System.IO;
using System.Text;

namespace CMS.IO
{
    /// <summary>
    /// Represents a writer that can write a sequential series of characters.
    /// </summary>
    public class StreamWriter : TextWriter
    {
        #region "Variables"

        // Default encoding is the same as System.IO.StreamWriter uses - UTF8NoBOM
        internal static readonly Encoding DEFAULT_ENCODING = new UTF8Encoding(false, true);

        private readonly System.IO.StreamWriter mSystemWriter;

        private bool disposed;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of stream writer.
        /// </summary>
        /// <param name="sw">Stream writer object</param>
        private StreamWriter(System.IO.StreamWriter sw)
        {
            mSystemWriter = sw;
        }

        #endregion


        #region "Methods for creating new instances"

        /// <summary>
        /// Returns new instance of stream writer class.
        /// </summary>
        /// <param name="stream">Stream object</param>
        public static StreamWriter New(System.IO.Stream stream)
        {
            // The default parameter value is compatible with System.IO.StreamWriter
            return New(stream, DEFAULT_ENCODING);
        }


        /// <summary>
        /// Returns new instance of stream writer class.
        /// </summary>
        /// <param name="stream">Stream object</param>
        /// <param name="encoding">Encoding</param>
        public static StreamWriter New(System.IO.Stream stream, Encoding encoding)
        {
            var sw = new System.IO.StreamWriter(stream, encoding);

            return new StreamWriter(sw);
        }


        /// <summary>
        /// Returns new instance of stream writer class.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="append">If should append data to existing file</param>
        public static StreamWriter New(string path, bool append)
        {
            // The default parameter value is compatible with System.IO.StreamWriter
            return New(path, append, DEFAULT_ENCODING);
        }


        /// <summary>
        /// Returns new instance of stream writer class.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="append">If should append data to existing file</param>
        /// <param name="encoding">Encoding</param>
        public static StreamWriter New(string path, bool append, Encoding encoding)
        {
            var fs = FileStream.New(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);

            return New(fs, encoding);
        }


        /// <summary>
        /// Returns new instance of stream writer class.
        /// </summary>
        /// <param name="path">Path to file</param>        
        public static StreamWriter New(string path)
        {
            // The default parameter values are compatible with System.IO.StreamWriter
            return New(path, false, DEFAULT_ENCODING);
        }


        /// <summary>
        /// Returns new instance of stream writer class.
        /// </summary>
        /// <param name="sw">Stream writer object</param>
        public static StreamWriter New(System.IO.StreamWriter sw)
        {
            return new StreamWriter(sw);
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets the underlying stream that interfaces with a backing store.
        /// </summary>
        public System.IO.Stream BaseStream
        {
            get
            {
                return mSystemWriter.BaseStream;
            }
        }


        /// <summary>
        /// Returns current stream writer encoding.
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return mSystemWriter.Encoding;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Writes a string to the stream.
        /// </summary>
        /// <param name="value">String to write</param>
        public override void Write(string value)
        {
            mSystemWriter.Write(value);
        }


        /// <summary>
        /// Writes a string followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public override void WriteLine(string value)
        {
            mSystemWriter.WriteLine(value);
        }


        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
        /// </summary>
        public override void Flush()
        {
            mSystemWriter.Flush();
        }


        /// <summary>
        /// Releases all resources.
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                mSystemWriter.Close();
            }

            disposed = true;

            base.Dispose(disposing);
        }


        /// <summary>
        /// Writes one character to stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public override void Write(char value)
        {
            mSystemWriter.Write(value);
        }


        /// <summary>
        /// Writes char array to the stream.
        /// </summary>
        /// <param name="buffer">Buffer with chars</param>
        /// <param name="index">Buffer start index</param>
        /// <param name="count">Number of characters to write</param>
        public override void Write(char[] buffer, int index, int count)
        {
            mSystemWriter.Write(buffer, index, count);
        }

        #endregion
    }
}