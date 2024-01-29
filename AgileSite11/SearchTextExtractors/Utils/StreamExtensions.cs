using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CMS.Search.TextExtractors
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Reads all characters from the current position to the end of the stream.
        /// </summary>
        public static string ReadToEnd(this Stream stream, Encoding encoding)
        {
            using (var sr = IO.StreamReader.New(stream, encoding))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
