using System;

using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Base for the exceptions thrown during the macro resolving process.
    /// </summary>
    public class MacroException : Exception
    {
        /// <summary>
        /// Gets or sets the whole expression which was being processed when this error occurred.
        /// </summary>
        public string OriginalExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being processed when this error occurred.</param>
        /// <param name="innerException">Reference to the inner exception that is the cause of this exception.</param>
        public MacroException(string originalExpression, Exception innerException = null) :
            base(null, innerException)
        {
            OriginalExpression = originalExpression;
        }


        /// <summary>
        /// Gets the string by the specified resource key
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        internal string GetString(string resourceKey)
        {
            return CoreServices.Localization.GetString(resourceKey);
        }


        /// <summary>
        /// Gets the string by the specified resource key
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="defaultValue">Default value</param>
        internal string GetAPIString(string resourceKey, string defaultValue)
        {
            return CoreServices.Localization.GetAPIString(resourceKey, null, defaultValue);
        }
    }
}
