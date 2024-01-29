using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(IssueContactGroupInfo), IssueContactGroupInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// IssueContactGroupInfo data container class.
    /// </summary>
	[Serializable]
    public class IssueContactGroupInfo : AbstractInfo<IssueContactGroupInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.issuecontactgroup";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IssueContactGroupInfoProvider), OBJECT_TYPE, "newsletter.issuecontactgroup", "IssueContactGroupID", null, null, null, null, null, null, "IssueID", PredefinedObjectType.NEWSLETTERISSUE)
        {
            ModuleName = "CMS.Newsletter",
            TouchCacheDependencies = true,
            IsBinding = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ContactGroupID", PredefinedObjectType.CONTACTGROUP, ObjectDependencyEnum.Binding),
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Issue contact group ID
        /// </summary>
        [DatabaseField]
        public virtual int IssueContactGroupID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueContactGroupID"), 0);
            }
            set
            {
                SetValue("IssueContactGroupID", value);
            }
        }


        /// <summary>
        /// Issue ID
        /// </summary>
        [DatabaseField]
        public virtual int IssueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueID"), 0);
            }
            set
            {
                SetValue("IssueID", value);
            }
        }


        /// <summary>
        /// Contact group ID
        /// </summary>
        [DatabaseField]
        public virtual int ContactGroupID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ContactGroupID"), 0);
            }
            set
            {
                SetValue("ContactGroupID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IssueContactGroupInfoProvider.DeleteIssueContactGroupInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IssueContactGroupInfoProvider.SetIssueContactGroupInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected IssueContactGroupInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty IssueContactGroupInfo object.
        /// </summary>
        public IssueContactGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IssueContactGroupInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public IssueContactGroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>Configure permission name for managing permission type, or base permission name otherwise</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                    return "ManageSubscribers";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}