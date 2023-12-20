using System;
using System.IO;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains a utility method for stream reading.
    /// </summary>
    internal static class StreamUtils
    {
        /// <summary>
        /// Reads <paramref name="stream"/> from its current position to the end and returns the content read.
        /// Closes the stream after reading.
        /// </summary>
        /// <param name="stream">Stream whose content to read.</param>
        /// <param name="seekToBeginning">Specifies whether to rewind the stream to its beginning if its position is greater than 0. Use when the stream has been previously read (e.g. by the model binder).</param>
        public static string ReadStreamToEnd(Stream stream, bool seekToBeginning)
        {
            if (seekToBeginning && !stream.CanSeek)
            {
                throw new InvalidOperationException("Cannot seek to the beginning of the stream. Stream does not support seeking.");
            }

            if (seekToBeginning && stream.Position > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var streamReader = CMS.IO.StreamReader.New(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
