using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing MembershipRoleInfo management.
    /// </summary>
    public class MembershipRoleInfoProvider : AbstractInfoProvider<MembershipRoleInfo, MembershipRoleInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the query for all relationships between memberships and roles.
        /// </summary>   
        public static ObjectQuery<MembershipRoleInfo> GetMembershipRoles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified membership and role.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="roleId">Role ID</param>
        public static MembershipRoleInfo GetMembershipRoleInfo(int membershipId, int roleId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetMembershipRoleInfoInternal(membershipId, roleId);
        }


        /// <summary>
        /// Sets relationship between specified membership and role.
        /// </summary>
        /// <param name="infoObj">Membership-role relationship to be set</param>
        public static void SetMembershipRoleInfo(MembershipRoleInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified membership and role.
        /// </summary>
        /// <param name="infoObj">Membership-role relationship to be deleted</param>
        public static void DeleteMembershipRoleInfo(MembershipRoleInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship between specified membership and role.
        /// </summary>	
        /// <param name="membershipId">Membership ID</param>
        /// <param name="roleId">Role ID</param>
        public static void AddMembershipToRole(int membershipId, int roleId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            MembershipRoleInfo infoObj = ProviderObject.CreateInfo();

            infoObj.MembershipID = membershipId;
            infoObj.RoleID = roleId;

            SetMembershipRoleInfo(infoObj);

            // Invalidate all user objects
            UserInfo.TYPEINFO.InvalidateAllObjects();
        }


        /// <summary>
        /// Deletes relationship between specified membership and role.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="roleId">Role ID</param>
        public static void RemoveMembershipFromRole(int membershipId, int roleId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            MembershipRoleInfo infoObj = GetMembershipRoleInfo(membershipId, roleId);
            DeleteMembershipRoleInfo(infoObj);

            // Invalidate all user objects
            UserInfo.TYPEINFO.InvalidateAllObjects();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns relationship between specified membership and role.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="roleId">Role ID</param>
        protected virtual MembershipRoleInfo GetMembershipRoleInfoInternal(int membershipId, int roleId)
        {
            var condition = new WhereCondition()
                .WhereEquals("MembershipID", membershipId)
                .WhereEquals("RoleID", roleId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }

        #endregion
    }
}