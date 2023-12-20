using System;

namespace CMS.ImportExport
{
    /// <summary>
    /// Thrown during data export when no data are available for export.
    /// </summary>
    public class NoDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoDataException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NoDataException(string message)
            : base(message)
        {
        }
    }
}