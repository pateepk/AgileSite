using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    using BindingDataSet = InfoDataSet<DeviceProfileLayoutInfo>;
    using TargetLayoutIdentifierDictionary = SafeDictionary<String, Int32>;

    /// <summary>
    /// Provides DeviceProfileLayoutInfo object management.
    /// </summary>
    public class DeviceProfileLayoutInfoProvider : AbstractInfoProvider<DeviceProfileLayoutInfo, DeviceProfileLayoutInfoProvider>
    {

        #region "Constants"

        /// <summary>
        /// A name of the ClearTargetLayoutIdentifierTable web farm task.
        /// </summary>
        private const string WebFarmTaskName_ClearTargetLayoutIdentifierHashtable = "ClearTargetLayoutIdentifierHashtable";

        #endregion


        #region "Members"

        /// <summary>
        /// A dictionary for caching of layout mappings.
        /// </summary>
        /// <remarks>
        /// The dictionary stores target layout identifiers using a composite key that consists of device and source layout identifiers.
        /// </remarks>
        private static CMSStatic<TargetLayoutIdentifierDictionary> mTargetLayoutIdentifiers = new CMSStatic<TargetLayoutIdentifierDictionary>();


        /// <summary>
        /// An internal object used for locking.
        /// </summary>
        private static object mLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets a dictionary for caching of layout mappings.
        /// </summary>
        private static TargetLayoutIdentifierDictionary TargetLayoutIdentifiers
        {
            get
            {
                if (mTargetLayoutIdentifiers.Value == null)
                {
                    mTargetLayoutIdentifiers.Value = new TargetLayoutIdentifierDictionary();
                }

                return mTargetLayoutIdentifiers;
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all bindings between two layouts matching the specified parameters.
        /// </summary>
        public static ObjectQuery<DeviceProfileLayoutInfo> GetDeviceProfileLayouts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a binding between two layouts with the specified identifier.
        /// </summary>
        /// <param name="bindingId">Binding identifier.</param>        
        public static DeviceProfileLayoutInfo GetDeviceProfileLayoutInfo(int bindingId)
        {
            return ProviderObject.GetInfoById(bindingId);
        }


        /// <summary>
        /// Returns a binding between two layouts with the specified globally unique identifier.
        /// </summary>
        /// <param name="bindingGuid">Binding globally unique identifier.</param>
        public static DeviceProfileLayoutInfo GetDeviceProfileLayoutInfo(Guid bindingGuid)
        {
            return ProviderObject.GetInfoByGuid(bindingGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) the specified binding between two layouts.
        /// </summary>
        /// <param name="binding">The binding to set.</param>
        public static void SetDeviceProfileLayoutInfo(DeviceProfileLayoutInfo binding)
        {
            string key = GetTargetLayoutCompositeKey(binding);
            TargetLayoutIdentifiers[key] = binding.TargetLayoutID;
            ProviderObject.SetInfo(binding);
            CreateClearTargetLayoutIdentifierHashtableWebFarmTask();
        }


        /// <summary>
        /// Deletes the specified binding between two layouts.
        /// </summary>
        /// <param name="binding">The binding to delete.</param>
        public static void DeleteDeviceProfileLayoutInfo(DeviceProfileLayoutInfo binding)
        {
            string key = GetTargetLayoutCompositeKey(binding);
            TargetLayoutIdentifiers[key] = 0;
            ProviderObject.DeleteInfo(binding);
            CreateClearTargetLayoutIdentifierHashtableWebFarmTask();
        }


        /// <summary>
        /// Deletes the specified binding between two layouts.
        /// </summary>
        /// <param name="bindingId">An identifier of the binding to delete.</param>
        public static void DeleteDeviceProfileLayoutInfo(int bindingId)
        {
            DeviceProfileLayoutInfo deviceProfileLayout = GetDeviceProfileLayoutInfo(bindingId);
            DeleteDeviceProfileLayoutInfo(deviceProfileLayout);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a target layout for the specified device profile and source layout.
        /// </summary>
        /// <param name="deviceProfileId">Device profile identifier.</param>
        /// <param name="sourceLayoutId">Source layout identifier.</param>
        public static LayoutInfo GetTargetLayoutInfo(int deviceProfileId, int sourceLayoutId)
        {
            int targetLayoutID = GetTargetLayoutId(deviceProfileId, sourceLayoutId);

            return LayoutInfoProvider.GetLayoutInfo(targetLayoutID);
        }


        /// <summary>
        /// Returns a target layout for the specified device profile and source layout.
        /// </summary>
        /// <param name="deviceProfile">Device profile.</param>
        /// <param name="sourceLayout">Source layout.</param>
        public static LayoutInfo GetTargetLayoutInfo(DeviceProfileInfo deviceProfile, LayoutInfo sourceLayout)
        {
            if ((deviceProfile == null) || (sourceLayout == null))
            {
                return null;
            }

            return GetTargetLayoutInfo(deviceProfile.ProfileID, sourceLayout.LayoutId);
        }


        /// <summary>
        /// Returns a target layout identifier for the specified device profile and source layout.
        /// </summary>
        /// <param name="deviceProfileId">Device profile identifier.</param>
        /// <param name="sourceLayoutId">Source layout identifier.</param>
        public static int GetTargetLayoutId(int deviceProfileId, int sourceLayoutId)
        {
            int targetLayoutId;
            string key = GetTargetLayoutCompositeKey(deviceProfileId, sourceLayoutId);
            if (TargetLayoutIdentifiers.TryGetValue(key, out targetLayoutId))
            {
                return targetLayoutId;
            }
            targetLayoutId = ProviderObject.GetTargetLayoutIdInternal(deviceProfileId, sourceLayoutId);
            TargetLayoutIdentifiers[key] = targetLayoutId;

            return targetLayoutId;
        }


        /// <summary>
        /// Returns a target layout identifier for the specified device profile and source layout.
        /// </summary>
        /// <param name="deviceProfile">Device profile.</param>
        /// <param name="sourceLayout">Source layout.</param>
        public static int GetTargetLayoutId(DeviceProfileInfo deviceProfile, LayoutInfo sourceLayout)
        {
            if ((deviceProfile == null) || (sourceLayout == null))
            {
                return 0;
            }

            return GetTargetLayoutId(deviceProfile.ProfileID, sourceLayout.LayoutId);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a target layout identifier for the specified device profile and source layout.
        /// </summary>
        /// <param name="deviceProfileId">Device profile identifier.</param>
        /// <param name="sourceLayoutId">Source layout identifier.</param>
        protected virtual int GetTargetLayoutIdInternal(int deviceProfileId, int sourceLayoutId)
        {
            string where = String.Format("(DeviceProfileID = {0:D}) AND (SourceLayoutID = {1:D})", deviceProfileId, sourceLayoutId);
            BindingDataSet data = GetDeviceProfileLayouts().Where(where).TopN(1).Column("TargetLayoutID").TypedResult;
            int targetLayoutId = 0;

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                targetLayoutId = ValidationHelper.GetInteger(data.Tables[0].Rows[0]["TargetLayoutID"], 0);
            }

            return targetLayoutId;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates a target layout composite key of the specified layout binding, and returns it.
        /// </summary>
        /// <param name="binding">A layout binding.</param>
        /// <returns>A composite key of the specified layout binding.</returns>
        private static string GetTargetLayoutCompositeKey(DeviceProfileLayoutInfo binding)
        {
            return GetTargetLayoutCompositeKey(binding.DeviceProfileID, binding.SourceLayoutID);
        }


        /// <summary>
        /// Creates a target layout composite key of the specified layout binding, and returns it.
        /// </summary>
        /// <param name="deviceProfileId">A device profile identifier.</param>
        /// <param name="sourceLayoutId">A source layout identifier.</param>
        /// <returns>A composite key of the specified layout binding.</returns>
        private static string GetTargetLayoutCompositeKey(int deviceProfileId, int sourceLayoutId)
        {
            return String.Format("{0:D}|{1:D}", deviceProfileId, sourceLayoutId);
        }

        #endregion


        #region "Cache methods"

        /// <summary>
        /// Runs this provider's web farm tasks.
        /// </summary>
        /// <param name="actionName">An action name.</param>
        /// <param name="data">Custom task data.</param>
        /// <param name="binary">Binary data.</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            switch (actionName)
            {
                case WebFarmTaskName_ClearTargetLayoutIdentifierHashtable:
                    ClearTargetLayoutIdentifierHashtable(false);
                    break;

                default:
                    string message = String.Format("[{0}.ProcessWebFarmTask] The action '{1}' has no supporting code.", TypeInfo.ObjectType, actionName);
                    throw new Exception(message);
            }
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            lock (mLock)
            {
                ClearTargetLayoutIdentifierHashtable(logTasks);
            }
        }


        /// <summary>
        /// Clears provider's cache of target layouts based on device profile and source layout.
        /// </summary>
        /// <param name="logTasks">Indicates whether web farm tasks will be created.</param>
        public static void ClearTargetLayoutIdentifierHashtable(bool logTasks)
        {
            TargetLayoutIdentifiers.Clear();
            if (logTasks)
            {
                CreateClearTargetLayoutIdentifierHashtableWebFarmTask();
            }
        }


        /// <summary>
        /// Creates a web farm task to clear a cache of target layouts based on device profile and source layout.
        /// </summary>
        private static void CreateClearTargetLayoutIdentifierHashtableWebFarmTask()
        {
            ProviderObject.CreateWebFarmTask(WebFarmTaskName_ClearTargetLayoutIdentifierHashtable, null);
        }

        #endregion

    }

}