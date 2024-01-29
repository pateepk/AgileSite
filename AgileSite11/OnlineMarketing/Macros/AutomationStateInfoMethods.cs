using System;

using CMS;
using CMS.Automation;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Newsletters;
using CMS.OnlineMarketing;

[assembly: RegisterExtension(typeof(AutomationStateInfoMethods), typeof(AutomationStateInfo))]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Online marketing methods - wrapping methods for macro resolver.
    /// </summary>
    internal class AutomationStateInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns GUID of last newsletter that was sent to the user in scope of given automation state. 
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (IssueInfo), "Returns last newsletter issue that was sent to the contact of the state.", 1)]
        [MacroMethodParam(0, "state", typeof (AutomationStateInfo), "Process instance to get last newsletter issue from.")]
        public static object GetLastNewsletterIssue(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new NotSupportedException();
            }
            return GetLastNewsletterIssue(parameters[0] as AutomationStateInfo);
        }


        /// <summary>
        /// Returns last newsletter issue that was sent to the contact of the state.
        /// </summary>
        /// <param name="state">Automation state</param>
        private static IssueInfo GetLastNewsletterIssue(AutomationStateInfo state)
        {
            // Check if automation state is null
            if (state == null)
            {
                return null;
            }

            // Get newsletter issue GUID and Site ID
            var newsletterGuid = ValidationHelper.GetGuid(state.StateCustomData[SendNewsletterIssueAction.LAST_SENT_NEWSLETTER_ISSUE_GUID_KEY], Guid.Empty);
            var newsletterSiteId = ValidationHelper.GetInteger(state.StateCustomData[SendNewsletterIssueAction.LAST_SENT_NEWSLETTER_ISSUE_SITEID_KEY], 0);

            return IssueInfoProvider.GetIssueInfo(newsletterGuid, newsletterSiteId);
        }
    }
}