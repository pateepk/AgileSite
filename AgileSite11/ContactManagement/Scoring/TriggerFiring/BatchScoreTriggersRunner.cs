using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Automation;
using CMS.Base;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    internal class BatchScoreTriggersRunner
    {
        private readonly List<int> mContactIds;
        private List<ContactWithScoreValue> mOldScoreValues;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contactIds">Collection of Contact IDs to be processed</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactIds"/> is null</exception>
        public BatchScoreTriggersRunner(ISet<int> contactIds)
        {
            if (contactIds == null)
            {
                throw new ArgumentNullException("contactIds");
            }

            mContactIds = contactIds.ToList();
        }


        /// <summary>
        /// This method has to be called before recalculation. 
        /// It saves current score points so triggers will fire only when they exceed limit after recalculation.
        /// </summary>
        public void SaveScoresSnapshot()
        {
            mOldScoreValues = GetContactScoreValues().ToList();
        }


        /// <summary>
        /// Compares current score points with score points saved before recalculation (by method <see cref="SaveScoresSnapshot"/>) 
        /// and prepares worker queue for triggers that could be affected by points change.
        /// </summary>
        /// <exception cref="InvalidOperationException">Method <see cref="SaveScoresSnapshot"/> wasn't called before recalculation</exception>
        public void ProcessTriggers()
        {
            if (mOldScoreValues == null)
            {
                throw new InvalidOperationException("Method SaveScoresSnapshot has to be called before checking for score changes");
            }

            var newScoreValues = GetContactScoreValues().ToLookup(s => s.ScoreID);

            var scoreIdsConnectedToTriggers = new HashSet<int>(ObjectWorkflowTriggerInfoProvider.GetObjectWorkflowTriggers()
                                                                                                .WhereEquals("TriggerObjectType", ScoreInfo.OBJECT_TYPE)
                                                                                                .Column("TriggerTargetObjectID")
                                                                                                .GetListResult<int>());

            foreach (var lookup in newScoreValues)
            {
                // Filter out scores that don't affect any trigger
                if (!scoreIdsConnectedToTriggers.Contains(lookup.Key))
                {
                    continue;
                }

                var score = ScoreInfoProvider.GetScoreInfo(lookup.Key);

                var options = lookup.Select((contactWithScoreValue, id) =>
                {
                    int oldScoreValue = 0;
                    var oldContactWithScoreValue = mOldScoreValues.SingleOrDefault(x => (x.ContactID == contactWithScoreValue.ContactID) && (x.ScoreID == score.ScoreID));
                    if (oldContactWithScoreValue != null)
                    {
                        oldScoreValue = oldContactWithScoreValue.ScoreValue;
                    }

                    return ScoreInfoProvider.CreateTriggerOptions(score, contactWithScoreValue.ContactID, contactWithScoreValue.ScoreValue, oldScoreValue);
                });

                TriggerHelper.ProcessTriggers(options);
            }
        }


        /// <summary>
        /// Gets contact with all his score values
        /// </summary>
        /// <returns>Contact and score values of all scores contact got points in</returns>
        private IEnumerable<ContactWithScoreValue> GetContactScoreValues()
        {
            var contactsWithScores = ScoreContactRuleInfoProvider.GetContactsWithScore()
                                                                 .WhereIn("ContactID", mContactIds);

            // Need to use datarows directly, since casting to ScoreContactRuleInfo deletes values from the properties
            // that don't fit
            if (!DataHelper.DataSourceIsEmpty(contactsWithScores))
            {
                foreach (DataRow dr in contactsWithScores.Tables[0].Rows)
                {
                    var scoreID = dr["ScoreID"].ToInteger(0);
                    var scoreValue = dr["Score"].ToInteger(0);
                    var contactID = dr["ContactID"].ToInteger(0);


                    yield return new ContactWithScoreValue
                                               {
                        ScoreID = scoreID,
                        ScoreValue = scoreValue,
                        ContactID = contactID,
                    };
                }
            }
        }
    }
}
