using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(VisitorToContactInfo), VisitorToContactInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// VisitorToContactInfo data container class.
    /// </summary>
	[Serializable]
    public class VisitorToContactInfo : AbstractInfo<VisitorToContactInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.visitortocontact";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VisitorToContactInfoProvider), OBJECT_TYPE, "OM.VisitorToContact", "VisitorToContactID", null, null, null, null, null, null, null, null)
        {
            ModuleName = "CMS.ContactManagement",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("VisitorToContactContactID", "om.contact", ObjectDependencyEnum.Required),
            },
            Feature = FeatureEnum.FullContactManagement,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Visitor to contact ID
        /// </summary>
        [DatabaseField]
        public virtual int VisitorToContactID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VisitorToContactID"), 0);
            }
            set
            {
                SetValue("VisitorToContactID", value);
            }
        }


        /// <summary>
        /// Visitor to contact visitor GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid VisitorToContactVisitorGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("VisitorToContactVisitorGUID"), Guid.Empty);
            }
            set
            {
                SetValue("VisitorToContactVisitorGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Visitor to contact contact ID
        /// </summary>
        [DatabaseField]
        public virtual int VisitorToContactContactID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VisitorToContactContactID"), 0);
            }
            set
            {
                SetValue("VisitorToContactContactID", value, 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VisitorToContactInfoProvider.DeleteVisitorToContactInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            VisitorToContactInfoProvider.SetVisitorToContactInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected VisitorToContactInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty VisitorToContactInfo object.
        /// </summary>
        public VisitorToContactInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new VisitorToContactInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public VisitorToContactInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}