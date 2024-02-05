using CMS.DataEngine;
using System.Linq;

namespace CMS.Modules
{
    /// <summary>
    /// Class providing RolePermissionInfo management.
    /// </summary>
    public class RolePermissionInfoProvider : AbstractInfoProvider<RolePermissionInfo, RolePermissionInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all role permissions.
        /// </summary>
        public static ObjectQuery<RolePermissionInfo> GetRolePermissions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the RolePermissionInfo structure for the specified rolePermission.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="permissionId">PermissionID</param>
        public static RolePermissionInfo GetRolePermissionInfo(int roleId, int permissionId)
        {
            return ProviderObject.GetRolePermissionInfoInternal(roleId, permissionId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified rolePermission.
        /// </summary>
        /// <param name="rolePermission">RolePermission to set</param>
        public static void SetRolePermissionInfo(RolePermissionInfo rolePermission)
        {
            ProviderObject.SetInfo(rolePermission);
        }


        /// <summary>
        /// Sets (updates or inserts) specified rolePermission.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="permissionId">PermissionID</param>
        public static void SetRolePermissionInfo(int roleId, int permissionId)
        {
            RolePermissionInfo infoObj = ProviderObject.CreateInfo();

            infoObj.RoleID = roleId;
            infoObj.PermissionID = permissionId;

            SetRolePermissionInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified rolePermission.
        /// </summary>
        /// <param name="infoObj">RolePermission object</param>
        public static void DeleteRolePermissionInfo(RolePermissionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified rolePermission.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="permissionId">PermissionID</param>
        public static void DeleteRolePermissionInfo(int roleId, int permissionId)
        {
            RolePermissionInfo infoObj = GetRolePermissionInfo(roleId, permissionId);
            DeleteRolePermissionInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the RolePermissionInfo structure for the specified rolePermission.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="permissionId">PermissionID</param>
        protected virtual RolePermissionInfo GetRolePermissionInfoInternal(int roleId, int permissionId)
        {
            return
                GetObjectQuery().TopN(1)
                    .WhereEquals("RoleID", roleId)
                    .WhereEquals("PermissionID", permissionId).FirstOrDefault();
        }

        #endregion
    }
}