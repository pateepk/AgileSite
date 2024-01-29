using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Newsletters;

[assembly: RegisterExtension(typeof(IssueInfoMethods), typeof(IssueInfo))]


namespace CMS.Newsletters
{
    /// <summary>
    /// Macro methods for <see cref="IssueInfo"/> class.
    /// </summary>
    public class IssueInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns all recipients for the given issue belonging to the email campaign.
        /// </summary>
        /// <remarks>The method returns recipients only for email campaigns (<see cref="EmailCommunicationTypeEnum.EmailCampaign"/>). 
        /// It does not return recipients for newsletters (<see cref="EmailCommunicationTypeEnum.Newsletter"/>).</remarks>
        [MacroMethod(typeof(IDataQuery), "Returns all recipients for the given email belonging to the email campaign (does not return recipients for the newsletter emails).", 0)]
        [MacroMethodParam(0, "obj", typeof(IssueInfo), "Object instance")]
        public static object GetAllRecipients(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                var issue = GetParamValue(parameters, 0, default(IssueInfo));
                if (issue == null)
                {
                    return null;
                }

                var recipientsProvider = new CampaignEmailRecipientsProvider(issue);
                return recipientsProvider.GetAllRecipients();
            }

            throw new System.NotSupportedException();
        }


        /// <summary>
        /// Returns count of all contacts subscribed to given issue, excludes opted out contacts and bounced.
        /// </summary>
        /// <remarks>The method returns recipients only for email campaigns (<see cref="EmailCommunicationTypeEnum.EmailCampaign"/>). 
        /// It does not return recipients for newsletters (<see cref="EmailCommunicationTypeEnum.Newsletter"/>).</remarks>
        [MacroMethod(typeof(int), "Returns marketable recipients for the given email belonging to the email campaign (does not return recipients for the newsletter emails).", 0)]
        [MacroMethodParam(0, "obj", typeof(IssueInfo), "Object instance")]
        public static object GetMarketableRecipientsCount(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                var issue = GetParamValue(parameters, 0, default(IssueInfo));
                if (issue == null)
                {
                    return null;
                }

                var recipientsProvider = new CampaignEmailRecipientsProvider(issue);
                return recipientsProvider.GetMarketableRecipients().Count;
            }

            throw new System.NotSupportedException();
        }
    }
}