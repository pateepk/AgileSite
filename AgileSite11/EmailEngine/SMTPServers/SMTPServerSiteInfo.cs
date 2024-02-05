using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Base;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(SMTPServerSiteInfo), SMTPServerSiteInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// SMTPServerSiteInfo data container class.
    /// </summary>
    public class SMTPServerSiteInfo : AbstractInfo<SMTPServerSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.smtpserversite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SMTPServerSiteInfoProvider), OBJECT_TYPE, "CMS.SMTPServerSite", null, null, null, null, null, null, "SiteID", "ServerID", SMTPServerInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
            },
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                LogExport = true
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// Server ID.
        /// </summary>
        public virtual int ServerID
        {
            get
            {
                return GetIntegerValue("ServerID", 0);
            }
            set
            {
                SetValue("ServerID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SMTPServerSiteInfoProvider.DeleteSMTPServerSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SMTPServerSiteInfoProvider.SetSMTPServerSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SMTPServerSiteInfo object.
        /// </summary>
        public SMTPServerSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SMTPServerSiteInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SMTPServerSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}