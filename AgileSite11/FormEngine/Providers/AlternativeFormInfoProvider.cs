using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.IO;

namespace CMS.FormEngine
{
    /// <summary>
    /// Class providing AlternativeFormInfo management.
    /// </summary>
    public class AlternativeFormInfoProvider : AbstractInfoProvider<AlternativeFormInfo, AlternativeFormInfoProvider>, IFullNameInfoProvider
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public AlternativeFormInfoProvider()
            : base(AlternativeFormInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    FullName = true
                })
        {
        }

        #endregion


        #region "Variables"

        /// <summary>
        /// Virtual directory where the FormLayouts are located.
        /// </summary>
        private const string mFormLayoutsDirectory = "~/CMSVirtualFiles/AltFormLayouts";


        /// <summary>
        /// List of default form layout namespaces
        /// </summary>
        private static readonly List<string> mDefaultNamespaces = new List<string> { "CMS.FormEngine.Web.UI" };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether alternative forms should be stored externally
        /// </summary>
        public static bool StoreAlternativeFormsInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreAltFormLayoutsInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreAltFormLayoutsInFS", value);
            }
        }


        /// <summary>
        /// Alternative form layouts directory - Read only
        /// </summary>
        public static string FormLayoutsDirectory
        {
            get
            {
                return mFormLayoutsDirectory;
            }
        }


        /// <summary>
        /// List of default form layout namespaces
        /// </summary>
        public static List<string> DefaultNamespaces
        {
            get
            {
                return mDefaultNamespaces;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns the query for all alternative forms.
        /// </summary>   
        public static ObjectQuery<AlternativeFormInfo> GetAlternativeForms()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns alt.form layout info for specified path.
        /// </summary>
        /// <param name="path">Path</param>
        public static AlternativeFormInfo GetVirtualObject(string path)
        {
            List<string> prefixes = new List<string>();
            // Get alt.form name
            string formName = VirtualPathHelper.GetVirtualObjectName(path, FormLayoutsDirectory, ref prefixes);
            if (prefixes.Count > 0)
            {
                formName = prefixes[0] + '.' + formName;
            }
            return GetAlternativeFormInfo(formName);
        }


        /// <summary>
        /// Returns the AlternativeFormInfo structure for the alternative form specified by its full name.
        /// </summary>
        /// <param name="alternativeFormFullName">Full name of the alternative form ('classname.'formname')</param>
        public static AlternativeFormInfo GetAlternativeFormInfo(string alternativeFormFullName)
        {
            return ProviderObject.GetAlternativeFormInfoInternal(alternativeFormFullName);
        }


        /// <summary>
        /// Returns the AlternativeFormInfo structure for the specified alternativeForm.
        /// </summary>
        /// <param name="alternativeFormId">AlternativeForm id</param>
        public static AlternativeFormInfo GetAlternativeFormInfo(int alternativeFormId)
        {
            return ProviderObject.GetInfoById(alternativeFormId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified alternativeForm.
        /// </summary>
        /// <param name="alternativeForm">AlternativeForm to set</param>
        public static void SetAlternativeFormInfo(AlternativeFormInfo alternativeForm)
        {
            ProviderObject.SetInfo(alternativeForm);
        }


        /// <summary>
        /// Deletes specified alternativeForm.
        /// </summary>
        /// <param name="infoObj">AlternativeForm object</param>
        public static void DeleteAlternativeFormInfo(AlternativeFormInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified alternativeForm.
        /// </summary>
        /// <param name="alternativeFormId">AlternativeForm id</param>
        public static void DeleteAlternativeFormInfo(int alternativeFormId)
        {
            AlternativeFormInfo infoObj = GetAlternativeFormInfo(alternativeFormId);
            DeleteAlternativeFormInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the AlternativeFormInfo structure for the alternative form specified by its full name.
        /// </summary>
        /// <param name="alternativeFormFullName">Full name of the alternative form ('classname.'formname')</param>
        protected AlternativeFormInfo GetAlternativeFormInfoInternal(string alternativeFormFullName)
        {
            return GetInfoByFullName(alternativeFormFullName, true);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(AlternativeFormInfo info)
        {
            bool layoutChanged = info.ItemChanged("FormLayout");
            if (layoutChanged || String.IsNullOrEmpty(info.FormVersionGUID))
            {
                info.FormVersionGUID = Guid.NewGuid().ToString();
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Full name methods"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(AlternativeFormInfo.OBJECT_TYPE, "FormClassID;FormName");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            return new AlternativeFormFullNameWhereConditionBuilder(fullName)
                .Build()
                .ToString(true);
        }

        #endregion
    }
}