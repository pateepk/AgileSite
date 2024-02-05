using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Polls;

[assembly: RegisterObjectType(typeof(PollInfo), PollInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(PollInfo), PollInfo.OBJECT_TYPE_GROUP)]

namespace CMS.Polls
{
    /// <summary>
    /// PollInfo data container class.
    /// </summary>
    public class PollInfo : AbstractInfo<PollInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.POLL;

        /// <summary>
        /// Object type for group
        /// </summary>
        public const string OBJECT_TYPE_GROUP = "polls.grouppoll";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PollInfoProvider), OBJECT_TYPE, "Polls.Poll", "PollID", "PollLastModified", "PollGUID", "PollCodeName", "PollDisplayName", null, "PollSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                    new ObjectTreeLocation(GLOBAL, CONTENTMANAGEMENT),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            GroupIDColumn = "PollGroupID",
            ModuleName = ModuleName.POLLS,
            Feature = FeatureEnum.Polls,
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                    new ObjectTreeLocation(GLOBAL, CONTENTMANAGEMENT),
                },
            },
            TypeCondition = new TypeCondition().WhereIsNull("PollGroupID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information for group polls.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOGROUP = new ObjectTypeInfo(typeof(PollInfoProvider), OBJECT_TYPE_GROUP, "Polls.Poll", "PollID", "PollLastModified", "PollGUID", "PollCodeName", "PollDisplayName", null, "PollSiteID", "PollGroupID", PredefinedObjectType.GROUP)
            {
                SynchronizationSettings =
                {
                    LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                    IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                    ObjectTreeLocations = new List<ObjectTreeLocation>()
                    {
                        new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                    }
                },
                LogEvents = true,
                TouchCacheDependencies = true,
                OriginalTypeInfo = TYPEINFO,
                GroupIDColumn = "PollGroupID",
                ModuleName = ModuleName.POLLS,
                ImportExportSettings = { AllowSingleExport = false, LogExport = true, },
                TypeCondition = new TypeCondition().WhereIsNotNull("PollGroupID"),
                ContinuousIntegrationSettings =
                {
                    Enabled = true
                }
            };

        #endregion


        #region "Roles and sites information"

        private Hashtable mAllowedRoles = null;
        private Hashtable mSites = null;


        /// <summary>
        /// Hashtable of allowed roles. RoleName is key and RoleID is value.
        /// </summary>
        public Hashtable AllowedRoles
        {
            get
            {
                LoadRoles();
                return mAllowedRoles;
            }
        }


        /// <summary>
        /// Hashtable of sites where the poll is assigned.
        /// SiteName is key and SiteID is value.
        /// </summary>
        public Hashtable Sites
        {
            get
            {
                LoadSites();
                return mSites;
            }
        }


        /// <summary>
        /// Loads the table of allowed roles.
        /// </summary>
        private void LoadRoles()
        {
            if (mAllowedRoles == null)
            {
                Hashtable table = new Hashtable();

                // Load the roles if role access initialized
                if (PollAccess == SecurityAccessEnum.AuthorizedRoles)
                {
                    // Get the roles
                    DataSet ds = PollInfoProvider.GetPollRoles(PollID);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        DataRowCollection rows = ds.Tables[0].Rows;
                        foreach (DataRow row in rows)
                        {
                            // Store just role ID (save the memory, important information is that the key is present)
                            int siteID = ValidationHelper.GetInteger(row["SiteID"], 0);
                            string roleName = ValidationHelper.GetString(row["RoleName"], "");

                            if (siteID == 0)
                            {
                                roleName = "." + roleName;
                            }

                            int roleId = ValidationHelper.GetInteger(row["RoleID"], 0);

                            table[roleName.ToLowerCSafe()] = roleId;
                        }
                    }
                }

                mAllowedRoles = table;
            }
        }


        /// <summary>
        /// Loads the table of sites.
        /// </summary>
        private void LoadSites()
        {
            if (mSites == null)
            {
                Hashtable table = new Hashtable();

                // Get the sites
                DataSet ds = PollInfoProvider.GetPollSites(PollID);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    DataRowCollection rows = ds.Tables[0].Rows;
                    foreach (DataRow row in rows)
                    {
                        // Store just site ID
                        string siteName = ValidationHelper.GetString(row["SiteName"], "");
                        int siteId = ValidationHelper.GetInteger(row["SiteID"], 0);

                        table[siteName.ToLowerCSafe()] = siteId;
                    }
                }

                mSites = table;
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the poll.
        /// </summary>
        public virtual int PollID
        {
            get
            {
                return GetIntegerValue("PollID", 0);
            }
            set
            {
                SetValue("PollID", value);
            }
        }


        /// <summary>
        /// Code name of the poll.
        /// </summary>
        public virtual string PollCodeName
        {
            get
            {
                return GetStringValue("PollCodeName", "");
            }
            set
            {
                SetValue("PollCodeName", value);
            }
        }


        /// <summary>
        /// Display name of the poll.
        /// </summary>
        public virtual string PollDisplayName
        {
            get
            {
                return GetStringValue("PollDisplayName", "");
            }
            set
            {
                SetValue("PollDisplayName", value);
            }
        }


        /// <summary>
        /// Title of the poll.
        /// </summary>
        public virtual string PollTitle
        {
            get
            {
                return GetStringValue("PollTitle", "");
            }
            set
            {
                SetValue("PollTitle", value);
            }
        }


        /// <summary>
        /// Question of the poll.
        /// </summary>
        public virtual string PollQuestion
        {
            get
            {
                return GetStringValue("PollQuestion", "");
            }
            set
            {
                SetValue("PollQuestion", value);
            }
        }


        /// <summary>
        /// Response message displayed after voting.
        /// </summary>
        public virtual string PollResponseMessage
        {
            get
            {
                return GetStringValue("PollResponseMessage", "");
            }
            set
            {
                SetValue("PollResponseMessage", value);
            }
        }


        /// <summary>
        /// Indicates from when is poll opened.
        /// </summary>
        public virtual DateTime PollOpenFrom
        {
            get
            {
                return GetDateTimeValue("PollOpenFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value != DateTimeHelper.ZERO_TIME)
                {
                    SetValue("PollOpenFrom", value);
                }
                else
                {
                    SetValue("PollOpenFrom", null);
                }
            }
        }


        /// <summary>
        /// Indicates close time for the poll.
        /// </summary>
        public virtual DateTime PollOpenTo
        {
            get
            {
                return GetDateTimeValue("PollOpenTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value != DateTimeHelper.ZERO_TIME)
                {
                    SetValue("PollOpenTo", value);
                }
                else
                {
                    SetValue("PollOpenTo", null);
                }
            }
        }


        /// <summary>
        /// Indicates security access for the poll.
        /// </summary>
        public virtual SecurityAccessEnum PollAccess
        {
            get
            {
                return (SecurityAccessEnum)ValidationHelper.GetInteger(GetValue("PollAccess"), 0);
            }
            set
            {
                SetValue("PollAccess", Convert.ToInt32(value));
            }
        }


        /// <summary>
        /// Indicates if poll allows multiple answers.
        /// </summary>
        public virtual bool PollAllowMultipleAnswers
        {
            get
            {
                return GetBooleanValue("PollAllowMultipleAnswers", false);
            }
            set
            {
                SetValue("PollAllowMultipleAnswers", value);
            }
        }


        /// <summary>
        /// Poll GUID.
        /// </summary>
        public virtual Guid PollGUID
        {
            get
            {
                return GetGuidValue("PollGUID", Guid.Empty);
            }
            set
            {
                SetValue("PollGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime PollLastModified
        {
            get
            {
                return GetDateTimeValue("PollLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PollLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// ID of a group where the poll is placed.
        /// </summary>
        public virtual int PollGroupID
        {
            get
            {
                return GetIntegerValue("PollGroupID", 0);
            }
            set
            {
                SetValue("PollGroupID", value);
            }
        }


        /// <summary>
        /// ID of the site where poll is located.
        /// </summary>
        public virtual int PollSiteID
        {
            get
            {
                return GetIntegerValue("PollSiteID", 0);
            }
            set
            {
                SetValue("PollSiteID", value);
            }
        }


        /// <summary>
        /// Indicates if poll activity is logged.
        /// </summary>
        public virtual bool PollLogActivity
        {
            get
            {
                return GetBooleanValue("PollLogActivity", false);
            }
            set
            {
                SetValue("PollLogActivity", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (PollGroupID == 0)
                {
                    return TYPEINFO;
                }
                else
                {
                    return TYPEINFOGROUP;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PollInfoProvider.DeletePollInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PollInfoProvider.SetPollInfo(this);
        }


        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        public override bool CheckUniqueCodeName()
        {
            // Child object (unique within the given parent)
            return CheckUniqueValues("PollCodeName", "PollSiteID", "PollGroupID");
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PollInfo object.
        /// </summary>
        public PollInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PollInfo object from the given DataRow.
        /// </summary>
        public PollInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns true, if the poll is assigned to the site.
        /// </summary>
        /// <param name="siteName">Sitename to check</param>
        public bool IsInSite(string siteName)
        {
            return (Sites[siteName.ToLowerCSafe()] != null);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // This binding has to be cloned in the postprocessing phase (because of cloning of group roles)
            settings.ExcludedBindingTypes.Add(PollRoleInfo.OBJECT_TYPE);

            Insert();
        }


        /// <summary>
        /// Clones forum role and forum moderator bindings
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            if (settings.IncludeBindings)
            {
                // Clone project role permissions
                DataSet ds = PollRoleInfoProvider.GetPollRoleInfos("PollID = " + originalObject.Generalized.ObjectID, null, -1, null);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    int originalParentId = settings.ParentID;
                    settings.ParentID = PollID;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        PollRoleInfo binding = new PollRoleInfo(dr);
                        int newRoleId = settings.Translations.GetNewID(RoleInfo.OBJECT_TYPE_GROUP, binding.RoleID, null, 0, null, null, null);
                        if (newRoleId > 0)
                        {
                            binding.RoleID = newRoleId;
                        }
                        binding.Generalized.InsertAsClone(settings, result);
                    }

                    settings.ParentID = originalParentId;
                }
            }
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return PollInfoProvider.CheckLicense(action, domainName);
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    if ((PollGroupID > 0) && (PollSiteID > 0))
                    {
                        return UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "read", siteName, (UserInfo)userInfo, exceptionOnFailure);
                    }
                    if (PollSiteID <= 0)
                    {
                        return UserInfoProvider.IsAuthorizedPerResource(ModuleName.POLLS, "globalread", siteName, (UserInfo)userInfo, exceptionOnFailure);
                    }
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    if ((PollGroupID > 0) && (PollSiteID > 0))
                    {
                        return UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "manage", siteName, (UserInfo)userInfo, exceptionOnFailure);
                    }
                    if (PollSiteID <= 0)
                    {
                        return UserInfoProvider.IsAuthorizedPerResource(ModuleName.POLLS, "globalmodify", siteName, (UserInfo)userInfo, exceptionOnFailure);
                    }
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}