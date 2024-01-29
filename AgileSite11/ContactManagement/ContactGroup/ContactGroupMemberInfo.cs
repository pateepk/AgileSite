using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ContactGroupMemberInfo), ContactGroupMemberInfo.OBJECT_TYPE_CONTACT)]
[assembly: RegisterObjectType(typeof(ContactGroupMemberInfo), ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactGroupMembersInfo data container class.
    /// </summary>
    public class ContactGroupMemberInfo : AbstractInfo<ContactGroupMemberInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type for contact
        /// </summary>
        public const string OBJECT_TYPE_CONTACT = PredefinedObjectType.CONTACTGROUPMEMBERCONTACT;

        /// <summary>
        /// Object type for account
        /// </summary>
        public const string OBJECT_TYPE_ACCOUNT = "om.contactgroupmemberaccount";


        /// <summary>
        /// Type information for group members of type contact.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOCONTACT = new ObjectTypeInfo(typeof(ContactGroupMemberInfoProvider), OBJECT_TYPE_CONTACT, "OM.ContactGroupMember", "ContactGroupMemberID", null, null, null, null, null, null, "ContactGroupMemberRelatedID", ContactInfo.OBJECT_TYPE)
                                                     {
                                                         DependsOn = new List<ObjectDependency>
                                                             {
                                                                 new ObjectDependency("ContactGroupMemberContactGroupID", ContactGroupInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
                                                             },

                                                         SynchronizationSettings =
                                                         {
                                                             IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                             LogSynchronization = SynchronizationTypeEnum.None
                                                         },
                                                         LogEvents = false,
                                                         TouchCacheDependencies = true,
                                                         TypeCondition = new TypeCondition().WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact),
                                                         IsBinding = true,
                                                         ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
                                                         Feature = FeatureEnum.SimpleContactManagement,
                                                         ContainsMacros = false,
                                                     };


        /// <summary>
        /// Type information for group members of type account.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOACCOUNT = new ObjectTypeInfo(typeof(ContactGroupMemberInfoProvider), OBJECT_TYPE_ACCOUNT, "OM.ContactGroupMember", "ContactGroupMemberID", null, null, null, null, null, null, "ContactGroupMemberRelatedID", AccountInfo.OBJECT_TYPE)
                                                     {
                                                         DependsOn = new List<ObjectDependency>
                                                             {
                                                                 new ObjectDependency("ContactGroupMemberContactGroupID", ContactGroupInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
                                                             },

                                                         SynchronizationSettings =
                                                         {
                                                             IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                             LogSynchronization = SynchronizationTypeEnum.None
                                                         },
                                                         LogEvents = false,
                                                         TouchCacheDependencies = true,
                                                         TypeCondition = new TypeCondition().WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Account),
                                                         IsBinding = true,
                                                         ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
                                                         Feature = FeatureEnum.SimpleContactManagement
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public virtual int ContactGroupMemberID
        {
            get
            {
                return GetIntegerValue("ContactGroupMemberID", 0);
            }
            set
            {
                SetValue("ContactGroupMemberID", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the related object.
        /// </summary>
        public virtual ContactGroupMemberTypeEnum ContactGroupMemberType
        {
            get
            {
                return (ContactGroupMemberTypeEnum)GetIntegerValue("ContactGroupMemberType", 0);
            }
            set
            {
                SetValue("ContactGroupMemberType", (int)value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the contact group.
        /// </summary>
        public virtual int ContactGroupMemberContactGroupID
        {
            get
            {
                return GetIntegerValue("ContactGroupMemberContactGroupID", 0);
            }
            set
            {
                SetValue("ContactGroupMemberContactGroupID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the related object - contact or account.
        /// </summary>
        public virtual int ContactGroupMemberRelatedID
        {
            get
            {
                return GetIntegerValue("ContactGroupMemberRelatedID", 0);
            }
            set
            {
                SetValue("ContactGroupMemberRelatedID", value);
            }
        }


        /// <summary>
        /// Gets or sets value indicating if current contact group member is added from dynamic condition.
        /// </summary>
        public virtual bool ContactGroupMemberFromCondition
        {
            get
            {
                return GetBooleanValue("ContactGroupMemberFromCondition", false);
            }
            set
            {
                SetValue("ContactGroupMemberFromCondition", value);
            }
        }


        /// <summary>
        /// Gets or sets value indicating if current contact group member is added as an account member.
        /// </summary>
        public virtual bool ContactGroupMemberFromAccount
        {
            get
            {
                return GetBooleanValue("ContactGroupMemberFromAccount", false);
            }
            set
            {
                SetValue("ContactGroupMemberFromAccount", value);
            }
        }


        /// <summary>
        /// Gets or sets value indicating if current contact group member is manually added.
        /// </summary>
        public virtual bool ContactGroupMemberFromManual
        {
            get
            {
                return GetBooleanValue("ContactGroupMemberFromManual", false);
            }
            set
            {
                SetValue("ContactGroupMemberFromManual", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactGroupMemberInfoProvider.DeleteContactGroupMemberInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(this);
        }


        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                switch (ContactGroupMemberType)
                {
                    // Contact
                    default:
                    case ContactGroupMemberTypeEnum.Contact:
                        return TYPEINFOCONTACT;

                    // Account
                    case ContactGroupMemberTypeEnum.Account:
                        return TYPEINFOACCOUNT;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactGroupMemberInfo object.
        /// </summary>
        public ContactGroupMemberInfo()
            : base(TYPEINFOCONTACT)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactGroupMemberInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactGroupMemberInfo(DataRow dr)
            : base(TYPEINFOCONTACT, dr)
        {
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
            if (this.ContactGroupMemberFromManual)
            {
                // Clone only manually added members
                this.Insert();
            }
        }

        #endregion
    }
}