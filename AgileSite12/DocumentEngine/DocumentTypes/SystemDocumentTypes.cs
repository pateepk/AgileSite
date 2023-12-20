using System;

using CMS.Core;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// System document types
    /// </summary>
    public static class SystemDocumentTypes
    {
        /// <summary>
        /// Default Menu item document type class name
        /// </summary>
        internal const string DEFAULT_MENU_ITEM = "CMS.MenuItem";

        /// <summary>
        /// Class name of the Menu item document type.
        /// </summary>
        public static string MenuItem
        {
            get
            {
                return DataHelper.GetNotEmpty(CoreServices.AppSettings["CMSDefaultPageClassName"], DEFAULT_MENU_ITEM);
            }
        }


        /// <summary>
        /// Class name of the Folder document type.
        /// </summary>
        public static string Folder
        {
            get
            {
                return "CMS.Folder";
            }
        }


        /// <summary>
        /// Class name of the File document type.
        /// </summary>
        public static string File
        {
            get
            {
                return "CMS.File";
            }
        }


        /// <summary>
        /// Class name of the Root document type.
        /// </summary>
        public static string Root
        {
            get
            {
                return "CMS.Root";
            }
        }
    }
}