using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base CMS Control properties interface definition.
    /// </summary>
    public interface ICMSControlProperties : ICMSBaseProperties
    {
        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        TreeProvider TreeProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Class name value or several values separated by semicolon.
        /// </summary>
        string ClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Path of the documents to be displayed. /% selects all documents.
        /// </summary>
        string Path
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code, such as en-us.
        /// </summary>
        string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the documents from the default culture version should be alternatively used.
        /// </summary>
        bool CombineWithDefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if only published documents should be displayed.
        /// </summary>
        bool SelectOnlyPublished
        {
            get;
            set;
        }


        /// <summary>
        /// Relative level of child documents that should be selected. -1 selects all child documents.
        /// </summary>
        int MaxRelativeLevel
        {
            get;
            set;
        }


        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
        /// </summary>
        bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        /// <param name="forceReload">Indicates force load</param>
        void ReloadData(bool forceReload);
    }
}