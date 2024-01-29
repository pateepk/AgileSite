using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Holds collection used and created for security check within membership libraries
    /// </summary>
    [Serializable]
    internal sealed class UserSecurityCollections
    {
        public SafeDictionary<string, SafeDictionary<string, int?>> SitesRoles { get; private set; }

        public SafeDictionary<string, SafeDictionary<string, int?>> MembershipRoles { get; private set; }

        public SafeDictionary<int, DateTime?> RolesValidity { get; private set; }

        public SafeDictionary<string, SafeDictionary<string, int?>> Memberships { get; private set; }

        public SafeDictionary<int, DateTime?> MembershipsValidity { get; private set; }

        public SafeDictionary<int, DateTime?> MembershipRoleValidity { get; private set; }


        private UserSecurityCollections()
        {
            SitesRoles = CreateRoleCollection();
            MembershipRoles = CreateRoleCollection();
            Memberships = CreateRoleCollection();
            RolesValidity = CreateValidityCollection();
            MembershipsValidity = CreateValidityCollection();
            MembershipRoleValidity = CreateValidityCollection();
        }


        /// <summary>
        /// Returns <see cref="UserSecurityCollections"/> object with loaded data for specified user.
        /// </summary>
        public static UserSecurityCollections GetSecurityCollectionsForUser(UserInfo user)
        {
            UserSecurityCollections securityCollections;

            int uid = user.UserID;
            if (uid > 0)
            {
                // Load the Sites & roles table
                var userSites = UserInfoProvider.GetUserSites(uid).Columns("SiteName, SiteID");
                var  userRoles = UserInfoProvider.GetUserRoles(user, String.Empty, null, -1, "RoleName, RoleID, SiteID, ValidTo", false, true, true);
                DataTable membershipRolesData = null;

                if ((DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") == "")
                    || LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Membership))
                {
                    membershipRolesData = UserInfoProvider.GetUserMembershipRoles(user, "(ValidTo IS NULL OR ValidTo > @Date)", null, -1, "RoleName , CMS_Role.RoleID, SiteID, ValidTo, CMS_MembershipRole.MembershipID");
                }

                var memberships = CreateRoleCollection();
                var membershipsValidity = CreateValidityCollection();

                var siteRoles = CreateRoleCollection();
                var membershipRoles = CreateRoleCollection();
                var rolesValidity = CreateValidityCollection();
                var membershipRolesValidity = CreateValidityCollection();

                // Membership table must be created first!
                CreateMembershipsTable(user.UserID, userSites, memberships, membershipsValidity);
                CreateRolesTable(userRoles, userSites, membershipRolesData, siteRoles, membershipRoles, rolesValidity, membershipsValidity, membershipRolesValidity);

                securityCollections = new UserSecurityCollections
                {
                    SitesRoles = siteRoles,
                    MembershipRoles = membershipRoles,
                    Memberships = memberships,
                    RolesValidity = rolesValidity,
                    MembershipsValidity = membershipsValidity,
                    MembershipRoleValidity = membershipRolesValidity
                };
            }
            else
            {
                securityCollections = new UserSecurityCollections();
            }

            return securityCollections;
        }


        /// <summary>
        /// Creates collection for users's memberships
        /// </summary>
        private static void CreateMembershipsTable(int userId, IEnumerable<SiteInfo> userSites, SafeDictionary<string, SafeDictionary<string, int?>> memberships, SafeDictionary<int, DateTime?> membershipValidity)
        {
            if (userSites != null)
            {
                // Get all valid memberships for current user
                DataSet ds = MembershipInfoProvider.GetUserMemberships(userId);

                // If any memberships found
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Iterate through site collection
                    foreach (var site in userSites)
                    {
                        // Create the membership table for every site
                        var membershipSiteTable = new SafeDictionary<string, int?>();
                        memberships[site.SiteName.ToLowerInvariant()] = membershipSiteTable;

                        // Get the user memberships at the specified site
                        DataRow[] rows = ds.Tables[0].Select("MembershipSiteID = " + site.SiteID);

                        AddMembershipsToHashTable(rows, membershipSiteTable, membershipValidity);
                    }

                    // Add global memberships
                    var globalMemberships = new SafeDictionary<string, int?>();
                    memberships[UserInfo.GLOBAL_ROLES_KEY] = globalMemberships;

                    DataRow[] globalRows = ds.Tables[0].Select("MembershipSiteID IS NULL");
                    AddMembershipsToHashTable(globalRows, globalMemberships, membershipValidity);
                }
            }
        }


        /// <summary>
        /// Initializes the user sites roles collection by given data.
        /// </summary>
        private static void CreateRolesTable(DataTable userRoles, IEnumerable<SiteInfo> userSites, DataTable membershipRolesTable, SafeDictionary<string, SafeDictionary<string, int?>> siteRoles, SafeDictionary<string, SafeDictionary<string, int?>> membershipRoles, SafeDictionary<int, DateTime?> rolesValidity, SafeDictionary<int, DateTime?> membershipsValidity, SafeDictionary<int, DateTime?> membershipRolesValidity)
        {
            var unlimitedRoles = new SafeDictionary<int, bool>();

            // Read sites and roles into the collection
            if (userSites != null)
            {
                foreach (var site in userSites)
                {
                    string siteName = site.SiteName.ToLowerInvariant();

                    // Create the roles table
                    var rolesTable = new SafeDictionary<string, int?>();
                    siteRoles[siteName] = rolesTable;

                    // Get the user roles at the specified site
                    DataRow[] rows = userRoles.Select("SiteID = " + site.SiteID);
                    AddRoleRowToHashTable(rows, rolesTable, unlimitedRoles, rolesValidity, membershipsValidity, membershipRolesValidity);

                    if (membershipRolesTable != null)
                    {
                        // Create the membership table
                        var membershipTable = new SafeDictionary<string, int?>();
                        membershipRoles[siteName] = membershipTable;

                        rows = membershipRolesTable.Select("SiteID = " + site.SiteID);
                        AddRoleRowToHashTable(rows, membershipTable, unlimitedRoles, rolesValidity, membershipsValidity, membershipRolesValidity);
                    }
                }

                // Collect global roles
                var globalRolesTable = new SafeDictionary<string, int?>();
                siteRoles[UserInfo.GLOBAL_ROLES_KEY] = globalRolesTable;

                // Get the user global roles 
                DataRow[] globalRows = userRoles.Select("SiteID IS NULL");
                AddRoleRowToHashTable(globalRows, globalRolesTable, unlimitedRoles, rolesValidity, membershipsValidity, membershipRolesValidity);

                if (membershipRolesTable != null)
                {
                    var globalMembershipRolesTable = new SafeDictionary<string, int?>();
                    membershipRoles[UserInfo.GLOBAL_ROLES_KEY] = globalMembershipRolesTable;

                    // Membership global roles
                    globalRows = membershipRolesTable.Select("SiteID IS NULL");
                    AddRoleRowToHashTable(globalRows, globalMembershipRolesTable, unlimitedRoles, rolesValidity, membershipsValidity, membershipRolesValidity);
                }
            }
        }


        /// <summary>
        /// Add records to target collection given in rows collection.
        /// </summary>
        /// <param name="rows">Rows with data</param>
        /// <param name="targetHashTable">Collection with data</param>
        /// <param name="unlimitedRoles">Collection with rows with no limit datetime</param>
        /// <param name="rolesValidity">Collection with rows with non permanent roles</param>
        /// <param name="membershipsValidity">Collection with membership validity</param>
        /// <param name="membershipRoleValidity">Collection with roles and validity available thru membership</param>
        private static void AddRoleRowToHashTable(IEnumerable<DataRow> rows, SafeDictionary<string, int?> targetHashTable, SafeDictionary<int, bool> unlimitedRoles, SafeDictionary<int, DateTime?> rolesValidity, SafeDictionary<int, DateTime?> membershipsValidity, SafeDictionary<int, DateTime?> membershipRoleValidity)
        {
            foreach (DataRow rr in rows)
            {
                string roleName = DataHelper.GetStringValue(rr, "RoleName").ToLowerInvariant();
                DateTime validTo = DataHelper.GetDateTimeValue(rr, "ValidTo", DateTimeHelper.ZERO_TIME);
                int roleID = DataHelper.GetIntValue(rr, "RoleID");
                int membershipID = DataHelper.GetIntValue(rr, "MembershipID");
                targetHashTable[roleName] = roleID;

                // If valid is null -> add to temp role names collection
                if (validTo == DateTimeHelper.ZERO_TIME)
                {
                    unlimitedRoles[roleID] = true;
                }
                // Check if same role name is in unlimited roles
                else
                {
                    // If is in unlimited roles - remove limited restriction
                    if (unlimitedRoles.Contains(roleID))
                    {
                        rolesValidity.Remove(roleID);
                    }
                    else
                    {
                        // Add limited role to hash table
                        DateTime record = ValidationHelper.GetDateTime(rolesValidity[roleID], DateTimeHelper.ZERO_TIME);
                        if ((record == DateTimeHelper.ZERO_TIME) || (validTo > record))
                        {
                            // Only if no record is found or record with lesser date
                            if (validTo > record)
                            {
                                rolesValidity[roleID] = validTo;
                            }
                        }
                    }
                }

                if (membershipID > 0)
                {
                    DateTime mv = ValidationHelper.GetDateTime(membershipsValidity[membershipID], DateTimeHelper.ZERO_TIME);
                    if (mv == DateTimeHelper.ZERO_TIME)
                    {
                        membershipRoleValidity[roleID] = DateTimeHelper.ZERO_TIME;
                    }
                    else if ((membershipRoleValidity[roleID] == null) || (ValidationHelper.GetDateTime(membershipRoleValidity[roleID], DateTime.MaxValue) < mv))
                    {
                        membershipRoleValidity[roleID] = mv;
                    }
                }
            }
        }


        /// <summary>
        /// Add records to target collection given in rows collection.
        /// </summary>
        /// <param name="rows">Rows with data</param>
        /// <param name="targetHashTable">Hash table with data</param>
        /// <param name="membershipValidity">Collection with rows with validity data for memberships</param>
        private static void AddMembershipsToHashTable(IEnumerable<DataRow> rows, SafeDictionary<string, int?> targetHashTable, SafeDictionary<int, DateTime?> membershipValidity)
        {
            foreach (DataRow rr in rows)
            {
                // Get membership data from data row
                String membershipName = DataHelper.GetStringValue(rr, "MembershipName").ToLowerInvariant();
                DateTime validTo = DataHelper.GetDateTimeValue(rr, "ValidTo", DateTimeHelper.ZERO_TIME);
                int membershipID = DataHelper.GetIntValue(rr, "MembershipID");

                // Add membership hashtable to global membership table
                targetHashTable[membershipName] = membershipID;

                // Check validity. Add valid data to membership validity table
                if (validTo != DateTimeHelper.ZERO_TIME)
                {
                    membershipValidity[membershipID] = validTo;
                }
            }
        }


        private static SafeDictionary<string, SafeDictionary<string, int?>> CreateRoleCollection()
        {
            return new SafeDictionary<string, SafeDictionary<string, int?>>();
        }


        private static SafeDictionary<int, DateTime?> CreateValidityCollection()
        {
            return new SafeDictionary<int, DateTime?>();
        }
    }
}
