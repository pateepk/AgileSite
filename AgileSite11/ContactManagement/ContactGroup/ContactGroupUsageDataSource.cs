using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(ContactGroupUsageDataSource))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides statistical information about contact groups.
    /// </summary>
    internal class ContactGroupUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.OnlineMarketing.ContactGroup";
            }
        }


        /// <summary>
        /// Add contact groups statistics to the usage data collection.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            var contactGroups = ContactGroupInfoProvider.GetContactGroups().ToList();

            usageData.Add("ContactGroupsCount", contactGroups.Count);
            usageData.Add("DynamicContactGroupsCount", GetEnabledDynamicGroupsCount(contactGroups));
            usageData.Add("StaticContactGroupsCount", GetEnabledStaticGroupCount(contactGroups));
            usageData.Add("ContactGroupWithAccount", GetNumberOfContactGroupsWithAccount());
            usageData.Add("NumberContactInContactGroup", JsonConvert.SerializeObject(GetContactsCountPerContactGroup()));

            return usageData;
        }


        /// <summary>
        /// Returns number of dynamic contact groups.
        /// </summary>
        internal int GetEnabledDynamicGroupsCount(IEnumerable<ContactGroupInfo> contactGroups)
        {
            return contactGroups.Count(cg => !string.IsNullOrEmpty(cg.ContactGroupDynamicCondition) && cg.ContactGroupEnabled);
        }


        /// <summary>
        /// Returns number of static contact groups.
        /// </summary>
        internal int GetEnabledStaticGroupCount(IEnumerable<ContactGroupInfo> contactGroups)
        {
            return contactGroups.Count(cg => string.IsNullOrEmpty(cg.ContactGroupDynamicCondition) && cg.ContactGroupEnabled);
        }


        /// <summary>
        /// Returns number of contacts per each contact group.
        /// </summary>
        internal IList<int> GetContactsCountPerContactGroup()
        {
            return ContactGroupMemberInfoProvider.GetRelationships()
                                                 .Column("ContactGroupMemberContactGroupID")
                                                 .AddColumn(new CountColumn("ContactGroupMemberContactGroupID").As("Contacts"))
                                                 .WhereEquals("ContactGroupMemberType", ContactGroupMemberTypeEnum.Contact)
                                                 .GroupBy("ContactGroupMemberContactGroupID")
                                                 .AsNested()
                                                 .Column("Contacts")
                                                 .GetListResult<int>();
        }


        /// <summary>
        /// Returns how many contact groups contains account.
        /// </summary>
        internal int GetNumberOfContactGroupsWithAccount()
        {
            return ContactGroupMemberInfoProvider.GetRelationships()
                                                 .Column("ContactGroupMemberContactGroupID")
                                                 .WhereEquals("ContactGroupMemberType", ContactGroupMemberTypeEnum.Account)
                                                 .GroupBy("ContactGroupMemberContactGroupID")
                                                 .Count();
        }
    }
}
