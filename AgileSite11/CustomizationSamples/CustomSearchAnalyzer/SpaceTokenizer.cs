using System;
using System.IO;

using Lucene.Net.Analysis;

namespace CMS.CustomSearchAnalyzer
{
    /// <summary>
    /// A SpaceTokenizer is a tokenizer that divides text at space.
    /// </summary>
    public class SpaceTokenizer : Tokenizer
    {
        #region "Variables"

        private int offset = 0;
        private int bufferIndex = 0;
        private int dataLen = 0;
        private const int MAX_WORD_LEN = 255;
        private const int IO_BUFFER_SIZE = 1024;
        private char[] buffer = new char[MAX_WORD_LEN];
        private char[] ioBuffer = new char[IO_BUFFER_SIZE];

        #endregion


        #region "Methods"

        /// <summary>
        /// Construct a new SpaceTokenizer.
        /// </summary>
        public SpaceTokenizer(TextReader in_Renamed)
            : base(in_Renamed)
        {
        }


        /// <summary>
        /// Returns the next token in the stream, or null at EOS.
        /// </summary>
        public override bool IncrementToken()
        {
            // Initialize length & offset
            int length = 0;
            int start = offset;

            // Loop until break
            while (true)
            {
                // Current character
                char c;

                // Increase offset
                offset++;
                // Read characterst to the buffer
                if (bufferIndex >= dataLen)
                {
                    dataLen = input.Read((Char[])ioBuffer, 0, ioBuffer.Length);
                    bufferIndex = 0;
                }

                // Check whether at least one character is in buffer
                if (dataLen <= 0)
                {
                    if (length > 0)
                    {
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    c = ioBuffer[bufferIndex++];
                }

                // Check whether char is not space
                if (!c.Equals(' '))
                {
                    // if it's a token char
                    if (length == 0)
                    {
                        // start of token
                        start = offset - 1;
                    }

                    // buffer it
                    buffer[length++] = c;

                    // buffer overflow
                    if (length == MAX_WORD_LEN)
                    {
                        break;
                    }
                }
                else if (length > 0)
                {
                    // at non-Letter w/ chars
                    break; // return 'em
                }
            }

            // Return new token
            return true;
        }

        #endregion
    }
}