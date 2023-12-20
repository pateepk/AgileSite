using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Metadata for a MacroRule in Online Marketing context. See <see cref="MacroRuleMetadataContainer"/> for information on how to use it to 
    /// speed up recalculation of contact groups.
    /// </summary>
    public struct MacroRuleMetadata
    {
        /// <summary>
        /// A constant representing all activities. Is used in <see cref="AffectingActivities"/> to mark that all activities affect this macro rule.
        /// </summary>
        public static string ALL_ACTIVITIES = "ALL_ACTIVITIES";


        /// <summary>
        /// A constant representing all attributes. Is used in <see cref="AffectingAttributes"/> to mark that all attributes affect this macro rule.
        /// </summary>
        public static string ALL_ATTRIBUTES = "ALL_ATTRIBUTES";


        /// <summary>
        /// Codename of the <see cref="MacroRuleInfo"/>.
        /// </summary>
        public readonly string MacroRuleName;


        /// <summary>
        /// A translator to use for <see cref="MacroRuleInfo"/> with <see cref="MacroRuleName"/>.
        /// </summary>
        public readonly IMacroRuleInstanceTranslator Translator;


        /// <summary>
        /// Determines whether the macro rule is affected by change of contact's attribute.
        /// </summary>
        public readonly IList<string> AffectingAttributes;


        /// <summary>
        /// List with <see cref="ALL_ACTIVITIES"/> to mark that the rule should be recalculated on every activity.
        /// </summary>
        public static ReadOnlyCollection<string> mAllActivitiesList = new ReadOnlyCollection<string>(new List<string>(1) { ALL_ACTIVITIES });
        

        /// <summary>
        /// Activities that affect given macro rule (that means if any of these activities performs, the rule should be recalculated).
        /// </summary>
        public readonly IList<string> AffectingActivities;
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="macroRuleName">Rule of the macro (see <see cref="MacroRuleInfo.MacroRuleName"/>)</param>
        /// <param name="translator">A translator to use for macro with name <paramref name="macroRuleName"/></param>
        /// <param name="affectingActivities">
        /// List of activities that affect this macro rule. The rule gets recalculated only if one of these activity types performs.
        /// Leave null to not recalculate the rule on any activity.
        /// </param>
        /// <param name="affectingAttributes">
        /// List of attributes that affect this macro rule. The rule gets recalculated only if one of these attributes changes.
        /// Leave null to not recalculate the rule on any attribute change.
        /// </param>
        public MacroRuleMetadata(string macroRuleName, IMacroRuleInstanceTranslator translator, IList<string> affectingActivities, IList<string> affectingAttributes)
        {
            MacroRuleName = macroRuleName;
            Translator = translator;
            // By default, all activities and attributes affect given MacroRule, so that it gets recalculated when it performs
            AffectingActivities = affectingActivities ?? new List<string>(0);
            AffectingAttributes = affectingAttributes ?? new List<string>(0);
        }
    }
}