using System;
using System.Linq;
using System.Text;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Defines format of culture specific URLs. Enum is Flags enum.
    /// </summary>
    [Flags]
    public enum UrlOptionsEnum
    {
        /// <summary>
        /// URLs for not translated documents should be generated an URLs are not culture specific.
        /// </summary>
        Default = 1,


        /// <summary>
        /// URLs of not existing culture versions will not be generated.
        /// </summary>
        ExcludeUntranslatedDocuments = 2,
        
        
        /// <summary>
        /// Returns URL for the document aliasPath and document URL path (preferable if document URL path is not wildcard URL).
        /// Without this option the culture code or alias is added to the URL as a query parameter.
        /// </summary>
        UseCultureSpecificURLs = 4
    }
}
