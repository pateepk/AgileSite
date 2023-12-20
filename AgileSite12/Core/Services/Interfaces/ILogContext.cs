using System;

namespace CMS.Core
{
    /// <summary>
    /// Interface for the Log context
    /// </summary>
    public interface ILogContext
    {
        /// <summary>
        /// Offset of a partial log in a complete log
        /// </summary>
        int Offset
        {
            get;
        }

        
        /// <summary>
        /// Log content
        /// </summary>
        string Log
        {
            get;
            set;
        }


        /// <summary>
        /// Log guid.
        /// </summary>
        Guid LogGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum length of the log
        /// </summary>
        int MaxLength
        {
            get;
            set;
        }


        /// <summary>
        /// Appends the text.
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="newLine">Append as new line</param>
        void AppendText(string text, bool newLine = true);


        /// <summary>
        /// Clears the log
        /// </summary>
        void Clear();
    }
}
