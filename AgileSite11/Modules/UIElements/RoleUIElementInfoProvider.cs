using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Modules
{
    using TypedDataSet = InfoDataSet<RoleUIElementInfo>;

    /// <summary>
    /// Class providing RoleUIElementInfo management.
    /// </summary>
    public class RoleUIElementInfoProvider : AbstractInfoProvider<RoleUIElementInfo, RoleUIElementInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all role UI elements.
        /// </summary>
        public static ObjectQuery<RoleUIElementInfo> GetRoleUIElements()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the RoleUIElementInfo structure for the specified role UI element.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        [Obsolete("Use method GetRoleUIElements() instead")]
        public static TypedDataSet GetRoleUIElements(string where, string orderBy)
        {
            return GetRoleUIElements().Where(where).OrderBy(orderBy).TypedResult;
        }


        /// <summary>
        /// Returns the RoleUIElementInfo structure for the specified role UI element.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="elementId">ElementID</param>
        public static RoleUIElementInfo GetRoleUIElementInfo(int roleId, int elementId)
        {
            return ProviderObject.GetRoleUIElementInfoInternal(roleId, elementId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified role UI element.
        /// </summary>
        /// <param name="infoObj">RoleUIElement to set</param>
        public static void SetRoleUIElementInfo(RoleUIElementInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified role UI element.
        /// </summary>
        /// <param name="infoObj">RoleUIElement object</param>
        public static void DeleteRoleUIElementInfo(RoleUIElementInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified role UI Element.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="elementId">ElementID</param>
        public static void DeleteRoleUIElementInfo(int roleId, int elementId)
        {
            RoleUIElementInfo infoObj = GetRoleUIElementInfo(roleId, elementId);
            DeleteRoleUIElementInfo(infoObj);
        }


        /// <summary>
        /// Add specified role UI Element.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="elementId">ElementID</param>
        public static void AddRoleUIElementInfo(int roleId, int elementId)
        {
            RoleUIElementInfo infoObj = new RoleUIElementInfo();
            infoObj.RoleID = roleId;
            infoObj.ElementID = elementId;
            SetRoleUIElementInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the RoleUIElementInfo structure for the specified role UI element.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="elementId">ElementID</param>
        protected virtual RoleUIElementInfo GetRoleUIElementInfoInternal(int roleId, int elementId)
        {
            return
                GetObjectQuery().TopN(1)
                    .WhereEquals("RoleID", roleId)
                    .WhereEquals("ElementID", elementId).FirstOrDefault();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(RoleUIElementInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }

        #endregion
    }
}