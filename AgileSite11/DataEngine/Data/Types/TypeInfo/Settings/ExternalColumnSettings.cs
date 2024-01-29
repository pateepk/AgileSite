using System;
using System.Collections.Generic;

using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object to encapsulate the settings of an externally stored column within a InfoObject.
    /// </summary>
    public class ExternalColumnSettings<InfoType>
    {
        #region "Variables"

        private bool mStoreColumnInDatabase = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Storage provider to use. If not set, the default storage provider is used.
        /// </summary>
        public StorageProvider StorageProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the settings key which indicates whether to store column in external storage or not.
        /// </summary>
        public string StoreInExternalStorageSettingsKey
        {
            private get;
            set;
        }


        /// <summary>
        /// Indicates whether to store data also in DB.
        /// </summary>
        public bool StoreColumnInDatabase
        {
            private get
            {
                return mStoreColumnInDatabase;
            }
            set
            {
                mStoreColumnInDatabase = value;
            }
        }


        /// <summary>
        /// Gets or sets the callback function which returns base path of the object for given column name.
        /// </summary>
        public Func<InfoType, string> StoragePath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a transformation function which is called when the data is saved to the external storage.
        /// Can be used for example to add default directives to layout code.
        /// </summary>
        public Func<InfoType, object, bool, object> SetDataTransformation
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a transformation function which is called when the data is retrieved from the external storage.
        /// Can be used for example to remove default directives to layout code.
        /// </summary>
        public Func<InfoType, object, object> GetDataTransformation
        {
            get;
            set;
        }


        /// <summary>
        /// Array of column names whose value change causes resaving of the external columns.
        /// </summary>
        public List<string> DependencyColumns
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the column should be stored in external storage
        /// </summary>
        /// <param name="objectSiteName">Object site name</param>
        public virtual bool StoreInExternalStorage(string objectSiteName)
        {
            if (!string.IsNullOrEmpty(StoreInExternalStorageSettingsKey) && !SettingsKeyInfoProvider.GetBoolValue(StoreInExternalStorageSettingsKey, objectSiteName))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns true if the column should be stored in database
        /// </summary>
        /// <param name="objectSiteName">Object site name</param>
        public virtual bool StoreInDatabase(string objectSiteName)
        {
            return StoreColumnInDatabase;
        }

        #endregion
    }
}