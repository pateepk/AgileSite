using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;

namespace CMS.Polls
{
    /// <summary>
    /// Class providing PollRoleInfo management.
    /// </summary>
    public class PollRoleInfoProvider : AbstractInfoProvider<PollRoleInfo, PollRoleInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the PollRoleInfo structure for the specified pollRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="pollId">PollID</param>
        public static PollRoleInfo GetPollRoleInfo(int roleId, int pollId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoleID", roleId);
            parameters.Add("@PollID", pollId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Polls.PollRole.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new PollRoleInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns PollRoleInfo objects specified by parameters.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY statement</param>
        /// <param name="topN">Number of returned records</param>
        /// <param name="columns">Data columns to return</param>
        public static DataSet GetPollRoleInfos(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("Polls.PollRole.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified pollRole.
        /// </summary>
        /// <param name="pollRole">PollRole to set</param>
        public static void SetPollRoleInfo(PollRoleInfo pollRole)
        {
            if (pollRole != null)
            {
                // Check IDs
                if ((pollRole.RoleID <= 0) || (pollRole.PollID <= 0))
                {
                    throw new Exception("[PollRoleInfoProvider.SetPollRoleInfo]: Object IDs not set.");
                }

                // Get existing
                PollRoleInfo existing = GetPollRoleInfo(pollRole.RoleID, pollRole.PollID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                    //pollRole.Generalized.UpdateData();
                }
                else
                {
                    pollRole.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[PollRoleInfoProvider.SetPollRoleInfo]: No PollRoleInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified pollRole.
        /// </summary>
        /// <param name="infoObj">PollRole object</param>
        public static void DeletePollRoleInfo(PollRoleInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified pollRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="pollId">PollID</param>
        public static void RemoveRoleFromPoll(int roleId, int pollId)
        {
            // Get the objects
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            PollInfo poll = PollInfoProvider.GetPollInfo(pollId);

            RemoveRoleFromPoll(role, poll);
        }


        /// <summary>
        /// Deletes specified poll role.
        /// </summary>
        /// <param name="role">RoleID</param>
        /// <param name="poll">PollID</param>
        public static void RemoveRoleFromPoll(RoleInfo role, PollInfo poll)
        {
            if ((role != null) && (poll != null))
            {
                // Remove relation
                PollRoleInfo infoObj = GetPollRoleInfo(role.RoleID, poll.PollID);
                DeletePollRoleInfo(infoObj);

                String name = role.RoleName.ToLowerCSafe();

                // Add to poll.AllowedRoles hashtable
                if (role.SiteID == 0)
                {
                    name = "." + name;
                }

                if (poll.AllowedRoles.Contains(name))
                {
                    // Remove from poll.roles hashtable
                    poll.AllowedRoles.Remove(name);
                }
            }
        }


        /// <summary>
        /// Adds specified role to the poll.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="pollId">PollID</param>
        public static void AddRoleToPoll(int roleId, int pollId)
        {
            // Get the objects
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            PollInfo poll = PollInfoProvider.GetPollInfo(pollId);

            AddRoleToPoll(role, poll);
        }


        /// <summary>
        /// Adds specified role to the poll.
        /// </summary>
        /// <param name="role">Role</param>
        /// <param name="poll">Poll</param>
        public static void AddRoleToPoll(RoleInfo role, PollInfo poll)
        {
            if ((role != null) && (poll != null))
            {
                // Create new binding
                PollRoleInfo infoObj = new PollRoleInfo();
                infoObj.RoleID = role.RoleID;
                infoObj.PollID = poll.PollID;

                // Save to the database
                SetPollRoleInfo(infoObj);

                String name = role.RoleName.ToLowerCSafe();
                
                // Add to poll.AllowedRoles hashtable
                if (role.SiteID == 0)
                {
                    name = "." + name;
                }

                poll.AllowedRoles[name] = role.RoleID;
            }
        }

        #endregion
    }
}