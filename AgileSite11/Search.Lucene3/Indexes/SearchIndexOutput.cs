using Lucene.Net.Store;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Search index output
    /// </summary>
    internal class SearchIndexOutput : BufferedIndexOutput
    {
        internal System.IO.Stream file = null;

        // remember if the file is open, so that we don't try to close it
        // more than once
        private volatile bool isOpen;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Output stream</param>
        public SearchIndexOutput(System.IO.Stream stream)
        {
            file = stream;
            isOpen = true;
        }

        
        /// <summary>
        /// Flushes the given buffer to the output
        /// </summary>
        /// <param name="b">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size to write</param>
        public override void FlushBuffer(byte[] b, int offset, int size)
        {
            file.Write(b, offset, size);
            // {{dougsale-2.4.0}}
            // FSIndexOutput.Flush
            // When writing frequently with small amounts of data, the data isn't flushed to disk.
            // Thus, attempting to read the data soon after this method is invoked leads to
            // BufferedIndexInput.Refill() throwing an IOException for reading past EOF.
            // Test\Index\TestDoc.cs demonstrates such a situation.
            // Forcing a flush here prevents said issue.
            // {{DIGY 2.9.0}}
            // This code is not available in Lucene.Java 2.9.X.
            // Can there be a indexing-performance problem?
            file.Flush();
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        /// <param name="disposing">Flag if the object is currently disposing</param>
        protected override void Dispose(bool disposing)
        {
            // only close the file if it has not been closed yet
            if (isOpen)
            {
                bool success = false;
                try
                {
                    base.Dispose(disposing);
                    success = true;
                }
                finally
                {
                    isOpen = false;
                    if (!success)
                    {
                        try
                        {
                            file.Dispose();
                        }
                        catch (System.Exception)
                        {
                            // Suppress so we don't mask original exception
                        }
                    }
                    else
                        file.Dispose();
                }
            }
        }

        
        /// <summary>
        /// Adjusts the stream to the given position
        /// </summary>
        /// <param name="pos">Position</param>
        public override void Seek(long pos)
        {
            base.Seek(pos);
            file.Seek(pos, System.IO.SeekOrigin.Begin);
        }


        /// <summary>
        /// Returns the stream length
        /// </summary>
        public override long Length
        {
            get { return file.Length; }
        }


        /// <summary>
        /// Sets the stream length
        /// </summary>
        /// <param name="length">Length to set</param>
        public override void SetLength(long length)
        {
            file.SetLength(length);
        }
    }
}