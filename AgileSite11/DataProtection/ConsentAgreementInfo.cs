using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.DataProtection;

[assembly: RegisterObjectType(typeof(ConsentAgreementInfo), ConsentAgreementInfo.OBJECT_TYPE)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Data container class for <see cref="ConsentAgreementInfo"/>.
    /// </summary>
    [Serializable]
    public class ConsentAgreementInfo : AbstractInfo<ConsentAgreementInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.consentagreement";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ConsentAgreementInfoProvider), OBJECT_TYPE, "CMS.ConsentAgreement", "ConsentAgreementID", null, "ConsentAgreementGuid", null, null, null, null, "ConsentAgreementContactID", ContactInfo.OBJECT_TYPE)
        {
            ModuleName = DataProtectionModule.MODULE_NAME,
            Feature = FeatureEnum.DataProtection,
            TouchCacheDependencies = true,
            IsBinding = true,
            ContainsMacros = false,
            LogEvents = false,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ConsentAgreementConsentID", ConsentInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            }
        };


        /// <summary>
        /// Consent agreement ID
        /// </summary>
		[DatabaseField]
        public virtual int ConsentAgreementID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConsentAgreementID"), 0);
            }
            set
            {
                SetValue("ConsentAgreementID", value);
            }
        }


        /// <summary>
        /// Consent agreement guid
        /// </summary>
		[DatabaseField]
        public virtual Guid ConsentAgreementGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ConsentAgreementGuid"), Guid.Empty);
            }
            set
            {
                SetValue("ConsentAgreementGuid", value);
            }
        }


        /// <summary>
        /// Contact ID
        /// </summary>
		[DatabaseField]
        public virtual int ConsentAgreementContactID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConsentAgreementContactID"), 0);
            }
            set
            {
                SetValue("ConsentAgreementContactID", value);
            }
        }


        /// <summary>
        /// Consent ID
        /// </summary>
		[DatabaseField]
        public virtual int ConsentAgreementConsentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConsentAgreementConsentID"), 0);
            }
            set
            {
                SetValue("ConsentAgreementConsentID", value);
            }
        }


        /// <summary>
        /// Indicates if consent agreement was revoked.
        /// </summary>
        [DatabaseField]
        public virtual bool ConsentAgreementRevoked
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ConsentAgreementRevoked"), false);
            }
            set
            {
                SetValue("ConsentAgreementRevoked", value);
            }
        }


        /// <summary>
        /// Consent hash
        /// </summary>
		[DatabaseField]
        public virtual string ConsentAgreementConsentHash
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentAgreementConsentHash"), String.Empty);
            }
            set
            {
                SetValue("ConsentAgreementConsentHash", value, String.Empty);
            }
        }


        /// <summary>
        /// Gets or sets a time of a consent action
        /// </summary>
        [DatabaseField]
        public virtual DateTime ConsentAgreementTime
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ConsentAgreementTime"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ConsentAgreementTime", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ConsentAgreementInfoProvider.DeleteConsentAgreementInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ConsentAgreementInfoProvider.SetConsentAgreementInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ConsentAgreementInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ConsentAgreementInfo"/> class.
        /// </summary>
        public ConsentAgreementInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="ConsentAgreementInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ConsentAgreementInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}