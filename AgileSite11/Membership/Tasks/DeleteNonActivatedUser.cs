using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class contains definition of delete non activated users task.
    /// </summary>
    public class DeleteNonActivatedUser : ITask
    {
        /// <summary>
        /// Task execution method.
        /// </summary>
        /// <param name="task">Task to execute</param>        
        public string Execute(TaskInfo task)
        {
            try
            {
                string siteName = string.Empty;

                // Get site name           
                SiteInfo si = SiteInfoProvider.GetSiteInfo(task.TaskSiteID);
                if (si == null)
                {
                    return null;
                }

                siteName = si.SiteName;

                // If registration email confirmation is enabled
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationEmailConfirmation"))
                {
                    // Get number of days after non activated users should be deleted
                    int deleteAfter = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSDeleteNonActivatedUserAfter");
                    if (deleteAfter > 0)
                    {
                        // Build where condition
                        string where = "(UserEnabled = 0) AND (UserActivationDate IS NULL) AND (ISNULL(UserWaitingForApproval, 0) = 0) AND @DeleteDateTime > UserCreated";

                        // Prepare siteID param
                        QueryDataParameters parameters = new QueryDataParameters();
                        parameters.Add("@SiteID", si.SiteID);
                        parameters.Add("@DeleteDateTime", DateTime.Now.AddDays(-deleteAfter));

                        // Execute
                        DataSet ds = ConnectionHelper.ExecuteQuery("cms.user.selectallofsite", parameters, where);

                        // Delete all found users
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                int userID = ValidationHelper.GetInteger(dr["UserID"], 0);
                                if (userID != 0)
                                {
                                    // Delete user
                                    UserInfoProvider.DeleteUser(userID);
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
    }
}