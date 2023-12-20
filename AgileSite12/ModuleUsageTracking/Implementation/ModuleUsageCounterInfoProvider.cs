using CMS.DataEngine;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Class providing ModuleUsageCounterInfo management.
    /// </summary>
    internal class ModuleUsageCounterInfoProvider : AbstractInfoProvider<ModuleUsageCounterInfo, ModuleUsageCounterInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ModuleUsageCounterInfoProvider()
            : base(ModuleUsageCounterInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ModuleUsageCounterInfo objects.
        /// </summary>
        public static ObjectQuery<ModuleUsageCounterInfo> GetModuleUsageCounters()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ModuleUsageCounterInfo with specified ID.
        /// </summary>
        /// <param name="id">ModuleUsageCounterInfo ID</param>
        public static ModuleUsageCounterInfo GetModuleUsageCounterInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns ModuleUsageCounterInfo with specified name.
        /// </summary>
        /// <param name="name">ModuleUsageCounterInfo name</param>
        public static ModuleUsageCounterInfo GetModuleUsageCounterInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ModuleUsageCounterInfo.
        /// </summary>
        /// <param name="infoObj">ModuleUsageCounterInfo to be set</param>
        public static void SetModuleUsageCounterInfo(ModuleUsageCounterInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ModuleUsageCounterInfo.
        /// </summary>
        /// <param name="infoObj">ModuleUsageCounterInfo to be deleted</param>
        public static void DeleteModuleUsageCounterInfo(ModuleUsageCounterInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ModuleUsageCounterInfo with specified ID.
        /// </summary>
        /// <param name="id">ModuleUsageCounterInfo ID</param>
        public static void DeleteModuleUsageCounterInfo(int id)
        {
            ModuleUsageCounterInfo infoObj = GetModuleUsageCounterInfo(id);
            DeleteModuleUsageCounterInfo(infoObj);
        }


        /// <summary>
        /// Increments ModuleUsageCounterInfo with specified name.
        /// </summary>
        /// <param name="name">ModuleUsageCounterInfo name</param>
        public static void IncrementModuleUsageCounterInfo(string name)
        {
            ProviderObject.IncrementModuleUsageCounterInfoInternal(name);
        }


        /// <summary>
        /// Clears ModuleUsageCounterInfo with specified name.
        /// </summary>
        /// <param name="name">ModuleUsageCounterInfo name</param>
        public static void ClearModuleUsageCounterInfo(string name)
        {
            ProviderObject.ClearModuleUsageCounterInfoInternal(name);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Increments ModuleUsageCounterInfo with specified name.
        /// </summary>
        /// <param name="name">ModuleUsageCounterInfo name</param>
        protected virtual void IncrementModuleUsageCounterInfoInternal(string name)
        {
            ExecuteQueryWithNameParameter("increment", name);
        }


        /// <summary>
        /// Clears ModuleUsageCounterInfo with specified name.
        /// </summary>
        /// <param name="name">ModuleUsageCounterInfo name</param>
        protected virtual void ClearModuleUsageCounterInfoInternal(string name)
        {
            ExecuteQueryWithNameParameter("clear", name);
        }


        private void ExecuteQueryWithNameParameter(string queryName, string nameParameter)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@Name", nameParameter);
            ConnectionHelper.ExecuteNonQuery("cms.moduleusagecounter." + queryName, parameters);
        }

        #endregion
    }
}