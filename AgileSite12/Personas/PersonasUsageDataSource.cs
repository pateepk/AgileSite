using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Personas;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(PersonasUsageDataSource))]

namespace CMS.Personas
{
    /// <summary>
    /// Provides statistical information about personas module.
    /// </summary>
    internal class PersonasUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Personas";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml };

            usageData.Add("EnabledPersonasCount", GetEnabledPersonasCount());
            usageData.Add("TotalPersonasCount", PersonaInfoProvider.GetPersonas().Count);
            usageData.Add("PersonasRulesCountPerRuleType", JsonConvert.SerializeObject(GetNumberOfRulesInPersonasGroupedByType(), settings));
            usageData.Add("NumberOfContactsInEachPersona", JsonConvert.SerializeObject(GetNumberOfContactsInEachPersona(), settings));

            return usageData;
        }


        /// <summary>
        /// Returns number of enabled personas.
        /// </summary>
        internal int GetEnabledPersonasCount()
        {
            return PersonaInfoProvider.GetPersonas()
                                      .WhereTrue("PersonaEnabled")
                                      .Count;
        }


        /// <summary>
        /// Returns number of rule types per each score.
        /// </summary>
        internal List<Dictionary<int, int>> GetNumberOfRulesInPersonasGroupedByType()
        {
            return RuleInfoProvider.GetRules()
                                   .WhereTrue("RuleBelongsToPersona")
                                   .Columns("RuleScoreID", "RuleType")
                                   .AddColumn(new CountColumn("RuleScoreID").As("RulesInScore"))
                                   .GroupBy("RuleScoreID", "RuleType")
                                   .Select(dataRow => new
                                   {
                                       RuleScoreID = dataRow[0].ToInteger(0),
                                       RuleType = dataRow[1].ToInteger(0),
                                       RulesInScore = dataRow[2].ToInteger(0)
                                   })
                                   .GroupBy(r => r.RuleScoreID, r => new KeyValuePair<int, int>(r.RuleType, r.RulesInScore))
                                   .Select(g => g.ToDictionary(pair => pair.Key, pair => pair.Value))
                                   .ToList();
        }


        /// <summary>
        /// Returns number of contacts in each persona.
        /// </summary>
        internal IList<int> GetNumberOfContactsInEachPersona()
        {
            return ContactInfoProvider.GetContacts()
                                      .Column("ContactPersonaID")
                                      .AddColumn(new CountColumn("ContactPersonaID").As("ContactsInPersona"))
                                      .WhereNotNull("ContactPersonaID")
                                      .GroupBy("ContactPersonaID")
                                      .AsNested()
                                      .Column("ContactsInPersona")
                                      .GetListResult<int>();
        }
    }
}
