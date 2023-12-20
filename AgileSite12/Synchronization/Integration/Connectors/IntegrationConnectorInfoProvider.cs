using System;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing IntegrationConnectorInfo management.
    /// </summary>
    public class IntegrationConnectorInfoProvider : AbstractInfoProvider<IntegrationConnectorInfo, IntegrationConnectorInfoProvider>
    {
        private const string CLEAR_ACTION_NAME = "clearintegrationconnectorinfos";


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public IntegrationConnectorInfoProvider()
            : base(IntegrationConnectorInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all integration connectors.
        /// </summary>
        public static ObjectQuery<IntegrationConnectorInfo> GetIntegrationConnectors()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns integration connector with specified ID.
        /// </summary>
        /// <param name="connectorId">Integration connector ID.</param>
        public static IntegrationConnectorInfo GetIntegrationConnectorInfo(int connectorId)
        {
            return ProviderObject.GetInfoById(connectorId);
        }


        /// <summary>
        /// Returns integration connector with specified name.
        /// </summary>
        /// <param name="connectorName">Integration connector name.</param>
        public static IntegrationConnectorInfo GetIntegrationConnectorInfo(string connectorName)
        {
            return ProviderObject.GetInfoByCodeName(connectorName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified integration connector.
        /// </summary>
        /// <param name="connectorObj">Integration connector to be set.</param>
        public static void SetIntegrationConnectorInfo(IntegrationConnectorInfo connectorObj)
        {
            ProviderObject.SetInfo(connectorObj);
        }


        /// <summary>
        /// Deletes specified integration connector.
        /// </summary>
        /// <param name="connectorObj">Integration connector to be deleted.</param>
        public static void DeleteIntegrationConnectorInfo(IntegrationConnectorInfo connectorObj)
        {
            ProviderObject.DeleteInfo(connectorObj);
        }


        /// <summary>
        /// Deletes integration connector with specified ID.
        /// </summary>
        /// <param name="connectorId">Integration connector ID.</param>
        public static void DeleteIntegrationConnectorInfo(int connectorId)
        {
            IntegrationConnectorInfo connectorObj = GetIntegrationConnectorInfo(connectorId);
            DeleteIntegrationConnectorInfo(connectorObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            if (logTasks)
            {
                CreateWebFarmTask(CLEAR_ACTION_NAME, null);
            }

            // Clear collections with connectors and subscriptions
            IntegrationHelper.InvalidateConnectors();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(IntegrationConnectorInfo info)
        {
            base.SetInfo(info);
            if (info != null)
            {
                ClearHashtables(info.Generalized.LogWebFarmTasks);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(IntegrationConnectorInfo info)
        {
            base.DeleteInfo(info);
            if (info != null)
            {
                ClearHashtables(info.Generalized.LogWebFarmTasks);
            }
        }

        #endregion


        #region "Web farm"

        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            // Switch by action name
            switch (actionName)
            {
                // Clear integration connector infos
                case CLEAR_ACTION_NAME:
                    ClearHashtables(false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}