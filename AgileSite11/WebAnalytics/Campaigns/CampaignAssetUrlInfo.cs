using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(CampaignAssetUrlInfo), CampaignAssetUrlInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// CampaignAssetUrlInfo data container class.
    /// </summary>
	[Serializable]
    public class CampaignAssetUrlInfo : AbstractInfo<CampaignAssetUrlInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.campaignasseturl";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CampaignAssetUrlInfoProvider), OBJECT_TYPE, "Analytics.CampaignAssetUrl", "CampaignAssetUrlID", null, "CampaignAssetUrlGuid", null, "CampaignAssetUrlPageTitle", null, null, "CampaignAssetUrlCampaignAssetID", CampaignAssetInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            TouchCacheDependencies = true,

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization
            },

            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CampaignAssetUrlCampaignAssetID", "analytics.campaignasset", ObjectDependencyEnum.Required),
            },

            ContinuousIntegrationSettings =
            {
                Enabled = true
            },

            Feature = FeatureEnum.CampaignAndConversions,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Campaign asset url ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignAssetUrlID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignAssetUrlID"), 0);
            }
            set
            {
                SetValue("CampaignAssetUrlID", value);
            }
        }


        /// <summary>
        /// Campaign asset url guid
        /// </summary>
        [DatabaseField]
        public virtual Guid CampaignAssetUrlGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("CampaignAssetUrlGuid"), Guid.Empty);
            }
            set
            {
                SetValue("CampaignAssetUrlGuid", value);
            }
        }


        /// <summary>
        /// Campaign asset url target is <see cref="Uri.AbsolutePath"/> without scheme, port, domain, query strings, fragments
        /// and trailing slash at the end if <paramref name="value"/> is not root.
        /// See <see cref="CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget"/> method.
        /// </summary>
        /// <exception cref="InvalidOperationException">Inserted value contained scheme, port, domain, query strings or fragments.</exception>
        /// <example>
        /// <para>'http://your-domain.com/landing_page/' is not normalized.</para>
        /// <para>String.Empty is not normalized.</para>
        /// <para>'/' is normalized.</para>
        /// <para>'landing/page' is not normalized.</para>
        /// <para>'/landing/page' is normalized.</para>
        /// <para>'/landing/page/' is not normalized.</para>
        /// </example>
        /// <seealso cref="CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget"/>
        [DatabaseField]
        public virtual string CampaignAssetUrlTarget
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignAssetUrlTarget"), String.Empty);
            }
            set
            {
                if (!CampaignAssetUrlInfoHelper.IsNormalizedUrlTarget(value))
                {
                    throw new InvalidOperationException(String.Format("Value '{0}' contains invalid parts of URL, see 'CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget' method.", value));
                }

                SetValue("CampaignAssetUrlTarget", value);
            }
        }


        /// <summary>
        /// Campaign asset url page title
        /// </summary>
        [DatabaseField]
        public virtual string CampaignAssetUrlPageTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignAssetUrlPageTitle"), String.Empty);
            }
            set
            {
                SetValue("CampaignAssetUrlPageTitle", value);
            }
        }


        /// <summary>
        /// Campaign asset url campaign asset ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignAssetUrlCampaignAssetID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignAssetUrlCampaignAssetID"), 0);
            }
            set
            {
                SetValue("CampaignAssetUrlCampaignAssetID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CampaignAssetUrlInfoProvider.DeleteCampaignAssetUrlInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CampaignAssetUrlInfoProvider.SetCampaignAssetUrlInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public CampaignAssetUrlInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CampaignAssetUrlInfo object.
        /// </summary>
        public CampaignAssetUrlInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CampaignAssetUrlInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CampaignAssetUrlInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}