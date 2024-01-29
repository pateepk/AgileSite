using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;

[assembly: RegisterModuleUsageDataSource(typeof(ContactUsageDataSource))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides statistical information about contacts.
    /// </summary>
    internal class ContactUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.OnlineMarketing.Contact";
            }
        }


        /// <summary>
        /// Add contacts statistics to the usage data collection.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            
            usageData.Add("TotalContactsCount", GetTotalContactsCount());

            return usageData;
        }


        internal int GetTotalContactsCount()
        {
            return ContactInfoProvider.GetContacts().Count;
        }
    }
}
