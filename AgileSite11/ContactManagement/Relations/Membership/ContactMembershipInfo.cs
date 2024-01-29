using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(ContactMembershipInfo), ContactMembershipInfo.OBJECT_TYPE_USER)]
[assembly: RegisterObjectType(typeof(ContactMembershipInfo), ContactMembershipInfo.OBJECT_TYPE_CUSTOMER)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactMembershipInfo data container class.
    /// </summary>
    public class ContactMembershipInfo : AbstractInfo<ContactMembershipInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type for user
        /// </summary>
        public const string OBJECT_TYPE_USER = "om.membershipuser";

        /// <summary>
        /// Object type for customer
        /// </summary>
        public const string OBJECT_TYPE_CUSTOMER = "om.membershipcustomer";


        /// <summary>
        /// Type information for user membership relations.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOUSER = new ObjectTypeInfo(typeof(ContactMembershipInfoProvider), OBJECT_TYPE_USER, "OM.Membership", "MembershipID", null, "MembershipGUID", null, null, null, null, "RelatedID", UserInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ContactID", ContactInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            TypeCondition = new TypeCondition().WhereEquals("MemberType", (int)MemberTypeEnum.CmsUser),
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            Feature = FeatureEnum.SimpleContactManagement,
            ContainsMacros = false,
        };


        /// <summary>
        /// Type information for ecommerce customer membership relations.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOCUSTOMER = new ObjectTypeInfo(typeof(ContactMembershipInfoProvider), OBJECT_TYPE_CUSTOMER, "OM.Membership", "MembershipID", null, "MembershipGUID", null, null, null, null, "RelatedID", PredefinedObjectType.CUSTOMER)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ContactID", ContactInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            TypeCondition = new TypeCondition().WhereEquals("MemberType", (int)MemberTypeEnum.EcommerceCustomer),
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            Feature = FeatureEnum.FullContactManagement,
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the ID of relationship.
        /// </summary>
        public virtual int MembershipID
        {
            get
            {
                return GetIntegerValue("MembershipID", 0);
            }
            set
            {
                SetValue("MembershipID", value);
            }
        }

        
        /// <summary>
        /// Gets or sets the ID of the contact.
        /// </summary>
        public virtual int ContactID
        {
            get
            {
                return GetIntegerValue("ContactID", 0);
            }
            set
            {
                SetValue("ContactID", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of object this contact is related to.
        /// </summary>
        public virtual MemberTypeEnum MemberType
        {
            get
            {
                return (MemberTypeEnum)GetIntegerValue("MemberType", (int)MemberTypeEnum.CmsUser);
            }
            set
            {
                SetValue("MemberType", (int)value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the related object.
        /// </summary>
        public virtual int RelatedID
        {
            get
            {
                return GetIntegerValue("RelatedID", 0);
            }
            set
            {
                SetValue("RelatedID", value);
            }
        }


        /// <summary>
        /// Gets or sets the membership's unique identifier.
        /// </summary>
        public virtual Guid MembershipGUID
        {
            get
            {
                return GetGuidValue("MembershipGUID", Guid.Empty);
            }
            set
            {
                SetValue("MembershipGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time the membership object was created.
        /// </summary>
        public virtual DateTime MembershipCreated
        {
            get
            {
                return GetDateTimeValue("MembershipCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MembershipCreated", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactMembershipInfoProvider.DeleteMembershipInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactMembershipInfoProvider.SetMembershipInfo(this);
        }


        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                switch (MemberType)
                {
                    // Ecommerce customer
                    case MemberTypeEnum.EcommerceCustomer:
                        return TYPEINFOCUSTOMER;

                    // User
                    default:
                        return TYPEINFOUSER;
                }
            }
        }


        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        /// <returns>ContactMembershipInfo object</returns>
        protected override BaseInfo GetExisting()
        {
            return ContactMembershipInfoProvider.GetMembershipInfo(ContactID, RelatedID, MemberType);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactMembershipInfo object.
        /// </summary>
        public ContactMembershipInfo()
            : base(TYPEINFOUSER)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactMembershipInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactMembershipInfo(DataRow dr)
            : base(TYPEINFOUSER, dr)
        {
        }

        #endregion
    }
}