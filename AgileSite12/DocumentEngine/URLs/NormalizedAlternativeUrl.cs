using System;

using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class represents url in normalized state used in alternative url feature.
    /// </summary>
    /// <remarks>
    /// Regular url can be normalized using <see cref="AlternativeUrlHelper.NormalizeAlternativeUrl"/> method.
    /// </remarks>
    /// <seealso cref="AlternativeUrlHelper.NormalizeAlternativeUrl"/>
    public sealed class NormalizedAlternativeUrl
    {
        /// <summary>
        /// Gets normalized url.
        /// </summary>
        public string NormalizedUrl
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="NormalizedAlternativeUrl"/>.
        /// </summary>
        /// <param name="normalizedUrl">Url in normalized state.</param>
        /// <exception cref="InvalidOperationException">When null value is provided.</exception>
        internal NormalizedAlternativeUrl(string normalizedUrl)
        {
            NormalizedUrl = normalizedUrl ?? String.Empty;
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return NormalizedUrl;
        }
    }
}
