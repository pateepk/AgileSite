using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Modules
{    
    /// <summary>
    /// Class providing RoleApplicationInfo management.
    /// </summary>
    public class RoleApplicationInfoProvider : AbstractInfoProvider<RoleApplicationInfo, RoleApplicationInfoProvider>
    {
        #region "Public methods"

		/// <summary>
        /// Returns all RoleApplicationInfo bindings.
        /// </summary>
        public static ObjectQuery<RoleApplicationInfo> GetRoleApplications()
        {
            return ProviderObject.GetObjectQuery();
        }


		/// <summary>
        /// Returns RoleApplicationInfo binding structure.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="uielementId">UI element ID</param>  
        public static RoleApplicationInfo GetRoleApplicationInfo(int roleId, int uielementId)
        {
            return ProviderObject.GetRoleApplicationInfoInternal(roleId, uielementId);
        }


        /// <summary>
        /// Sets specified RoleApplicationInfo.
        /// </summary>
        /// <param name="infoObj">RoleApplicationInfo to set</param>
        public static void SetRoleApplicationInfo(RoleApplicationInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified RoleApplicationInfo binding.
        /// </summary>
        /// <param name="infoObj">RoleApplicationInfo object</param>
        public static void DeleteRoleApplicationInfo(RoleApplicationInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes RoleApplicationInfo binding.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="uielementId">UI element ID</param>  
        public static void RemoveRoleFromUIelement(int roleId, int uielementId)
        {
            ProviderObject.RemoveRoleFromUIelementInternal(roleId, uielementId);
        }


        /// <summary>
        /// Creates RoleApplicationInfo binding. 
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="uielementId">UI element ID</param>   
        public static void AddRoleToUIelement(int roleId, int uielementId)
        {
            ProviderObject.AddRoleToUIelementInternal(roleId, uielementId);
        }

        #endregion


        #region "Internal methods"
	
        /// <summary>
        /// Returns the RoleApplicationInfo structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="uielementId">UI element ID</param>  
        protected virtual RoleApplicationInfo GetRoleApplicationInfoInternal(int roleId, int uielementId)
        {
            var bindings = GetRoleApplications()
                .Where("RoleID", QueryOperator.Equals, roleId)
                .Where("ElementID", QueryOperator.Equals, uielementId)
                .TopN(1);

            if (bindings.Count > 0)
            {
                return bindings.First();
            }

            return null;
        }


		/// <summary>
        /// Deletes RoleApplicationInfo binding.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="uielementId">UI element ID</param>  
        protected virtual void RemoveRoleFromUIelementInternal(int roleId, int uielementId)
        {
            var infoObj = GetRoleApplicationInfo(roleId, uielementId);
			if (infoObj != null) 
			{
				DeleteRoleApplicationInfo(infoObj);
			}
        }


        /// <summary>
        /// Creates RoleApplicationInfo binding. 
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="uielementId">UI element ID</param>   
        protected virtual void AddRoleToUIelementInternal(int roleId, int uielementId)
        {
            // Create new binding
            var infoObj = new RoleApplicationInfo();
            infoObj.RoleID = roleId;
			infoObj.ElementID = uielementId;

            // Save to the database
            SetRoleApplicationInfo(infoObj);
        }
       
        #endregion		
    }
}