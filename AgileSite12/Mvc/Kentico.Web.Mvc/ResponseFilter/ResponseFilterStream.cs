using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CMS.Helpers;

namespace Kentico.Web.Mvc.Internal
{
    /// <summary>
    /// Provides a stream with filters that modifies the stream data.
    /// </summary>
    internal class ResponseFilterStream : Stream
    {
        private readonly IEnumerable<Func<string, string>> responseFilters;
        private Encoding encoding;
        private MemoryStream responseHtmlStream;
        private Stream outStream;
        private const int INITIAL_STREAM_CAPACITY = 16384;


        private Encoding Encoding
        {
            get
            {
                return encoding ?? (encoding = CMSHttpContext.Current?.Response?.ContentEncoding ?? Encoding.UTF8);
            }
        }


        /// <summary>
        /// Creates new instance of <see cref="ResponseFilterStream"/>
        /// </summary>
        /// <param name="outStream">Output stream</param>
        /// <param name="responseFilters">Collection of response filters</param>
        public ResponseFilterStream(Stream outStream, IEnumerable<Func<string, string>> responseFilters)
        {
            this.outStream = outStream;
            this.responseFilters = responseFilters;
            responseHtmlStream = new MemoryStream(INITIAL_STREAM_CAPACITY);
        }


        /// <summary>
        /// Handles the write event.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            responseHtmlStream.Write(buffer, offset, count);
        }


        /// <summary>
        /// Closes the current stream and releases any resources associated with the current stream.
        /// Runs the processor over the stream data.
        /// </summary>
        public override void Close()
        {
            var finalHtml = Encoding.GetString(responseHtmlStream.GetBuffer(), 0, (int)responseHtmlStream.Length);

            foreach (var processor in responseFilters)
            {
                finalHtml = processor(finalHtml);
            }

            var data = Encoding.GetBytes(finalHtml);

            if (data.Length > 0)
            {
                outStream.Write(data, 0, data.Length);
            }

            outStream.Close();
            outStream = null;

            responseHtmlStream.Dispose();
            responseHtmlStream = null;
        }


        #region "Stream members"

        public override bool CanRead => outStream.CanRead;


        public override bool CanSeek => outStream.CanSeek;


        public override bool CanWrite => true;


        public override long Length => 0;


        public override long Position { get; set; }


        public override void Flush()
        {
            outStream.Flush();
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            return outStream.Seek(offset, origin);
        }


        public override void SetLength(long value)
        {
            outStream.SetLength(value);
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            return outStream.Read(buffer, offset, count);
        }

        #endregion
    }
}
