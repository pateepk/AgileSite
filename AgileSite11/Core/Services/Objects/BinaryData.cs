using System;
using System.IO;
using System.Security.Cryptography;

namespace CMS.Core
{
    /// <summary>
    /// Wrapper for the binary data (fox example for the web farm tasks).
    /// </summary>
    public class BinaryData
    {
        #region "Variables"

        /// <summary>
        /// Resulting binary data
        /// </summary>
        protected byte[] mData = null;


        /// <summary>
        /// Resulting stream
        /// </summary>
        protected Stream mStream = null;


        /// <summary>
        /// Checksum to compare binary data
        /// </summary>
        private string mChecksum;

        #endregion


        #region "Properties"

        /// <summary>
        /// Length of the binary data
        /// </summary>
        public long Length
        {
            get
            {
                if (SourceData != null)
                {
                    return SourceData.Length;
                }
                else if (SourceStream != null)
                {
                    return SourceStream.Length;
                }
                else if (GetData != null)
                {
                    // Use data to get the length
                    var data = Data;
                    if (data != null)
                    {
                        return data.Length;
                    }
                }

                return 0;
            }
        }


        /// <summary>
        /// Resulting binary data
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (mData == null)
                {
                    if (SourceData != null)
                    {
                        mData = SourceData;
                    }
                    else if (SourceStream != null)
                    {
                        // Get data from stream
                        try
                        {
                            mData = GetByteArrayFromStream(SourceStream);
                        }
                        catch (Exception ex)
                        {
                            CoreServices.EventLog.LogException("WebFarm", "LOADDATAFROMSTREAM", ex);
                        }
                    }
                    else if (GetData != null)
                    {
                        // Get the data with external function
                        mData = GetData();
                        IsLoaded = true;
                    }
                }

                return mData;
            }
        }


        /// <summary>
        /// Returns SourceStream if not null, otherwise returns SourceData wrapped in the MemoryStream.
        /// </summary>
        public Stream Stream
        {
            get
            {
                if (mStream == null)
                {
                    if (SourceStream != null)
                    {
                        mStream = SourceStream;
                    }
                    else
                    {
                        if (SourceData != null)
                        {
                            mStream = new MemoryStream(SourceData);
                        }
                    }
                }
                return mStream;
            }
        }


        /// <summary>
        /// Source data
        /// </summary>
        protected byte[] SourceData
        {
            get;
            set;
        }


        /// <summary>
        /// Source stream
        /// </summary>
        public Stream SourceStream
        {
            get;
            protected set;
        }


        /// <summary>
        /// External method used to get the binary data when required
        /// </summary>
        protected Func<byte[]> GetData
        {
            get;
            set;
        }


        /// <summary>
        /// True if data were loaded to memory.
        /// </summary>
        public bool IsLoaded
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets data checksum.
        /// </summary>
        public string Checksum
        {
            get
            {
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    byte[] md5Data = null;
                    if (SourceData != null)
                    {
                        md5Data = md5.ComputeHash(SourceData);
                    }
                    else if (SourceStream != null)
                    {
                        // Reinitialize position
                        var originalPosition = SourceStream.Position;
                        SourceStream.Position = 0;

                        md5Data = md5.ComputeHash(SourceStream);

                        // Get back previous value
                        SourceStream.Position = originalPosition;
                    }

                    if (md5Data != null)
                    {
                        mChecksum = BitConverter.ToString(md5Data).Replace("-", "");
                    }
                }

                return mChecksum;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Source data</param>
        public BinaryData(byte[] data)
        {
            SourceData = data;
            IsLoaded = true;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="str">Source stream</param>
        public BinaryData(Stream str)
        {
            SourceStream = str;
            IsLoaded = true;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getData">Method used to get the binary data when required</param>
        public BinaryData(Func<byte[]> getData)
        {
            GetData = getData;
        }


        /// <summary>
        /// Implicit operator to convert byte array to task binary data
        /// </summary>
        /// <param name="data">Source data</param>
        public static implicit operator BinaryData(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            return new BinaryData(data);
        }


        /// <summary>
        /// Implicit operator to convert stream to task binary data
        /// </summary>
        /// <param name="str">Source stream</param>
        public static implicit operator BinaryData(Stream str)
        {
            if (str == null)
            {
                return null;
            }

            return new BinaryData(str);
        }


        /// <summary>
        /// Returns byte array from input stream.
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="bufferSize">Buffer size, by default 64kB</param>
        /// <param name="position">Position from which the reading should start</param>
        public static byte[] GetByteArrayFromStream(Stream stream, int bufferSize = 65536, long position = 0)
        {
            if (stream == null)
            {
                return null;
            }

            // Optimization for memory stream
            var memoryStream = stream as MemoryStream;
            if (position == 0 && memoryStream != null)
            {
                stream.Seek(0L, SeekOrigin.End);

                return memoryStream.ToArray();
            }

            // Set starting position
            stream.Position = position;

            // Read input stream to memory stream
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms, bufferSize);

                return ms.ToArray();
            }
        }


        /// <summary>
        /// Closes the stream behind this binary data (if set)
        /// </summary>
        public void Close()
        {
            // Close and dispose stream
            if (SourceStream != null)
            {
                SourceStream.Close();
            }
        }

        #endregion
    }
}
