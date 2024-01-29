using System.Linq;

using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Class providing BizFormRoleInfo management.
    /// </summary>
    public class BizFormRoleInfoProvider : AbstractInfoProvider<BizFormRoleInfo, BizFormRoleInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the BizFormRoleInfo objects.
        /// </summary>
        public static ObjectQuery<BizFormRoleInfo> GetBizFormRoles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the BizFormRoleInfo structure for the specified bizFormRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="formId">FormID</param>
        public static BizFormRoleInfo GetBizFormRoleInfo(int roleId, int formId)
        {
            return ProviderObject.GetBizFormRoleInfoInternal(roleId, formId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified BizFormRoleInfo.
        /// </summary>
        /// <param name="infoObj">BizFormRoleInfo to be set.</param>
        public static void SetBizFormRoleInfo(BizFormRoleInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Inserts specified BizFormRoleInfo.
        /// </summary>
        /// <param name="formId">FormId</param>
        /// <param name="roleId">RoleId</param>
        public static void SetBizFormRoleInfo(int roleId, int formId)
        {
            ProviderObject.SetBizFormRoleInfoInternal(roleId, formId);
        }


        /// <summary>
        /// Deletes specified BizFormRoleInfo.
        /// </summary>
        /// <param name="infoObj">BizFormRoleInfo to be deleted.</param>
        public static void DeleteBizFormRoleInfo(BizFormRoleInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified BizFormRole.
        /// </summary>
        /// <param name="formId">FormId</param>
        /// <param name="roleId">RoleId</param>
        public static void DeleteBizFormRoleInfo(int roleId, int formId)
        {
            BizFormRoleInfo infoObj = GetBizFormRoleInfo(roleId, formId);
            DeleteBizFormRoleInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Removes all allowed roles from the form.
        /// </summary>        
        /// <param name="formId">Form ID</param>
        public static void RemoveAllRolesFromForm(int formId)
        {
            ProviderObject.RemoveAllRolesFromFormInternal(formId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns the BizFormRoleInfo structure for the specified form and role.
        /// </summary>
        /// <param name="roleId">RoleId</param>
        /// <param name="formId">FormId</param>       
        protected virtual BizFormRoleInfo GetBizFormRoleInfoInternal(int roleId, int formId)
        {
            return GetObjectQuery().Where("FormID", QueryOperator.Equals, formId).Where("RoleID", QueryOperator.Equals, roleId).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Inserts specified BizFormRoleInfo.
        /// </summary>
        /// <param name="roleId">RoleId</param>
        /// <param name="formId">FormId</param>
        protected virtual void SetBizFormRoleInfoInternal(int roleId, int formId)
        {
            var infoObj = ProviderObject.CreateInfo();
            infoObj.RoleID = roleId;
            infoObj.FormID = formId;

            SetInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Removes all allowed roles from the form.
        /// </summary>        
        /// <param name="formId">Form ID</param>
        protected virtual void RemoveAllRolesFromFormInternal(int formId)
        {
            BulkDelete(new WhereCondition().WhereEquals("FormID", formId));
        }

        #endregion
    }
}