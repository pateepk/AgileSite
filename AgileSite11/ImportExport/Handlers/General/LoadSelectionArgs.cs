using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Event arguments for events based on object type
    /// </summary>
    public class LoadSelectionArgs<SettingsType> : CMSEventArgs
        where SettingsType : AbstractImportExportSettings
    {
        private bool mSelect = true;
        private bool mProcessDependency = true;
        private bool mDependencyIsSiteObject = true;

        
        /// <summary>
        /// Import/Export settings
        /// </summary>
        public SettingsType Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Object type which is imported
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if site object is being imported
        /// </summary>
        public bool SiteObject
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the selection should be done by the system
        /// </summary>
        public bool Select
        {
            get
            {
                return mSelect;
            }
            set
            {
                mSelect = value;
            }
        }


        /// <summary>
        /// Indicates if the dependency should be processed for automatic selection
        /// </summary>
        public bool ProcessDependency
        {
            get
            {
                return mProcessDependency;
            }
            set
            {
                mProcessDependency = value;
            }
        }


        /// <summary>
        /// Forces dependency to act like site object
        /// </summary>
        public bool DependencyIsSiteObject
        {
            get
            {
                return mDependencyIsSiteObject;
            }
            set
            {
                mDependencyIsSiteObject = value;
            }
        }


        /// <summary>
        /// Empty dependency object. By default set to parent of current object.
        /// </summary>
        public BaseInfo DependencyObject
        {
            get;
            set;
        }


        /// <summary>
        /// Object type of the object the current object type depends on. By default set to ParentObjectType of current object.
        /// </summary>
        public string DependencyObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Reference column of the object the current object type depends on. By default set to ParentIDColumn of current object.
        /// </summary>
        public string DependencyIDColumn
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Event arguments for events based on object type in import process
    /// </summary>
    public class ImportLoadSelectionArgs : LoadSelectionArgs<SiteImportSettings>
    {
    }


    /// <summary>
    /// Event arguments for events based on object type in export process
    /// </summary>
    public class ExportLoadSelectionArgs : LoadSelectionArgs<SiteExportSettings>
    {
    }
}