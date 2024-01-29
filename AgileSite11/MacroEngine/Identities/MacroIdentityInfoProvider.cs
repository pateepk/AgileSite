using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class providing MacroIdentityInfo management.
    /// </summary>
    public partial class MacroIdentityInfoProvider : AbstractInfoProvider<MacroIdentityInfo, MacroIdentityInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// <para>
        /// Name of macro identity which has global administrator effective user and is present by default.
        /// </para>
        /// <para>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </para>
        /// </summary>
        public const string DEFAULT_GLOBAL_ADMINISTRATOR_IDENTITY_NAME = "GlobalAdministrator";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public MacroIdentityInfoProvider()
            : base(MacroIdentityInfo.TYPEINFO, new HashtableSettings { ID = true, Name = true, Load = LoadHashtableEnum.None })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the MacroIdentityInfo objects.
        /// </summary>
        public static ObjectQuery<MacroIdentityInfo> GetMacroIdentities()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns MacroIdentityInfo with specified ID.
        /// </summary>
        /// <param name="id">MacroIdentityInfo ID</param>
        public static MacroIdentityInfo GetMacroIdentityInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns MacroIdentityInfo with specified name.
        /// </summary>
        /// <param name="name">MacroIdentityInfo name</param>
        public static MacroIdentityInfo GetMacroIdentityInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Sets (updates or inserts) specified MacroIdentityInfo.
        /// </summary>
        /// <param name="infoObj">MacroIdentityInfo to be set</param>
        public static void SetMacroIdentityInfo(MacroIdentityInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified MacroIdentityInfo.
        /// </summary>
        /// <param name="infoObj">MacroIdentityInfo to be deleted</param>
        public static void DeleteMacroIdentityInfo(MacroIdentityInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes MacroIdentityInfo with specified ID.
        /// </summary>
        /// <param name="id">MacroIdentityInfo ID</param>
        public static void DeleteMacroIdentityInfo(int id)
        {
            MacroIdentityInfo infoObj = GetMacroIdentityInfo(id);
            DeleteMacroIdentityInfo(infoObj);
        }

        #endregion
    }
}