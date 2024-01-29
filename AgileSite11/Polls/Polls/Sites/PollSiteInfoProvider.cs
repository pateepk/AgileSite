using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Polls
{
    /// <summary>
    /// Class providing PollSiteInfo management.
    /// </summary>
    public class PollSiteInfoProvider : AbstractInfoProvider<PollSiteInfo, PollSiteInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the PollSiteInfo structure for the specified pollSite.
        /// </summary>
        /// <param name="pollId">PollID</param>
        /// <param name="siteId">SiteID</param>
        public static PollSiteInfo GetPollSiteInfo(int pollId, int siteId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@PollID", pollId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Polls.PollSite.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new PollSiteInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns PollSiteInfo objects specified by parameters.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY statement</param>
        /// <param name="topN">Number of returned records</param>
        /// <param name="columns">Data columns to return</param>
        public static DataSet GetPollSiteInfos(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("Polls.PollSite.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified pollSite.
        /// </summary>
        /// <param name="pollSite">PollSite to set</param>
        public static void SetPollSiteInfo(PollSiteInfo pollSite)
        {
            if (pollSite != null)
            {
                // Check IDs
                if ((pollSite.SiteID <= 0) || (pollSite.PollID <= 0))
                {
                    throw new Exception("[PollSiteInfoProvider.SetPollSiteInfo]: Object IDs not set.");
                }

                // Get existing
                PollSiteInfo existing = GetPollSiteInfo(pollSite.PollID, pollSite.SiteID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                    //pollSite.Generalized.UpdateData();
                }
                else
                {
                    pollSite.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[PollSiteInfoProvider.SetPollSiteInfo]: No PollSiteInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified pollSite.
        /// </summary>
        /// <param name="infoObj">PollSite object</param>
        public static void DeletePollSiteInfo(PollSiteInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified pollSite.
        /// </summary>
        /// <param name="pollId">PollID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemovePollFromSite(int pollId, int siteId)
        {
            // Get the objects
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            PollInfo poll = PollInfoProvider.GetPollInfo(pollId);
            if ((site != null) && (poll != null))
            {
                PollSiteInfo infoObj = GetPollSiteInfo(pollId, siteId);
                DeletePollSiteInfo(infoObj);

                poll.Sites[site.SiteName.ToLowerCSafe()] = null;
            }

            PollInfoProvider.ClearPollLicHash();
        }


        /// <summary>
        /// Adds specified poll to the site.
        /// </summary>
        /// <param name="pollId">PollID</param>
        /// <param name="siteId">SiteID</param>
        public static void AddPollToSite(int pollId, int siteId)
        {
            // Get the objects
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            PollInfo poll = PollInfoProvider.GetPollInfo(pollId);

            if ((site != null) && (poll != null))
            {
                PollInfoProvider.ClearPollLicHash();

                if (!PollInfoProvider.LicenseVersionCheck(site.DomainName, FeatureEnum.Polls, ObjectActionEnum.Edit))
                {
                    LicenseHelper.GetAllAvailableKeys(FeatureEnum.Polls);
                    //throw new Exception(ASCIIEncoding.ASCII.GetString(Convert.FromBase64String("W1BvbGxzLlZlcnNpb25DaGVja10gUmVxdWVzdGVkIGFjdGlvbiBjYW4gbm90IGJlIGV4ZWN1dGVkIGR1ZSB0byBsaWNlbnNlIGxpbWl0YXRpb25zLg==")));
                }


                if (!poll.IsInSite(site.SiteName))
                {
                    // Create new binding
                    PollSiteInfo infoObj = new PollSiteInfo();
                    infoObj.SiteID = siteId;
                    infoObj.PollID = pollId;

                    // Save to the database
                    SetPollSiteInfo(infoObj);

                    poll.Sites[site.SiteName.ToLowerCSafe()] = site.SiteID;
                }

                PollInfoProvider.ClearPollLicHash();
            }
        }

        #endregion
    }
}