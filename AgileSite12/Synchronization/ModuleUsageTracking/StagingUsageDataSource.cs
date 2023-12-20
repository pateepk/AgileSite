using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterModuleUsageDataSource(typeof(StagingUsageDataSource))]

namespace CMS.Synchronization
{
    /// <summary>
    /// Module usage tracking data source for staging.
    /// </summary>
    internal class StagingUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Staging data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Staging";
            }
        }


        /// <summary>
        /// Get module usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            bool anyChangesLogged = false;
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Check whether staging is explicitly disabled in web.config
            if (StagingTaskInfoProvider.StagingEnabled)
            {
                // Get Site names of all sites
                var query = new ObjectQuery("cms.site").Columns("SiteName");
                DataSet queryResult = query.Result;

                if (!DataHelper.DataSourceIsEmpty(queryResult))
                {
                    // Determine whether there exists at least one site with at least one change-logging settings key enabled
                    foreach (DataRow dr in queryResult.Tables[0].Rows)
                    {
                        string siteName = ValidationHelper.GetString(dr["SiteName"], String.Empty);

                        if (CoreServices.Settings[siteName + ".CMSStagingLogChanges"].ToBoolean(false) ||
                            CoreServices.Settings[siteName + ".CMSStagingLogObjectChanges"].ToBoolean(false) ||
                            CoreServices.Settings[siteName + ".CMSStagingLogStagingChanges"].ToBoolean(false) ||
                            CoreServices.Settings[siteName + ".CMSStagingLogDataChanges"].ToBoolean(false))
                        {
                            anyChangesLogged = true;
                            break;
                        }
                    }
                }
            }

            result.Add("StagingEnabled", anyChangesLogged);

            return result;
        }
    }
}
