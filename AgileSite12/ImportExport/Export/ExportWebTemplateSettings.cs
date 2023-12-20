using System.Collections.Generic;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Settings for exporting web template.
    /// </summary>
    public class ExportWebTemplateSettings
    {
        private List<ExportWebTemplateAdditionalObject> mAdditionalObjects;

        /// <summary>
        /// Site code name.
        /// </summary>
        public string SiteCodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Full web site path.
        /// </summary>
        public string WebsitePath
        {
            get;
            set;
        }


        /// <summary>
        /// Full target path.
        /// </summary>
        public string TargetPath
        {
            get;
            set;
        }


        /// <summary>
        /// Expressions to exclude the exported objects by code name and display name.
        /// </summary>
        public string[] ExcludedNameExpressions
        {
            get;
            set;
        }


        /// <summary>
        /// Expressions to include the exported objects by code name and display name.
        /// </summary>
        public string[] IncludedNameExpressions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if e-commerce module objects should be exported.
        /// </summary>
        public bool ExportEcommerce
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if community module objects should be exported.
        /// </summary>
        public bool ExportCommunity
        {
            get;
            set;
        }


        /// <summary>
        /// Current user info.
        /// </summary>
        public IUserInfo UserInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Additional objects
        /// </summary>
        public ICollection<ExportWebTemplateAdditionalObject> AdditionalObjects
        {
            get 
            {
                if (mAdditionalObjects == null)
                {
                    return mAdditionalObjects = new List<ExportWebTemplateAdditionalObject>();
                }

                return mAdditionalObjects;
            }
        }
    }
}