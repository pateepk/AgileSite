using System;
using System.Linq;
using System.Text;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Extensions of <see cref="ContactGroupInfo"/>.
    /// </summary>
    internal static class ContactGroupInfoExtensions
    {
        /// <summary>
        /// Checks whether given contact group is affected by given activity type, that means if it needs to be recalculated when it performs.
        /// </summary>
        /// <param name="cg">Contact group to check the activity type for</param>
        /// <param name="activityType">Activity to check affection for. You can use predefined types from <see cref="PredefinedActivityType"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="activityType"/> is null</exception>
        internal static bool IsAffectedByActivityType(this ContactGroupInfo cg, string activityType)
        {
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }

            return CachedMacroConditionAnalyzer.IsAffectedByActivityType(cg.ContactGroupDynamicCondition, activityType);
        }


        /// <summary>
        /// Checks whether given contact group is affected by contact attribute change, that means if it needs to be recalculated when attribute has changed.
        /// </summary>
        /// <param name="cg">Contact group to check the attribute affection</param>
        /// <param name="attribute">Name of the changed attribute column</param>
        /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is null</exception>
        internal static bool IsAffectedByAttributeChange(this ContactGroupInfo cg, string attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            return CachedMacroConditionAnalyzer.IsAffectedByAttributeChange(cg.ContactGroupDynamicCondition, attribute);
        }
    }
}