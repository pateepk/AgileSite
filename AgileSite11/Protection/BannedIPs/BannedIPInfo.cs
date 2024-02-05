using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Protection;

[assembly: RegisterObjectType(typeof(BannedIPInfo), BannedIPInfo.OBJECT_TYPE)]

namespace CMS.Protection
{
    /// <summary>
    /// BannedIPInfo data container class.
    /// </summary>
    public class BannedIPInfo : AbstractInfo<BannedIPInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.bannedip";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BannedIPInfoProvider), OBJECT_TYPE, "CMS.BannedIP", "IPAddressID", "IPAddressLastModified", "IPAddressGUID", null, "IPAddress", null, "IPAddressSiteID", null, null)
        {
            ModuleName = "cms.bannedip",
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                    {
                        new ObjectTreeLocation(SITE, CONFIGURATION),
                        new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                    }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            SupportsGlobalObjects = true,
            EnabledColumn = "IPAddressBanEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields =
                {
                    "IPAddress",
                    "IPAddressBanType"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// IP address ID.
        /// </summary>
        public virtual int IPAddressID
        {
            get
            {
                return GetIntegerValue("IPAddressID", 0);
            }
            set
            {
                SetValue("IPAddressID", value);
            }
        }


        /// <summary>
        /// IP address site ID.
        /// </summary>
        public virtual int IPAddressSiteID
        {
            get
            {
                return GetIntegerValue("IPAddressSiteID", 0);
            }
            set
            {
                SetValue("IPAddressSiteID", value, 0);
            }
        }


        /// <summary>
        /// Indicated whether IP address is allowed.
        /// </summary>
        public virtual bool IPAddressAllowed
        {
            get
            {
                return GetBooleanValue("IPAddressAllowed", false);
            }
            set
            {
                SetValue("IPAddressAllowed", value);
            }
        }


        /// <summary>
        /// Ban type of IP address.
        /// </summary>
        public virtual string IPAddressBanType
        {
            get
            {
                return GetStringValue("IPAddressBanType", "");
            }
            set
            {
                SetValue("IPAddressBanType", value);
            }
        }


        /// <summary>
        /// Indicates whether ban can be overridden.
        /// </summary>
        public virtual bool IPAddressAllowOverride
        {
            get
            {
                return GetBooleanValue("IPAddressAllowOverride", false);
            }
            set
            {
                SetValue("IPAddressAllowOverride", value);
            }
        }


        /// <summary>
        /// Indicates whether ban is enabled.
        /// </summary>
        public virtual bool IPAddressBanEnabled
        {
            get
            {
                return GetBooleanValue("IPAddressBanEnabled", false);
            }
            set
            {
                SetValue("IPAddressBanEnabled", value);
            }
        }


        /// <summary>
        /// IP address.
        /// </summary>
        public virtual string IPAddress
        {
            get
            {
                return GetStringValue("IPAddress", "");
            }
            set
            {
                SetValue("IPAddress", value);
                SetValue("IPAddressRegular", BannedIPInfoProvider.GetRegularIPAddress(value));
            }
        }


        /// <summary>
        /// IP Address regular expression.
        /// </summary>
        public virtual string IPAddressRegular
        {
            get
            {
                return GetStringValue("IPAddressRegular", "");
            }
        }


        /// <summary>
        /// Reason of ban.
        /// </summary>
        public virtual string IPAddressBanReason
        {
            get
            {
                return GetStringValue("IPAddressBanReason", "");
            }
            set
            {
                SetValue("IPAddressBanReason", value);
            }
        }


        /// <summary>
        /// IP Address GUID.
        /// </summary>
        public virtual Guid IPAddressGUID
        {
            get
            {
                return GetGuidValue("IPAddressGUID", Guid.Empty);
            }
            set
            {
                SetValue("IPAddressGUID", value);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime IPAddressLastModified
        {
            get
            {
                return GetDateTimeValue("IPAddressLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("IPAddressLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BannedIPInfoProvider.DeleteBannedIPInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BannedIPInfoProvider.SetBannedIPInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BannedIPInfo object.
        /// </summary>
        public BannedIPInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BannedIPInfo object from the given DataRow.
        /// </summary>
        public BannedIPInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}