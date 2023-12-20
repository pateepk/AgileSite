using System;
using System.Text;

namespace CMS.IO
{
    /// <summary>
    /// CMS.IO StringWriter wrapper for System.IO.StringWriter.
    /// </summary>
    public class StringWriter : System.IO.StringWriter
    {
        #region "Constructors"

        /// <summary>
        /// Creates new instance of string writer.
        /// </summary>
        public StringWriter()
            : base()
        {
        }


        /// <summary>
        /// Creates new instance of string writer.
        /// </summary>
        /// <param name="sb">String builder</param>
        /// <param name="formatProvider">Format provider</param>
        public StringWriter(StringBuilder sb, IFormatProvider formatProvider)
            : base(sb, formatProvider)
        {
        }


        /// <summary>
        /// Creates new instance of string writer.
        /// </summary>
        /// <param name="sb">String builder</param>
        public StringWriter(StringBuilder sb)
            : base(sb)
        {
        }

        #endregion
    }
}