using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(SMTPServerInfo), SMTPServerInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// SMTPServerInfo data container class.
    /// </summary>
    public class SMTPServerInfo : AbstractInfo<SMTPServerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.smtpserver";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SMTPServerInfoProvider), OBJECT_TYPE, "CMS.SMTPServer", "ServerID", "ServerLastModified", "ServerGUID", null, "ServerName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                    {
                        new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                    }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            EnabledColumn = "ServerEnabled",
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "ServerName" }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the SMTP server's ID.
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


        /// <summary>
        /// Gets or sets the name of the SMTP server.
        /// </summary>
        public virtual string ServerName
        {
            get
            {
                return GetStringValue("ServerName", String.Empty);
            }
            set
            {
                SetValue("ServerName", value);
            }
        }


        /// <summary>
        /// Gets or sets the user name for authentication.
        /// </summary>
        public virtual string ServerUserName
        {
            get
            {
                return GetStringValue("ServerUserName", String.Empty);
            }
            set
            {
                SetValue("ServerUserName", value, !String.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets the encrypted password for authentication.
        /// </summary>
        /// <seealso cref="EncryptionHelper.EncryptData"/>
        /// <seealso cref="EncryptionHelper.DecryptData"/>
        public virtual string ServerPassword
        {
            get
            {
                return GetStringValue("ServerPassword", String.Empty);
            }
            set
            {
                SetValue("ServerPassword", value, !String.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets if SSL should be used to encrypt the connection.
        /// </summary>
        public virtual bool ServerUseSSL
        {
            get
            {
                return GetBooleanValue("ServerUseSSL", false);
            }
            set
            {
                SetValue("ServerUseSSL", value);
            }
        }


        /// <summary>
        /// Gets or sets if the SMTP server is enabled.
        /// </summary>
        public virtual bool ServerEnabled
        {
            get
            {
                return GetBooleanValue("ServerEnabled", false);
            }
            set
            {
                SetValue("ServerEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the SMTP server priority.
        /// </summary>
        /// <value>The server priority.</value>
        public virtual SMTPServerPriorityEnum ServerPriority
        {
            get
            {
                return (SMTPServerPriorityEnum)GetIntegerValue("ServerPriority", (int)SMTPServerPriorityEnum.Normal);
            }
            set
            {
                SetValue("ServerPriority", (int)value);
            }
        }


        /// <summary>
        /// Gets or sets if the SMTP server is global.
        /// </summary>
        public virtual bool ServerIsGlobal
        {
            get
            {
                return GetBooleanValue("ServerIsGlobal", false);
            }
            set
            {
                SetValue("ServerIsGlobal", value);
            }
        }


        /// <summary>
        /// Gets or sets the server’s unique identifier.
        /// </summary>
        public virtual Guid ServerGUID
        {
            get
            {
                return GetGuidValue("ServerGUID", Guid.Empty);
            }
            set
            {
                SetValue("ServerGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the last modification.
        /// </summary>
        public virtual DateTime ServerLastModified
        {
            get
            {
                return GetDateTimeValue("ServerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ServerLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the SMTP server delivery method.
        /// </summary>
        public virtual SMTPServerDeliveryEnum ServerDeliveryMethod
        {
            get
            {
                return (SMTPServerDeliveryEnum)GetIntegerValue("ServerDeliveryMethod", (int)SMTPServerDeliveryEnum.Network);
            }
            set
            {
                SetValue("ServerDeliveryMethod", (int)value);
            }
        }


        /// <summary>
        /// Gets or sets pickup directory location of the SMTP server if <see cref="ServerDeliveryMethod"/> is set to <see cref="SMTPServerDeliveryEnum.SpecifiedPickupDirectory"/>.
        /// </summary>
        public virtual string ServerPickupDirectory
        {
            get
            {
                return GetStringValue("ServerPickupDirectory", String.Empty);
            }
            set
            {
                SetValue("ServerPickupDirectory", value, !String.IsNullOrEmpty(value));
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SMTPServerInfoProvider.DeleteSMTPServerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SMTPServerInfoProvider.SetSMTPServerInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new instance of SMTPServerInfo object.
        /// </summary>
        public SMTPServerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of SMTPServerInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SMTPServerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}