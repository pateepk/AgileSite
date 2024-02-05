using System;

using CMS.DataEngine;

namespace CMS.ModuleLicenses
{    
    /// <summary>
    /// Class providing ModuleLicenseKeyInfo management.
    /// </summary>
    public partial class ModuleLicenseKeyInfoProvider : AbstractInfoProvider<ModuleLicenseKeyInfo, ModuleLicenseKeyInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ModuleLicenseKeyInfoProvider()
            : base(ModuleLicenseKeyInfo.TYPEINFO, new HashtableSettings { GUID = false, Load = LoadHashtableEnum.None })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ModuleLicenseKeyInfo objects.
        /// </summary>
        public static ObjectQuery<ModuleLicenseKeyInfo> GetModuleLicenseKeys()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ModuleLicenseKeyInfo with specified ID.
        /// </summary>
        /// <param name="id">ModuleLicenseKeyInfo ID</param>
        public static ModuleLicenseKeyInfo GetModuleLicenseKeyInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns ModuleLicenseKeyInfo with specified GUID.
        /// </summary>
        /// <param name="guid">ModuleLicenseKeyInfo GUID</param>                
        public static ModuleLicenseKeyInfo GetModuleLicenseKeyInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns a query for all module license keys of ModuleLicenseKeyInfo objects of given resource.
        /// </summary>
        /// <param name="resourceId">ID of a resource</param>
        public static ObjectQuery<ModuleLicenseKeyInfo> GetResourceModuleLicenseKeyInfos(int resourceId)
        {
            return ProviderObject.GetObjectQuery().WhereEquals("ModuleLicenseKeyResourceID", resourceId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ModuleLicenseKeyInfo.
        /// </summary>
        /// <param name="infoObj">ModuleLicenseKeyInfo to be set</param>
        public static void SetModuleLicenseKeyInfo(ModuleLicenseKeyInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ModuleLicenseKeyInfo.
        /// </summary>
        /// <param name="infoObj">ModuleLicenseKeyInfo to be deleted</param>
        public static void DeleteModuleLicenseKeyInfo(ModuleLicenseKeyInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ModuleLicenseKeyInfo with specified ID.
        /// </summary>
        /// <param name="id">ModuleLicenseKeyInfo ID</param>
        public static void DeleteModuleLicenseKeyInfo(int id)
        {
            ModuleLicenseKeyInfo infoObj = GetModuleLicenseKeyInfo(id);
            DeleteModuleLicenseKeyInfo(infoObj);
        }


        /// <summary>
        /// Clears hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void Clear(bool logTasks)
        {
            ProviderObject.ClearHashtables(logTasks);
        }

        #endregion


        #region "Internal methods - Basic"
	
        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ModuleLicenseKeyInfo info)
        {
            ModuleLicensesHelper.ClearCache();
            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ModuleLicenseKeyInfo info)
        {
            ModuleLicensesHelper.ClearCache();
            base.DeleteInfo(info);
        }

        #endregion

        /// <summary>
        /// Clears the object's hashtables and special cache for module licenses.
        /// </summary>
        /// <param name="logTasks">whether to log webfarm tasks</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            ModuleLicensesHelper.ClearCache();
        }
    }
}