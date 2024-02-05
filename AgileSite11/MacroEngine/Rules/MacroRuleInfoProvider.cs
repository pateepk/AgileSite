using System;
using System.Collections;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    using TypedDataSet = InfoDataSet<MacroRuleInfo>;

    /// <summary>
    /// Class providing MacroRuleInfo management.
    /// </summary>
    public class MacroRuleInfoProvider : AbstractInfoProvider<MacroRuleInfo, MacroRuleInfoProvider>
    {
        /// <summary>
        /// Constructor which enables caching by code name and ID.
        /// </summary>
        public MacroRuleInfoProvider() 
            : base(MacroRuleInfo.TYPEINFO, new HashtableSettings()
            {
                ID = true,
                Name = true,
                UseWeakReferences = false,
            })
        {
        }


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all macro rules.
        /// </summary>
        public static ObjectQuery<MacroRuleInfo> GetMacroRules()
        {
            return ProviderObject.GetMacroRulesInternal();
        }


        /// <summary>
        /// Returns dataset of all macro rules matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        [Obsolete("Use method GetMacroRules() instead")]
        public static TypedDataSet GetMacroRules(string where, string orderBy, int topN, string columns)
        {
            return GetMacroRules().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns dataset of all macro rules matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        [Obsolete("Use method GetMacroRules() instead")]
        public static TypedDataSet GetMacroRules(string where, string orderBy)
        {
            return GetMacroRules(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns macro rule with specified ID.
        /// </summary>
        /// <param name="ruleId">Macro rule ID.</param>        
        public static MacroRuleInfo GetMacroRuleInfo(int ruleId)
        {
            return ProviderObject.GetMacroRuleInfoInternal(ruleId);
        }


        /// <summary>
        /// Returns macro rule with specified name.
        /// </summary>
        /// <param name="ruleName">Macro rule name.</param>                
        public static MacroRuleInfo GetMacroRuleInfo(string ruleName)
        {
            return ProviderObject.GetMacroRuleInfoInternal(ruleName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified macro rule.
        /// </summary>
        /// <param name="ruleObj">Macro rule to be set.</param>
        public static void SetMacroRuleInfo(MacroRuleInfo ruleObj)
        {
            ProviderObject.SetMacroRuleInfoInternal(ruleObj);
        }


        /// <summary>
        /// Deletes specified macro rule.
        /// </summary>
        /// <param name="ruleObj">Macro rule to be deleted.</param>
        public static void DeleteMacroRuleInfo(MacroRuleInfo ruleObj)
        {
            ProviderObject.DeleteMacroRuleInfoInternal(ruleObj);
        }


        /// <summary>
        /// Deletes macro rule with specified ID.
        /// </summary>
        /// <param name="ruleId">Macro rule ID.</param>
        public static void DeleteMacroRuleInfo(int ruleId)
        {
            MacroRuleInfo ruleObj = GetMacroRuleInfo(ruleId);
            DeleteMacroRuleInfo(ruleObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns condition in K# language from given rule info and parameters.
        /// </summary>
        /// <param name="rule">Rule to get the condition from</param>
        /// <param name="parameters">Parameter values to complete the condition</param>
        public string GetMacroRulesCondition(MacroRuleInfo rule, Hashtable parameters)
        {
            return ProviderObject.GetMacroRulesConditionInternal(rule, parameters);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns all macro rules.
        /// </summary>
        protected virtual ObjectQuery<MacroRuleInfo> GetMacroRulesInternal()
        {
            return GetObjectQuery();
        }


        /// <summary>
        /// Returns macro rule with specified ID.
        /// </summary>
        /// <param name="ruleId">Macro rule ID.</param>        
        protected virtual MacroRuleInfo GetMacroRuleInfoInternal(int ruleId)
        {
            return GetInfoById(ruleId);
        }


        /// <summary>
        /// Returns macro rule with specified name.
        /// </summary>
        /// <param name="ruleName">Macro rule name.</param>        
        protected virtual MacroRuleInfo GetMacroRuleInfoInternal(string ruleName)
        {
            return GetInfoByCodeName(ruleName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified macro rule.
        /// </summary>
        /// <param name="ruleObj">Macro rule to be set.</param>        
        protected virtual void SetMacroRuleInfoInternal(MacroRuleInfo ruleObj)
        {
            SetInfo(ruleObj);
        }


        /// <summary>
        /// Deletes specified macro rule.
        /// </summary>
        /// <param name="ruleObj">Macro rule to be deleted.</param>        
        protected virtual void DeleteMacroRuleInfoInternal(MacroRuleInfo ruleObj)
        {
            DeleteInfo(ruleObj);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns condition in K# language from given rule info and parameters.
        /// </summary>
        /// <param name="rule">Rule to get the condition from</param>
        /// <param name="parameters">Parameter values to complete the condition</param>
        protected virtual string GetMacroRulesConditionInternal(MacroRuleInfo rule, Hashtable parameters)
        {
            if (rule != null)
            {
                string condition = rule.MacroRuleCondition;
                foreach (string p in parameters.Keys)
                {
                    condition = condition.Replace("$" + p, ValidationHelper.GetString(parameters[p], ""));
                }
                return condition;
            }
            return null;
        }

        #endregion
    }
}
