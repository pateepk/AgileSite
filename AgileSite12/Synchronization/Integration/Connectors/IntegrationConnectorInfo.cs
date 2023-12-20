using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(IntegrationConnectorInfo), IntegrationConnectorInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// IntegrationConnectorInfo data container class.
    /// </summary>
    public class IntegrationConnectorInfo : AbstractInfo<IntegrationConnectorInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "integration.connector";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IntegrationConnectorInfoProvider), OBJECT_TYPE, "Integration.Connector", "ConnectorID", "ConnectorLastModified", null, "ConnectorName", "ConnectorDisplayName", null, null, null, null)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            LogIntegration = false,
            AssemblyNameColumn = "ConnectorAssemblyName",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
            },
            EnabledColumn = "ConnectorEnabled",
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Assembly which is the connector bound to.
        /// </summary>
        public virtual string ConnectorAssemblyName
        {
            get
            {
                return GetStringValue("ConnectorAssemblyName", string.Empty);
            }
            set
            {
                SetValue("ConnectorAssemblyName", value);
            }
        }


        /// <summary>
        /// Determines whether the connector is enabled.
        /// </summary>
        public virtual bool ConnectorEnabled
        {
            get
            {
                return GetBooleanValue("ConnectorEnabled", false);
            }
            set
            {
                SetValue("ConnectorEnabled", value);
            }
        }


        /// <summary>
        /// Connector's code name.
        /// </summary>
        public virtual string ConnectorName
        {
            get
            {
                return GetStringValue("ConnectorName", string.Empty);
            }
            set
            {
                SetValue("ConnectorName", value);
            }
        }


        /// <summary>
        /// Name of a class which is the connector bound to.
        /// </summary>
        public virtual string ConnectorClassName
        {
            get
            {
                return GetStringValue("ConnectorClassName", string.Empty);
            }
            set
            {
                SetValue("ConnectorClassName", value);
            }
        }


        /// <summary>
        /// Connector identifier.
        /// </summary>
        public virtual int ConnectorID
        {
            get
            {
                return GetIntegerValue("ConnectorID", 0);
            }
            set
            {
                SetValue("ConnectorID", value);
            }
        }


        /// <summary>
        /// Connector's display name.
        /// </summary>
        public virtual string ConnectorDisplayName
        {
            get
            {
                return GetStringValue("ConnectorDisplayName", string.Empty);
            }
            set
            {
                SetValue("ConnectorDisplayName", value);
            }
        }


        /// <summary>
        /// Connector last modified time stamp.
        /// </summary>
        public virtual DateTime ConnectorLastModified
        {
            get
            {
                return GetDateTimeValue("ConnectorLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ConnectorLastModified", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IntegrationConnectorInfoProvider.DeleteIntegrationConnectorInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IntegrationConnectorInfoProvider.SetIntegrationConnectorInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty IntegrationConnectorInfo object.
        /// </summary>
        public IntegrationConnectorInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IntegrationConnectorInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public IntegrationConnectorInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}