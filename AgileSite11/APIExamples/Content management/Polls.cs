using CMS.Polls;
using CMS.Helpers;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to polls.
    /// </summary>
    /// <pageTitle>Polls</pageTitle>
    internal class PollsMain
    {
        /// <summary>
        /// Holds poll API examples.
        /// </summary>
        /// <groupHeading>Polls</groupHeading>
        private class Polls
        {
            /// <heading>Creating a poll</heading>
            private void CreatePoll()
            {
                // Creates a new poll object
                PollInfo newPoll = new PollInfo();

                // Sets the poll properties (the poll is global)
                newPoll.PollDisplayName = "Color poll";
                newPoll.PollCodeName = "ColorPoll";
                newPoll.PollTitle = "Color poll";
                newPoll.PollQuestion = "What is your favorite color?";
                newPoll.PollResponseMessage = "Thank you for voting.";
                newPoll.PollAllowMultipleAnswers = false;
                newPoll.PollAccess = SecurityAccessEnum.AllUsers;

                // Saves the new poll to the database
                PollInfoProvider.SetPollInfo(newPoll);
            }


            /// <heading>Updating a poll</heading>
            private void GetAndUpdatePoll()
            {
                // Gets the poll
                // In this case, the poll is global. The . prefix in the code name parameter specifies a global poll.
                PollInfo updatePoll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (updatePoll != null)
                {
                    // Updates the poll properties
                    updatePoll.PollDisplayName = updatePoll.PollDisplayName.ToLower();

                    // Saves the updated poll to the database
                    PollInfoProvider.SetPollInfo(updatePoll);
                }
            }


            /// <heading>Updating multiple polls</heading>
            private void GetAndBulkUpdatePolls()
            {
                // Gets all polls created for the current site
                // Only returns polls created directly for the given site. Does not include global polls that are made available on the site.
                var polls = PollInfoProvider.GetPolls().OnSite(SiteContext.CurrentSiteID);
                
                // Loops through individual polls
                foreach (PollInfo poll in polls)
                {
                    // Updates the poll properties
                    poll.PollDisplayName = poll.PollDisplayName.ToUpper();

                    // Saves the updated poll to the database
                    PollInfoProvider.SetPollInfo(poll);
                }                
            }


            /// <heading>Making a global poll available on a site</heading>
            private void AddPollToSite()
            {
                // Gets the global poll. The . prefix in the code name parameter specifies a global poll.
                PollInfo poll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (poll != null)
                {
                    // Assigns the global poll to the current site
                    PollSiteInfoProvider.AddPollToSite(poll.PollID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing a global poll from a site</heading>
            private void RemovePollFromSite()
            {
                // Gets the global poll. The . prefix in the code name parameter specifies a global poll.
                PollInfo poll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (poll != null)
                {
                    // Removes the global poll from the current site
                    PollSiteInfoProvider.RemovePollFromSite(poll.PollID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Deleting a poll</heading>
            private void DeletePoll()
            {
                // Gets the poll
                // In this case, the poll is global. The . prefix in the code name parameter specifies a global poll.
                PollInfo deletePoll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (deletePoll != null)
                {
                    // Deletes the poll
                    PollInfoProvider.DeletePollInfo(deletePoll);
                }
            }
        }


        /// <summary>
        /// Holds poll answer API examples.
        /// </summary>
        /// <groupHeading>Poll answers</groupHeading>
        private class PollAnswers
        {
            /// <heading>Adding answers to a poll</heading>
            private void CreateAnswer()
            {
                // Gets the poll
                // In this case, the poll is global. The . prefix in the code name parameter specifies a global poll.
                PollInfo poll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (poll != null)
                {
                    // Creates a new answer object
                    PollAnswerInfo newAnswer = new PollAnswerInfo();

                    // Sets the answer properties and assigns the answer to the specified poll
                    newAnswer.AnswerPollID = poll.PollID;
                    newAnswer.AnswerText = "My favorite color is blue.";
                    newAnswer.AnswerEnabled = true;
                    newAnswer.AnswerCount = 0;

                    // Saves the poll answer to the database
                    PollAnswerInfoProvider.SetPollAnswerInfo(newAnswer);
                }
            }            


            /// <heading>Updating poll answers</heading>
            private void GetAndBulkUpdateAnswers()
            {
                // Gets the poll
                // In this case, the poll is global. The . prefix in the code name parameter specifies a global poll.
                PollInfo poll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (poll != null)
                {
                    // Gets the poll's answers
                    var answers = PollAnswerInfoProvider.GetPollAnswers().WhereEquals("AnswerPollID", poll.PollID);
                    
                    // Loops through individual poll answers
                    foreach (var answer in answers)
                    {
                        // Updates the answer properties
                        answer.AnswerText = answer.AnswerText.ToUpper();

                        // Saves the updated poll answer to the database
                        PollAnswerInfoProvider.SetPollAnswerInfo(answer);
                    }
                }
            }


            /// <heading>Deleting poll answers</heading>
            private void DeleteAnswer()
            {
                // Gets the poll
                // In this case, the poll is global. The . prefix in the code name parameter specifies a global poll.
                PollInfo poll = PollInfoProvider.GetPollInfo(".ColorPoll", 0);

                if (poll != null)
                {
                    // Gets the poll's first answer
                    var deleteAnswer = PollAnswerInfoProvider.GetPollAnswers()
                                                             .WhereEquals("AnswerPollID", poll.PollID)
                                                             .TopN(1)
                                                             .FirstObject;

                    if (deleteAnswer != null)
                    {                    
                        // Deletes the poll answer
                        PollAnswerInfoProvider.DeletePollAnswerInfo(deleteAnswer);
                    }
                }
            }
        }
    }
}
