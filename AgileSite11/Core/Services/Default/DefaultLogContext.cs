using System;
using System.Linq;
using System.Text;

namespace CMS.Core
{
    /// <summary>
    /// Default log context
    /// </summary>
    internal class DefaultLogContext : ILogContext
    {
        /// <summary>
        /// Offset of a partial log in a complete log
        /// </summary>
        public int Offset
        {
            get
            {
                return 0;
            }
        }


        /// <summary>
        /// Maximum length of the log
        /// </summary>
        public int MaxLength
        {
            get;
            set;
        }


        /// <summary>
        /// Log
        /// </summary>
        public string Log
        {
            get;
            set;
        }


        /// <summary>
        /// Log GUID
        /// </summary>
        public Guid LogGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Appends the text.
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="newLine">Append as new line</param>
        public void AppendText(string text, bool newLine = true)
        {
        }


        /// <summary>
        /// Clears the log
        /// </summary>
        public void Clear()
        {
        }
    }
}
