using System.IO;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Stream descriptor
    /// </summary>
    internal class StreamDescriptor : BinaryReader
    {
        // remember if the file is open, so that we don't try to close it
        // more than once
        protected internal volatile bool isOpen;

        internal long position;
        internal long length;

        private bool isDisposed;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">Input stream</param>
        public StreamDescriptor(Stream file)
            : base(file)
        {
            isOpen = true;
            length = file.Length;
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        /// <param name="disposing">Flag if the object is currently disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                if (isOpen)
                {
                    isOpen = false;
                }
            }

            isDisposed = true;
            base.Dispose(disposing);
        }


        /// <summary>
        /// Destructor
        /// </summary>
        ~StreamDescriptor()
        {
            Dispose(false);
        }
    }
}