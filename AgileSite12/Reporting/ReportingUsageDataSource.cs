using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Reporting;

[assembly: RegisterModuleUsageDataSource(typeof(ReportingUsageDataSource))]

namespace CMS.Reporting
{
    /// <summary>
    /// Provides statistical information about reporting module.
    /// </summary>
    internal class ReportingUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Reporting";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            usageData.Add("ReportingSubscribersCount", GetReportingSubscribersCount());

            return usageData;
        }


        /// <summary>
        /// Returns number of all users that are subscribed to at least one report.
        /// </summary>
        internal int GetReportingSubscribersCount()
        {
            return new ObjectQuery<ReportSubscriptionInfo>().Distinct().Column("ReportSubscriptionUserID").Count;
        }
    }
}
