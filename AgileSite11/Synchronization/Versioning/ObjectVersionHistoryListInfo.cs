using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(ObjectVersionHistoryListInfo), "cms.objectversionhistorylist")]

namespace CMS.Synchronization
{
    /// <summary>
    /// ObjectVersionHistoryListInfo virtual object.
    /// </summary>
    public class ObjectVersionHistoryListInfo : AbstractInfo<ObjectVersionHistoryListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.objectversionhistorylist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ObjectVersionHistoryInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ObjectVersionHistoryListInfo object.
        /// </summary>
        public ObjectVersionHistoryListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ObjectVersionHistoryListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ObjectVersionHistoryListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                        "VersionID",
                        "VersionObjectID",
                        "VersionObjectType",
                        "VersionObjectSiteID",
                        "VersionObjectDisplayName",
                        "VersionXML",
                        "VersionBinaryDataXML",
                        "VersionModifiedByUserID",
                        "VersionModifiedWhen", 
                        "VersionDeletedByUserID",
                        "VersionDeletedWhen",
                        "VersionNumber",
                        "VersionSiteBindingIDs",
                        "UserID",
                        "UserName",
                        "FirstName",
                        "MiddleName",
                        "LastName",
                        "FullName",
                        "Email",
                        "PreferredCultureCode",
                        "PreferredUICultureCode",
                        "UserEnabled",
                        "UserIsExternal",
                        "UserPasswordFormat",
                        "UserCreated",
                        "LastLogon",
                        "UserStartingAliasPath",
                        "UserGUID",
                        "UserLastModified",
                        "UserLastLogonInfo",
                        "UserIsHidden",
                        "UserVisibility",
                        "UserIsDomain",
                        "UserHasAllowedCultures",
                        "UserPrivilegeLevel"
                        );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.objectversionhistory", "selectlist");
        }

        #endregion
    }
}