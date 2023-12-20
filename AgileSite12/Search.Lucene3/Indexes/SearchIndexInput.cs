using Lucene.Net.Store;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Search index input based on the file stream
    /// </summary>
    internal class SearchIndexInput : BufferedIndexInput
    {
        protected internal StreamDescriptor file;
        internal bool isClone;
        private bool isDisposed;
        
        //  LUCENE-1566 - maximum read length on a 32bit JVM to prevent incorrect OOM 
        protected internal int chunkSize;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="chunkSize">Chunk size</param>
        public SearchIndexInput(System.IO.Stream stream, int bufferSize, int chunkSize)
            : base(bufferSize)
        {
            file = new StreamDescriptor(stream);
            this.chunkSize = chunkSize;
        }

        
        /// <summary>
        /// Reads the data from the stream
        /// </summary>
        /// <param name="b">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="len">Number of bytes to read</param>
        public override void ReadInternal(byte[] b, int offset, int len)
        {
            lock (file)
            {
                long position = FilePointer;
                if (position != file.position)
                {
                    file.BaseStream.Seek(position, System.IO.SeekOrigin.Begin);
                    file.position = position;
                }
                int total = 0;

                try
                {
                    do
                    {
                        int readLength;
                        if (total + chunkSize > len)
                        {
                            readLength = len - total;
                        }
                        else
                        {
                            // LUCENE-1566 - work around JVM Bug by breaking very large reads into chunks
                            readLength = chunkSize;
                        }
                        int i = file.Read(b, offset + total, readLength);
                        if (i == -1)
                        {
                            throw new System.IO.IOException("read past EOF");
                        }
                        file.position += i;
                        total += i;
                    }
                    while (total < len);
                }
                catch (System.OutOfMemoryException e)
                {
                    // propagate OOM up and add a hint for 32bit VM Users hitting the bug
                    // with a large chunk size in the fast path.
                    System.OutOfMemoryException outOfMemoryError = new System.OutOfMemoryException("OutOfMemoryError likely caused by the Sun VM Bug described in " + "https://issues.apache.org/jira/browse/LUCENE-1566; try calling FSDirectory.setReadChunkSize " + "with a a value smaller than the current chunks size (" + chunkSize + ")", e);
                    throw outOfMemoryError;
                }
            }
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        /// <param name="disposing">Flag that the object is being disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                // only close the file if this is not a clone
                if (!isClone && file != null)
                {
                    file.Close();
                    file = null;
                }
            }

            isDisposed = true;
        }


        /// <summary>
        /// Sets the internal stream position
        /// </summary>
        /// <param name="position">Position</param>
        public override void SeekInternal(long position)
        {
        }


        /// <summary>
        /// Returns the stream length
        /// </summary>
        public override long Length()
        {
            return file.length;
        }


        /// <summary>
        /// Clones the object
        /// </summary>
        /// <returns></returns>
        public override System.Object Clone()
        {
            SearchIndexInput clone = (SearchIndexInput)base.Clone();
            clone.isClone = true;
            return clone;
        }
    }
}