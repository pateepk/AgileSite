using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CMS.Activities;
using CMS.Base;

namespace CMS.Polls
{
    /// <summary>
    /// Provides initialization for poll voting activity.
    /// </summary>
    public class PollVotingActivityInitializer : IActivityInitializer
    {
        private readonly PollInfo mPoll;
        private readonly IEnumerable<int> mSelectedAnswersIDs;
        private readonly ITreeNode mCurrentDocument;
        private readonly ActivityTitleBuilder mActivityTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Instantiates new instance of <see cref="PollVotingActivityInitializer"/>.
        /// </summary>
        /// <param name="poll">Reference to poll the activity will be initialized for</param>
        /// <param name="selectedAnswersIDs"></param>
        /// <param name="currentDocument">Specifies document the activity is logged for</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="poll"/> is <c>null</c> -or- 
        /// <paramref name="selectedAnswersIDs"/> is <c>null</c> -or- 
        /// <paramref name="currentDocument"/> is <c>null</c>
        /// </exception>
        public PollVotingActivityInitializer(PollInfo poll, IEnumerable<int> selectedAnswersIDs, ITreeNode currentDocument)
        {
            if (poll == null)
            {
                throw new ArgumentNullException("poll");
            }
            if (selectedAnswersIDs == null)
            {
                throw new ArgumentNullException("selectedAnswersIDs");
            }
            if (currentDocument == null)
            {
                throw new ArgumentNullException("currentDocument");
            }

            mPoll = poll;
            mSelectedAnswersIDs = selectedAnswersIDs;
            mCurrentDocument = currentDocument;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c></exception>
        public void Initialize(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            activity.ActivityTitle = mActivityTitleBuilder.CreateTitle(ActivityType, mPoll.PollQuestion);
            activity.ActivityItemID = mPoll.PollID;
            // Wrap the joined answers with pipes to make sure translator will only matches the correct numbers 
            // (e.g. 1 would be contained withing 121, while |1| not)
            activity.ActivityValue = string.Format("|{0}|", string.Join("|", mSelectedAnswersIDs.Select(answerID => answerID.ToString(CultureInfo.InvariantCulture))));
            activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            activity.ActivityNodeID = mCurrentDocument.NodeID;
        }
        

        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.POLL_VOTING;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMPollVoting";
            }
        }
    }
}