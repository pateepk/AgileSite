using System;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasVotedInSpecifiedPollInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        private const string ANSWER_WARNING_TEXT = "Input 'answer' in macro or macro rule named VotedInPoll contained invalid answerText.";


        /// <summary>
        /// Translates CMSContactHasVotedInSpecifiedPoll Macro rule.
        /// Contact {_perfectum} voted {answer} in poll {item}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            var stringGuid = ruleParameters["item"].Value;
            string perfectum = ruleParameters["_perfectum"].Value;
            Guid pollGuid = stringGuid.ToGuid(Guid.Empty);
            string answerText = ruleParameters["answer"].Value;

            if (pollGuid == Guid.Empty)
            {
                MacroValidationHelper.LogInvalidGuidParameter("VotedInPoll", stringGuid);
                return new ObjectQuery<ContactInfo>().NoResults();
            }

            var polls = new DataQuery().From("Polls_Poll")
                                       .WhereEquals("PollGUID", pollGuid)
                                       .Column("PollID");

            var activities = ActivityInfoProvider.GetActivities()
                                                     .WhereEquals("ActivityType", PredefinedActivityType.POLL_VOTING)
                                                     .WhereIn("ActivityItemID", polls)
                                                     .Column("ActivityContactID");
            if (!string.IsNullOrEmpty(answerText))
            {
                // Fetches all answerIDs from database. AnswerIDs are stored as concatenated string in ActivityValue column and it is not easy to return matching contacts using one query
                var answerIDs = new DataQuery().From("Polls_PollAnswer")
                                               .WhereIn("AnswerPollID", polls)
                                               .WhereEquals("AnswerText", answerText)
                                               .Column("AnswerID")
                                               .GetListResult<int>();

                if (answerIDs.Count == 0)
                {
                    EventLogProvider.LogEvent(EventType.WARNING, "Macro", "VotedInPoll", ANSWER_WARNING_TEXT, loggingPolicy: new LoggingPolicy(TimeSpan.FromDays(1)));
                    MacroValidationHelper.LogInvalidGuidParameter("VotedInPoll", stringGuid);
                    return new ObjectQuery<ContactInfo>().NoResults(); 
                }

                var answerIDWhereCondition = new WhereCondition();
                foreach (int answerID in answerIDs)
                {
                    answerIDWhereCondition.WhereContains("ActivityValue", string.Format("|{0}|", answerID))
                                          .Or();
                }

                activities = activities.Where(answerIDWhereCondition);
            }

            var contacts = ContactInfoProvider.GetContacts();

            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", activities);
            }
            else
            {
                contacts.WhereIn("ContactID", activities);
            }

            return contacts;
        }
    }
}