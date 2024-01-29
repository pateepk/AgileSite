using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ClassSiteInfo), ClassSiteInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// ClassSiteInfo data container class.
    /// </summary>
    public class ClassSiteInfo : AbstractInfo<ClassSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.classsite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ClassSiteInfoProvider), OBJECT_TYPE, "CMS.ClassSite", null, null, null, null, null, null, "SiteID", "ClassID", DataClassInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            SupportsVersioning = false,

            RegisterAsBindingToObjectTypes = new List<string>
            {
                PredefinedObjectType.DOCUMENTTYPE,
                PredefinedObjectType.CUSTOMTABLECLASS
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// Class ID.
        /// </summary>
        public virtual int ClassID
        {
            get
            {
                return GetIntegerValue("ClassID", 0);
            }
            set
            {
                SetValue("ClassID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ClassSiteInfoProvider.DeleteClassSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ClassSiteInfoProvider.SetClassSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ClassSiteInfo object.
        /// </summary>
        public ClassSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ClassSiteInfo object from the given DataRow.
        /// </summary>
        public ClassSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}