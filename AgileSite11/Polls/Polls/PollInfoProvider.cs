using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Polls
{
    /// <summary>
    /// Class providing PollInfo management.
    /// </summary>
    public class PollInfoProvider : AbstractInfoProvider<PollInfo, PollInfoProvider>
    {
        #region "Variables"

        // Table of polls license limitations
        private static CMSStatic<SafeDictionary<string, int?>> mLicPolls = new CMSStatic<SafeDictionary<string, int?>>(() => new SafeDictionary<string, int?>());


        // Table of polls license limitations
        private static SafeDictionary<string, int?> LicPolls
        {
            get
            {
                return mLicPolls;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public PollInfoProvider()
            : base(PollInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true
            })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns all existing polls.
        /// </summary>
        public static ObjectQuery<PollInfo> GetPolls()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static PollInfo GetPollInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the PollInfo structure for the specified poll.
        /// </summary>
        /// <param name="pollId">Poll id</param>
        public static PollInfo GetPollInfo(int pollId)
        {
            return ProviderObject.GetInfoById(pollId);
        }


        /// <summary>
        /// Returns the PollInfo structure specified by PollCodeName and SiteID.
        /// </summary>
        /// <param name="pollCodeName">Poll code name (leading period denotes global poll)</param>
        /// <param name="siteId">Site ID</param>
        public static PollInfo GetPollInfo(string pollCodeName, int siteId)
        {
            return GetPollInfo(pollCodeName, siteId, 0);
        }


        /// <summary>
        /// Returns the PollInfo structure specified by PollCodeName, SiteID and GroupID.
        /// If siteId is not 0 and no poll is found, tries to find the poll
        /// among global polls.
        /// </summary>
        /// <param name="pollCodeName">Poll code name (leading period denotes global poll)</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Group ID</param>
        public static PollInfo GetPollInfo(string pollCodeName, int siteId, int groupId)
        {
            return ProviderObject.GetPollInternal(pollCodeName, siteId, groupId);
        }


        /// <summary>
        /// Returns the Dataset structure of all polls that match specified conditions.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderby">ORDER BY</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        public static InfoDataSet<PollInfo> GetPolls(string where, string orderby)
        {
            return GetPolls(where, orderby, 0, null);
        }


        /// <summary>
        /// Returns the Dataset structure of all polls that match specified conditions.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderby">ORDER BY</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Selected columns</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        public static InfoDataSet<PollInfo> GetPolls(string where, string orderby, int topN, string columns)
        {
            return ProviderObject.GetPollsInternal(where, orderby, topN, columns);
        }


        /// <summary>
        /// Returns the Dataset structure of all polls of the specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        public static InfoDataSet<PollInfo> GetPolls(int siteId, int topN = 0, string columns = null)
        {
            return ProviderObject.GetPollsInternal(siteId, topN, columns);
        }


        /// <summary>
        /// Returns the Dataset structure of all polls of the specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        public static InfoDataSet<PollInfo> GetPolls(string siteName, int topN = 0, string columns = null)
        {
            // Get polls of the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[PollInfoProvider.GetPolls]: Site name '" + siteName + "' does not exist.");
            }

            return GetPolls(si.SiteID, topN, columns);
        }


        /// <summary>
        /// Returns all polls for specified site and group.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Group ID</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        public static InfoDataSet<PollInfo> GetGroupPolls(int siteId, int groupId)
        {
            return GetPolls("PollSiteID = " + siteId + " AND PollGroupID = " + groupId, "PollDisplayName");
        }


        /// <summary>
        /// Returns all polls for specified site and group.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="groupId">Group ID</param>
        /// <param name="orderBy">Order By</param>
        /// <param name="topN">TOP N</param>
        /// <param name="columns">Columns restriction</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        public static InfoDataSet<PollInfo> GetGroupPolls(int siteId, string where, int groupId, string orderBy, int topN, string columns)
        {
            where = SqlHelper.AddWhereCondition("(PollSiteID = " + siteId + " AND PollGroupID = " + groupId + ")", where);

            return GetPolls(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified poll.
        /// </summary>
        /// <param name="poll">Poll to set</param>
        public static void SetPollInfo(PollInfo poll)
        {
            CheckLicense();

            ProviderObject.SetInfo(poll);
        }


        /// <summary>
        /// Deletes specified poll.
        /// </summary>
        /// <param name="pollObj">Poll object</param>
        public static void DeletePollInfo(PollInfo pollObj)
        {
            ProviderObject.DeleteInfo(pollObj);
        }


        /// <summary>
        /// Deletes specified poll.
        /// </summary>
        /// <param name="pollId">Poll id</param>
        public static void DeletePollInfo(int pollId)
        {
            var pollObj = GetPollInfo(pollId);
            DeletePollInfo(pollObj);
        }


        /// <summary>
        /// Clear poll hash count values.
        /// </summary>
        public static void ClearPollLicHash()
        {
            LicPolls.Clear();
        }


        /// <summary>
        /// Assigns the poll to the site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="pollId">Poll ID</param>
        public static void AddPollToSite(int pollId, int siteId)
        {
            PollSiteInfoProvider.AddPollToSite(pollId, siteId);
        }


        /// <summary>
        /// Removes the pole from the site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="pollId">Poll ID</param>
        public static void RemovePollFromSite(int pollId, int siteId)
        {
            PollSiteInfoProvider.RemovePollFromSite(pollId, siteId);
        }


        /// <summary>
        /// Assigns the role to the site.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="pollId">Poll ID</param>
        public static void AddRoleToPoll(int roleId, int pollId)
        {
            PollRoleInfoProvider.AddRoleToPoll(roleId, pollId);
        }


        /// <summary>
        /// Removes the role from the site.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="pollId">Poll ID</param>
        public static void RemoveRoleFromPoll(int roleId, int pollId)
        {
            PollRoleInfoProvider.RemoveRoleFromPoll(roleId, pollId);
        }


        /// <summary>
        /// Gets the DataSet of roles assigned to the poll.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static InfoDataSet<RoleInfo> GetPollRoles(int pollId)
        {
            return GetPollRoles(pollId, null, null, -1, null);
        }


        /// <summary>
        /// Gets the DataSet of roles assigned to the poll.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY condition</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        public static InfoDataSet<RoleInfo> GetPollRoles(int pollId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetPollRolesInternal(pollId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets the DataSet of sites where the poll is assigned.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static InfoDataSet<SiteInfo> GetPollSites(int pollId)
        {
            return GetPollSites(pollId, null, null, 0, null);
        }


        /// <summary>
        /// Gets the DataSet of sites where the poll is assigned.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY condition</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        public static InfoDataSet<SiteInfo> GetPollSites(int pollId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetPollSitesInternal(pollId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns true if the role is allowed within the poll.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="pollId">Poll ID</param>
        public static bool IsRoleAllowedForPoll(int pollId, string roleName)
        {
            // Get poll
            var poll = GetPollInfo(pollId);
            if (poll != null)
            {
                return (poll.AllowedRoles[roleName.ToLowerCSafe()] != null);
            }

            return false;
        }


        /// <summary>
        /// Returns true is user already voted for this poll (use cookie).
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static bool HasVoted(int pollId)
        {
            var pi = GetPollInfo(pollId);
            if (pi == null)
            {
                return false;
            }

            // Check the content of the cookie "VotedPolls", content will be list of the poll codenames user already voted separated by semicolon
            string polls = CookieHelper.GetValue(CookieName.VotedPolls);

            if ((polls != null) && (polls.Trim() != ""))
            {
                // Multiple check because of backward compatibility
                return (polls.Contains("|" + pi.PollID + "|") || polls.Contains("|" + pi.PollCodeName + "|") || polls.Contains("|" + pi.PollCodeName.ToLowerCSafe() + "|"));
            }

            return false;
        }


        /// <summary>
        /// Sets the flag that user voted for the poll (use cookie like in the method above).
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static void SetVoted(int pollId)
        {
            var pi = GetPollInfo(pollId);
            if (pi == null)
            {
                return;
            }

            // Get the value from the coolie
            string polls = CookieHelper.GetValue(CookieName.VotedPolls);

            if (polls == null)
            {
                polls = "|" + pi.PollID + "|";
            }
            else
            {
                polls += pi.PollID + "|";
            }

            // Actualize the cookie
            CookieHelper.SetValue(CookieName.VotedPolls, polls, DateTime.Now.AddYears(1));
        }


        /// <summary>
        /// License version checker.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature type</param>
        /// <param name="action">Type of action - edit, insert, delete</param>
        /// <returns>Returns true if feature is without any limitations for domain and action</returns>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            // Parse domain name to remove port etc.
            if (domain != null)
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            }

            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, feature, (action != ObjectActionEnum.Insert));
            if (versionLimitations == 0)
            {
                return true;
            }

            if (feature != FeatureEnum.Polls)
            {
                return true;
            }

            if (LicPolls[domain] == null)
            {
                var siteId = LicenseHelper.GetSiteIDbyDomain(domain);
                if (siteId > 0)
                {
                    LicPolls[domain] = GetPolls().OnSite(siteId).GetCount();
                }
            }

            try
            {
                // Try add
                if (action == ObjectActionEnum.Insert)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(LicPolls[domain], -1) + 1)
                    {
                        return false;
                    }
                }

                // Get status
                if (action == ObjectActionEnum.Edit)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(LicPolls[domain], 0))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                ClearPollLicHash();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public static bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null)
        {
            domainName = domainName ?? RequestContext.CurrentDomain;

            if (!LicenseVersionCheck(domainName, FeatureEnum.Polls, action))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Polls);
                return false;
            }

            return true;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the PollInfo structure specified by PollCodeName, SiteID and GroupID.
        /// If siteId is not 0 and no poll is found, tries to find the poll
        /// among global polls.
        /// </summary>
        /// <param name="pollCodeName">Poll code name (leading period denotes global poll)</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Group ID</param>
        protected PollInfo GetPollInternal(string pollCodeName, int siteId, int groupId)
        {
            if (String.IsNullOrEmpty(pollCodeName))
            {
                return null;
            }

            if (pollCodeName.StartsWithCSafe("."))
            {
                // Global poll
                siteId = 0;
                groupId = 0;
                pollCodeName = pollCodeName.Substring(1);
            }

            // Try to find poll info in hashtable (assuming site ID is specified)
            PollInfo result = GetInfoByCodeName(pollCodeName, siteId, groupId);
            if (result == null)
            {
                // If not found, give it a try in global polls
                if (siteId > 0)
                {
                    result = GetInfoByCodeName(pollCodeName, 0, 0);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the Dataset structure of all polls that match specified conditions.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderby">ORDER BY</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Selected columns</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        protected InfoDataSet<PollInfo> GetPollsInternal(string where, string orderby, int topN, string columns)
        {
            var query = GetObjectQuery().Where(where).OrderBy(orderby).TopN(topN).Columns(columns).BinaryData(true);
            return query.TypedResult;
        }


        /// <summary>
        /// Returns the Dataset structure of all polls of the specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<PollInfo> instead")]
        protected InfoDataSet<PollInfo> GetPollsInternal(int siteId, int topN, string columns)
        {
            // If sitename not given, get all
            if (siteId == 0)
            {
                return GetObjectQuery().OrderBy("PollDisplayName").TopN(topN).Columns(columns).BinaryData(true).TypedResult;
            }

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.EnsureDataSet<PollInfo>();

            return ConnectionHelper.ExecuteQuery("Polls.Poll.SelectAllOfSite", parameters, null, "PollDisplayName", topN, columns).As<PollInfo>();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(PollInfo info)
        {
            base.SetInfo(info);

            ClearPollLicHash();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(PollInfo info)
        {
            base.DeleteInfo(info);

            ClearPollLicHash();
        }


        /// <summary>
        /// Gets the DataSet of roles assigned to the poll.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY condition</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        protected InfoDataSet<RoleInfo> GetPollRolesInternal(int pollId, string where, string orderBy, int topN, string columns)
        {
            if (pollId > 0)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@PollID", pollId);
                parameters.EnsureDataSet<RoleInfo>();

                // Get the data
                return ConnectionHelper.ExecuteQuery("polls.poll.selectroles", parameters, where, orderBy, topN, columns).As<RoleInfo>();
            }

            return null;
        }


        /// <summary>
        /// Gets the DataSet of sites where the poll is assigned.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY condition</param>
        /// <param name="topN">TOP N condition</param>
        /// <param name="columns">Columns restriction</param>
        protected InfoDataSet<SiteInfo> GetPollSitesInternal(int pollId, string where, string orderBy, int topN, string columns)
        {
            if (pollId > 0)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@PollID", pollId);
                parameters.EnsureDataSet<SiteInfo>();

                // Get the data
                return ConnectionHelper.ExecuteQuery("polls.poll.selectsites", parameters, where, orderBy, topN, columns).As<SiteInfo>();
            }

            return null;
        }

        #endregion
    }
}