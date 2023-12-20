using System;
using System.IO;
using System.Text;

namespace CMS.IO
{
    /// <summary>
    /// Represents a reader that can read a sequential series of characters.
    /// </summary>
    public class StreamReader : TextReader
    {
        #region "Variables"

        private readonly System.IO.StreamReader mSystemReader;

        private bool disposed;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of stream reader.
        /// </summary>
        /// <param name="sr">System.IO.StreamReader object</param>
        protected StreamReader(System.IO.StreamReader sr)
        {
            mSystemReader = sr;
        }

        #endregion


        #region "Methods for creating new instances"

        /// <summary>
        /// Returns new instance of stream reader class.
        /// </summary>
        /// <param name="sr">System stream reader object</param>
        public static StreamReader New(System.IO.StreamReader sr)
        {
            return new StreamReader(sr);
        }


        /// <summary>
        /// Returns new instance of stream reader class.
        /// </summary>
        /// <param name="path">Path</param>
        public static StreamReader New(string path)
        {
            // Get the file stream for the file, and wrap it into the stream reader
            var fs = FileStream.New(path, FileMode.Open, FileAccess.Read);

            return New(fs);
        }


        /// <summary>
        /// Returns new instance of stream reader class.
        /// </summary>
        /// <param name="stream">System.IO.Stream object</param>
        public static StreamReader New(System.IO.Stream stream)
        {
            // The default parameter value is compatible with System.IO.StreamReader
            return New(stream, Encoding.UTF8);
        }


        /// <summary>
        /// Returns new instance of stream reader class.
        /// </summary>
        /// <param name="stream">System.IO.Stream object</param>
        /// <param name="encoding">Encoding</param>
        public static StreamReader New(System.IO.Stream stream, Encoding encoding)
        {
            var sr = new System.IO.StreamReader(stream, encoding);

            return new StreamReader(sr);
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets a value that indicates whether the current stream position is at the end of the stream.
        /// </summary>
        public bool EndOfStream
        {
            get
            {
                return mSystemReader.EndOfStream;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>        
        public override int Read()
        {
            return mSystemReader.Read();
        }


        /// <summary>
        /// Reads the stream from the current position to the end of the stream.
        /// </summary>        
        public override string ReadToEnd()
        {
            return mSystemReader.ReadToEnd();
        }


        /// <summary>
        /// Returns the next available character but does not consume it.
        /// </summary>        
        public override int Peek()
        {
            return mSystemReader.Peek();
        }


        /// <summary>
        /// Reads a line from the underlying string.
        /// </summary>        
        public override string ReadLine()
        {
            return mSystemReader.ReadLine();
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
                mSystemReader.Close();
            }

            disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}