using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(ObjectVersionHistoryInfo), ObjectVersionHistoryInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// ObjectVersionHistoryInfo data container class.
    /// </summary>
    public class ObjectVersionHistoryInfo : AbstractInfo<ObjectVersionHistoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.objectversionhistory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ObjectVersionHistoryInfoProvider), OBJECT_TYPE, "CMS.ObjectVersionHistory", "VersionID", "VersionModifiedWhen", null, null, null, null, "VersionObjectSiteID", null, null)
        {
            TouchCacheDependencies = true,
            LogIntegration = false,
            IsMainObject = false,
            SupportsGlobalObjects = true,
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(ObjectSettingsInfo.OBJECT_TYPE, "ObjectCheckedOutVersionHistoryID"),
                new ExtraColumn(ObjectSettingsInfo.OBJECT_TYPE, "ObjectPublishedVersionHistoryID"),
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID of the object to distinguish site objects vs. global objects.
        /// </summary>
        public virtual int VersionObjectSiteID
        {
            get
            {
                return GetIntegerValue("VersionObjectSiteID", 0);
            }
            set
            {
                SetValue("VersionObjectSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// ID of the object.
        /// </summary>
        public virtual int VersionObjectID
        {
            get
            {
                return GetIntegerValue("VersionObjectID", 0);
            }
            set
            {
                SetValue("VersionObjectID", value, (value > 0));
            }
        }


        /// <summary>
        /// Display name of the object for listings.
        /// </summary>
        public virtual string VersionObjectDisplayName
        {
            get
            {
                return GetStringValue("VersionObjectDisplayName", "");
            }
            set
            {
                SetValue("VersionObjectDisplayName", value);
            }
        }


        /// <summary>
        /// ID of the version.
        /// </summary>
        public virtual int VersionID
        {
            get
            {
                return GetIntegerValue("VersionID", 0);
            }
            set
            {
                SetValue("VersionID", value);
            }
        }


        /// <summary>
        /// Object type of the object.
        /// </summary>
        public virtual string VersionObjectType
        {
            get
            {
                return GetStringValue("VersionObjectType", "");
            }
            set
            {
                SetValue("VersionObjectType", value);
            }
        }


        /// <summary>
        /// Object version data.
        /// </summary>
        public virtual string VersionXML
        {
            get
            {
                return GetStringValue("VersionXML", "");
            }
            set
            {
                SetValue("VersionXML", value);
            }
        }


        /// <summary>
        /// ID of the user who deleted the object.
        /// </summary>
        public virtual int VersionDeletedByUserID
        {
            get
            {
                return GetIntegerValue("VersionDeletedByUserID", 0);
            }
            set
            {
                SetValue("VersionDeletedByUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// Version number.
        /// </summary>
        public virtual string VersionNumber
        {
            get
            {
                return GetStringValue("VersionNumber", "");
            }
            set
            {
                SetValue("VersionNumber", value);
            }
        }


        /// <summary>
        /// Version comment
        /// </summary>
        public virtual string VersionComment
        {
            get
            {
                return GetStringValue("VersionComment", "");
            }
            set
            {
                SetValue("VersionComment", value);
            }
        }


        /// <summary>
        /// Object version binary data.
        /// </summary>
        public virtual string VersionBinaryDataXML
        {
            get
            {
                return GetStringValue("VersionBinaryDataXML", "");
            }
            set
            {
                SetValue("VersionBinaryDataXML", value);
            }
        }


        /// <summary>
        /// Version site binding for deleted global objects with site bindings. Contains list of site GUIDs separated by ; to identify to which sites should be the object assigned during the restore opearation.
        /// </summary>
        public virtual string VersionSiteBindingIDs
        {
            get
            {
                return GetStringValue("VersionSiteBindingIDs", "");
            }
            set
            {
                SetValue("VersionSiteBindingIDs", value);
            }
        }


        /// <summary>
        /// DateTime when the version was created.
        /// </summary>
        public virtual DateTime VersionModifiedWhen
        {
            get
            {
                return GetDateTimeValue("VersionModifiedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionModifiedWhen", value);
            }
        }


        /// <summary>
        /// DateTime when the object was deleted.
        /// </summary>
        public virtual DateTime VersionDeletedWhen
        {
            get
            {
                return GetDateTimeValue("VersionDeletedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionDeletedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// ID of the user who created the version.
        /// </summary>
        public virtual int VersionModifiedByUserID
        {
            get
            {
                return GetIntegerValue("VersionModifiedByUserID", 0);
            }
            set
            {
                SetValue("VersionModifiedByUserID", value, (value > 0));
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ObjectVersionHistoryInfoProvider.DeleteVersionHistoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ObjectVersionHistoryInfoProvider.SetVersionHistoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ObjectVersionHistoryInfo object.
        /// </summary>
        public ObjectVersionHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ObjectVersionHistoryInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ObjectVersionHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return ObjectVersionHistoryInfoProvider.CheckLicense(domainName);
        }

        #endregion
    }
}