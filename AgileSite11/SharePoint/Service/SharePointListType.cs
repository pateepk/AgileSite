using System;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// Specifies well-known list types.
    /// </summary>
    public static class SharePointListType
    {
        /// <summary>
        /// All list types.
        /// </summary>
        public const int ALL = 0;


        /// <summary>
        /// Custom list.
        /// </summary>
        public const int GENERIC_LIST = 100;


        /// <summary>
        /// Document library.
        /// </summary>
        public const int DOCUMENT_LIBRARY = 101;


        /// <summary>
        /// Picture library.
        /// </summary>
        public const int PICTURE_LIBRARY = 109;
    }
}
