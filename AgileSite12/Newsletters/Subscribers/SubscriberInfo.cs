using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Newsletters;
using CMS.ContactManagement;

[assembly: RegisterObjectType(typeof(SubscriberInfo), SubscriberInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(SubscriberInfo), SubscriberInfo.OBJECT_TYPE_CONTACTGROUP)]
[assembly: RegisterObjectType(typeof(SubscriberInfo), SubscriberInfo.OBJECT_TYPE_CONTACT)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Subscriber data container class.
    /// </summary>
    public class SubscriberInfo : AbstractInfo<SubscriberInfo>
    {

        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NEWSLETTERSUBSCRIBER;

        /// <summary>
        /// Object type for contact group
        /// </summary>
        public const string OBJECT_TYPE_CONTACTGROUP = PredefinedObjectType.NEWSLETTERCONTACTGROUP;

        /// <summary>
        /// Object type for contact
        /// </summary>
        public const string OBJECT_TYPE_CONTACT = "newsletter.contactsubscriber";


        /// <summary>
        /// Subscriber custom data.
        /// </summary>
        protected ContainerCustomData mSubscriberCustomData;


        private BaseInfo mSubscriberRelated;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SubscriberInfoProvider), OBJECT_TYPE, "Newsletter.Subscriber", "SubscriberID", "SubscriberLastModified", "SubscriberGUID", null, "SubscriberFullName", null, "SubscriberSiteID", null, null)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.NEWSLETTER,
            Feature = FeatureEnum.Subscribers,
            ContainsMacros = false,
            SupportsCloning = false,
            SupportsCloneToOtherSite = false
        };


        /// <summary>
        /// Type information for contact group subscriber.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOCONTACTGROUP = new ObjectTypeInfo(typeof(SubscriberInfoProvider), OBJECT_TYPE_CONTACTGROUP, "Newsletter.Subscriber", "SubscriberID", "SubscriberLastModified", "SubscriberGUID", null, "SubscriberFullName", null, "SubscriberSiteID", null, null)
        {
            OriginalTypeInfo = TYPEINFO,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SubscriberRelatedID", ContactGroupInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            MacroCollectionName = "CMS.NewsletterContactGroupSubscriber",
            ModuleName = ModuleName.NEWSLETTER,
            Feature = FeatureEnum.Subscribers,
            ContainsMacros = false,
            AllowRestore = false,
            SupportsCloning = false,
            SupportsCloneToOtherSite = false,
            TypeCondition = new TypeCondition().WhereEquals("SubscriberType", PredefinedObjectType.CONTACTGROUP),
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false,
                AllowSingleExport = false,
                IsExportable = false
            }
        };


        /// <summary>
        /// Type information for contact subscriber.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOCONTACT = new ObjectTypeInfo(typeof(SubscriberInfoProvider), OBJECT_TYPE_CONTACT, "Newsletter.Subscriber", "SubscriberID", "SubscriberLastModified", "SubscriberGUID", null, "SubscriberFullName", null, "SubscriberSiteID", null, null)
        {
            OriginalTypeInfo = TYPEINFO,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SubscriberRelatedID", ContactInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.NEWSLETTER,
            Feature = FeatureEnum.Subscribers,
            ContainsMacros = false,
            MacroCollectionName = "CMS.NewsletterContactSubscriber",
            AllowRestore = false,
            SupportsCloning = false,
            SupportsCloneToOtherSite = false,
            TypeCondition = new TypeCondition().WhereEquals("SubscriberType", PredefinedObjectType.CONTACT),
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false,
                AllowSingleExport = false,
                IsExportable = false
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// SubscriberID.
        /// </summary>
        [DatabaseField]
        public virtual int SubscriberID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SubscriberID"), 0);
            }
            set
            {
                SetValue("SubscriberID", value);
            }
        }


        /// <summary>
        /// SubscriberEmail.
        /// </summary>
        [DatabaseField]
        public virtual string SubscriberEmail
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubscriberEmail"), string.Empty);
            }
            set
            {
                SetValue("SubscriberEmail", value);
            }
        }


        /// <summary>
        /// SubscriberFirstName.
        /// </summary>
        [DatabaseField]
        public virtual string SubscriberFirstName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubscriberFirstName"), string.Empty);
            }
            set
            {
                SetValue("SubscriberFirstName", value);
            }
        }


        /// <summary>
        /// SubscriberLastName.
        /// </summary>
        [DatabaseField]
        public virtual string SubscriberLastName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubscriberLastName"), string.Empty);
            }
            set
            {
                SetValue("SubscriberLastName", value);
            }
        }


        /// <summary>
        /// SubscriberFullName.
        /// </summary>
        [DatabaseField]
        public virtual string SubscriberFullName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubscriberFullName"), string.Empty);
            }
            set
            {
                SetValue("SubscriberFullName", value);
            }
        }


        /// <summary>
        /// SubscriberSiteID.
        /// </summary>
        [DatabaseField]
        public virtual int SubscriberSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SubscriberSiteID"), 0);
            }
            set
            {
                SetValue("SubscriberSiteID", value);
            }
        }


        /// <summary>
        /// SubscriberType.
        /// </summary>
        [DatabaseField]
        public virtual string SubscriberType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubscriberType"), null);
            }
            set
            {
                SetValue("SubscriberType", value);
            }
        }


        /// <summary>
        /// SubscriberRelatedID.
        /// </summary>
        [DatabaseField]
        public virtual int SubscriberRelatedID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SubscriberRelatedID"), 0);
            }
            set
            {
                SetValue("SubscriberRelatedID", value);
            }
        }


        /// <summary>
        /// Subscriber custom data.
        /// </summary>
        [DatabaseField]
        public ContainerCustomData SubscriberCustomData
        {
            get
            {
                if (mSubscriberCustomData == null)
                {
                    mSubscriberCustomData = new ContainerCustomData(this, "SubscriberCustomData");
                }
                return mSubscriberCustomData;
            }
        }


        /// <summary>
        /// Subscriber GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid SubscriberGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("SubscriberGUID"), Guid.Empty);
            }
            set
            {
                SetValue("SubscriberGUID", value);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SubscriberLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SubscriberLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriberLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets the number of bounced e-mails for this subscriber.
        /// </summary>
        [DatabaseField]
        public virtual int SubscriberBounces
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SubscriberBounces"), 0);
            }
            set
            {
                SetValue("SubscriberBounces", value, (value > 0));
            }
        }


        /// <summary>
        /// Subscriber related object (contact, contact group).
        /// </summary>
        public virtual BaseInfo SubscriberRelated
        {
            get
            {
                if ((mSubscriberRelated == null) && !string.IsNullOrEmpty(SubscriberType))
                {
                    // Create subscriber related info
                    mSubscriberRelated = ProviderHelper.GetInfoById(SubscriberType, SubscriberRelatedID);
                }
                return mSubscriberRelated;
            }
            internal set
            {
                mSubscriberRelated = value;
            }
        }


        /// <summary>
        /// Indicates fake subscriber. <see cref="IFakeSubscriberService"/>
        /// </summary>
        internal bool SubscriberIsFake
        {
            get; set;
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (SubscriberType == null)
                {
                    return TYPEINFO;
                }

                switch (SubscriberType.ToLowerCSafe())
                {
                    // Contact group subscriber
                    case PredefinedObjectType.CONTACTGROUP:
                        return TYPEINFOCONTACTGROUP;

                    // Contact subscriber
                    case PredefinedObjectType.CONTACT:
                        return TYPEINFOCONTACT;

                    // Standard subscriber
                    default:
                        return TYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SubscriberInfoProvider.DeleteSubscriberInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SubscriberInfoProvider.SetSubscriberInfo(this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                this.SubscriberEmail = ValidationHelper.GetString(p[PredefinedObjectType.NEWSLETTERSUBSCRIBER + ".email"], this.SubscriberEmail);
                this.SubscriberFirstName = ValidationHelper.GetString(p[PredefinedObjectType.NEWSLETTERSUBSCRIBER + ".firstname"], this.SubscriberFirstName);
                this.SubscriberLastName = ValidationHelper.GetString(p[PredefinedObjectType.NEWSLETTERSUBSCRIBER + ".lastname"], this.SubscriberLastName);
                this.SubscriberFullName = (this.SubscriberFirstName + " " + this.SubscriberLastName);
            }

            this.Insert();
        }


        /// <summary>
        /// Register the custom properties.
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("SubscriberRelated", c => c.SubscriberRelated);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty Subscriber object.
        /// </summary>
        public SubscriberInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new Subscriber object from the existing Subscriber object.
        /// Copy constructor.
        /// </summary>
        /// <param name="info">Original object to copy</param>
        /// <param name="keepSourceData">If true, the copy is shallow, otherwise a deep copy is created (all object's data is cloned)</param>
        public SubscriberInfo(SubscriberInfo info, bool keepSourceData)
            : base(TYPEINFO, info.DataClass, keepSourceData)
        {
        }


        /// <summary>
        /// Constructor - Creates a new Subscriber object from the given DataRow.
        /// </summary>
        public SubscriberInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>ManageSubscribers permission name for managing permission type, or base permission name otherwise</returns>
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