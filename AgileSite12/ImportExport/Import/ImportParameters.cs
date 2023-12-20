using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import parameters
    /// </summary>
    public class ImportParameters
    {
        #region "Variables"

        private Hashtable mExtraParameters;

        private bool mCheckExisting = true;
        private bool mCheckUnique = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Existing object
        /// </summary>
        public BaseInfo ExistingObject
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the object processing
        /// </summary>
        public ProcessObjectEnum ObjectProcessType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the object update should be skipped
        /// </summary>
        public bool SkipObjectUpdate
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
        /// Indicates if child object should be updated
        /// </summary>
        public bool UpdateChildObjects
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if object is being imported in post processing
        /// </summary>
        public bool PostProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Represents original object ID
        /// </summary>
        public int ObjectOriginalID
        {
            get;
            set;
        }


        /// <summary>
        /// Complete object package data
        /// </summary>
        public DataSet Data
        {
            get;
            set;
        }


        /// <summary>
        /// List of objects for post processing
        /// </summary>
        public List<GeneralizedInfo> PostProcessList
        {
            get;
            set;
        }


        /// <summary>
        /// List of already imported objects
        /// </summary>
        public List<ImportedObject> ImportedObjects
        {
            get;
            set;
        }


        /// <summary>
        /// Special parameters for import process
        /// </summary>
        private Hashtable ExtraParameters
        {
            get
            {
                return mExtraParameters ?? (mExtraParameters = new Hashtable());
            }
        }


        /// <summary>
        /// If true, the import process should check the existing object, otherwise it should import the object always as a new object.
        /// Not checking existing objects when not necessary improves import performance.
        /// </summary>
        public bool CheckExisting
        {
            get
            {
                return mCheckExisting;
            }
            set
            {
                mCheckExisting = value;
            }
        }


        /// <summary>
        /// Parent of the imported object(s) 
        /// </summary>
        public GeneralizedInfo ParentObject
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the objects should check their unique names
        /// </summary>
        public bool CheckUnique
        {
            get
            {
                return mCheckUnique;
            }
            set
            {
                mCheckUnique = value;
            }
        }


        /// <summary>
        /// Translation helper object to translate incoming IDs to new IDs
        /// </summary>
        public TranslationHelper TranslationHelper
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets extra parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public void SetValue(string name, object value)
        {
            ExtraParameters[name] = value;
        }


        /// <summary>
        /// Gets extra parameter
        /// </summary>
        /// <typeparam name="ReturnType">Data type of the parameter</typeparam>
        /// <param name="name">Parameter name</param>
        /// <param name="defaultValue">Default value</param>
        public ReturnType GetValue<ReturnType>(string name, ReturnType defaultValue)
        {
            return ValidationHelper.GetValue(ExtraParameters[name], defaultValue);
        }

        #endregion
    }
}