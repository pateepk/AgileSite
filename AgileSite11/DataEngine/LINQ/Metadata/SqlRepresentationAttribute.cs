using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines the SQL representation of the given method call
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SqlRepresentationAttribute : Attribute
    {
        /// <summary>
        /// Defines format of the SQL representing this method call. Individual parameters are represented via standard format string with {0}, {1} etc.
        /// </summary>
        public string Format
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format">Defines format of the SQL representing this method call. Individual parameters are represented via standard format string with {0}, {1} etc.</param>
        public SqlRepresentationAttribute(string format)
        {
            Format = format;
        }
    }
}
