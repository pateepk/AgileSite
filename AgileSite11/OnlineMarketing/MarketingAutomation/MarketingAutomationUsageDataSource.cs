using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Automation;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.MarketingAutomation;
using CMS.WorkflowEngine;

using Newtonsoft.Json;


[assembly: RegisterModuleUsageDataSource(typeof(MarketingAutomationUsageDataSource))]

namespace CMS.MarketingAutomation
{
    /// <summary>
    /// Provides statistical information about module.
    /// </summary>
    internal class MarketingAutomationUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.MarketingAutomation";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            var marketingAutomationProcesses = new ObjectQuery<WorkflowInfo>().WhereEquals("WorkflowType", WorkflowTypeEnum.Automation).ToList();

            usageData.Add("NumberOfEnabledMarketingAutomationProcesses", GetNumberOfEnabledMarketingAutomationProcesses(marketingAutomationProcesses));
            usageData.Add("NumberOfDisabledMarketingAutomationProcesses", GetNumberOfDisabledMarketingAutomationProcesses(marketingAutomationProcesses));
            usageData.Add("NumberOfContactsWithinEachProcess", JsonConvert.SerializeObject(GetNumberOfContactsWithinEachProcess()));

            return usageData;
        }


        /// <summary>
        /// Returns number of enabled marketing automation processes.
        /// </summary>
        internal int GetNumberOfEnabledMarketingAutomationProcesses(IEnumerable<WorkflowInfo> marketingAutomationProcesses)
        {
            return marketingAutomationProcesses.Count(ma => ma.WorkflowEnabled);
        }

        /// <summary>
        /// Returns number of disabled marketing automation processes.
        /// </summary>
        internal int GetNumberOfDisabledMarketingAutomationProcesses(IEnumerable<WorkflowInfo> marketingAutomationProcesses)
        {
            return marketingAutomationProcesses.Count(ma => !ma.WorkflowEnabled);
        }


        /// <summary>
        /// Returns number of contacts within each process.
        /// </summary>
        internal IList<int> GetNumberOfContactsWithinEachProcess()
        {
            return AutomationStateInfoProvider.GetAutomationStates()
                                              .Column("StateWorkflowID")
                                              .AddColumn(new CountColumn("StateWorkflowID").As("ContactsInAutomation"))
                                              .WhereEquals("StateObjectType", PredefinedObjectType.CONTACT)
                                              .GroupBy("StateWorkflowID")
                                              .AsNested()
                                              .Column("ContactsInAutomation")
                                              .GetListResult<int>();
        }
    }
}
