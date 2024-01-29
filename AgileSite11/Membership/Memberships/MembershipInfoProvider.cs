using System;
using System.Data;

using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing Membership management.
    /// </summary>
    public class MembershipInfoProvider : AbstractInfoProvider<MembershipInfo, MembershipInfoProvider>
    {
        /// <summary>
        /// Creates new instance of <see cref="MembershipInfoProvider"/>.
        /// </summary>
        public MembershipInfoProvider()
            : base(MembershipInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    Name = true, 
                    GUID = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }


        /// <summary>
        /// Returns the query for all memberships.
        /// </summary>   
        public static ObjectQuery<MembershipInfo> GetMemberships()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all valid memberships for given user
        /// </summary>
        /// <param name="userID">User ID</param>
        public static DataSet GetUserMemberships(int userID)
        {
            return ProviderObject.GetUserMembershipsInternal(userID);
        }


        /// <summary>
        /// Returns membership with specified ID.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>        
        public static MembershipInfo GetMembershipInfo(int membershipId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetInfoById(membershipId);
        }


        /// <summary>
        /// Returns membership with specified GUID.
        /// </summary>
        /// <param name="membershipGuid">Membership GUID</param>
        /// <param name="siteId">Site ID</param>
        public static MembershipInfo GetMembershipInfo(Guid membershipGuid, int siteId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetInfoByGuid(membershipGuid, siteId);
        }


        /// <summary>
        /// Returns membership with specified name.
        /// </summary>
        /// <param name="membershipName">Membership name</param>                
        /// <param name="siteName">Site name</param>                
        public static MembershipInfo GetMembershipInfo(string membershipName, string siteName)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetInfoByCodeName(membershipName, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified membership.
        /// </summary>
        /// <param name="membershipObj">Membership to be set</param>
        public static void SetMembershipInfo(MembershipInfo membershipObj)
        {
            ProviderObject.SetInfo(membershipObj);
        }


        /// <summary>
        /// Deletes specified membership.
        /// </summary>
        /// <param name="membershipObj">Membership to be deleted</param>
        public static void DeleteMembershipInfo(MembershipInfo membershipObj)
        {
            ProviderObject.DeleteInfo(membershipObj);
        }


        /// <summary>
        /// Deletes membership with specified ID.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        public static void DeleteMembershipInfo(int membershipId)
        {
            MembershipInfo membershipObj = GetMembershipInfo(membershipId);
            DeleteMembershipInfo(membershipObj);
        }


        /// <summary>
        /// Returns collection with valid memberships associated to user
        /// </summary>
        /// <param name="userID">User ID</param>
        protected virtual DataSet GetUserMembershipsInternal(int userID)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Now", Service.Resolve<IDateTimeNowService>().GetDateTimeNow());
            return ConnectionHelper.ExecuteQuery("cms.membership.getusermemberships", parameters, "UserID = " + userID + " AND (ValidTo IS NULL OR ValidTo > @Now)", "MembershipName");
        }
    }
}