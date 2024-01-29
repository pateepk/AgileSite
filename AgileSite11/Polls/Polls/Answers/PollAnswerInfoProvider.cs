using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Polls
{
    /// <summary>
    /// Class providing PollAnswerInfo management.
    /// </summary>
    public class PollAnswerInfoProvider : AbstractInfoProvider<PollAnswerInfo, PollAnswerInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static PollAnswerInfo GetPollAnswerInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the PollAnswerInfo structure for the specified pollAnswer.
        /// </summary>
        /// <param name="pollAnswerId">PollAnswer ID</param>
        public static PollAnswerInfo GetPollAnswerInfo(int pollAnswerId)
        {
            return ProviderObject.GetInfoById(pollAnswerId);
        }


        /// <summary>
        /// Returns all poll answers.
        /// </summary>
        public static ObjectQuery<PollAnswerInfo> GetPollAnswers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified pollAnswer.
        /// </summary>
        /// <param name="pollAnswer">PollAnswer object to set</param>
        public static void SetPollAnswerInfo(PollAnswerInfo pollAnswer)
        {
            if (!String.IsNullOrEmpty(DataHelper.GetNotEmpty(RequestContext.CurrentDomain, String.Empty)))
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Polls);
            }

            ProviderObject.SetInfo(pollAnswer);
        }


        /// <summary>
        /// Deletes specified pollAnswer.
        /// </summary>
        /// <param name="pollAnswerObj">PollAnswer object</param>
        public static void DeletePollAnswerInfo(PollAnswerInfo pollAnswerObj)
        {
            ProviderObject.DeleteInfo(pollAnswerObj);
        }


        /// <summary>
        /// Deletes specified pollAnswer.
        /// </summary>
        /// <param name="pollAnswerId">PollAnswer ID</param>
        public static void DeletePollAnswerInfo(int pollAnswerId)
        {
            // Delete the object
            PollAnswerInfo pollAnswerObj = GetPollAnswerInfo(pollAnswerId);
            DeletePollAnswerInfo(pollAnswerObj);
        }


        /// <summary>
        /// Moves the answer up in order.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="answerId">Answer ID</param>
        public static void MoveAnswerUp(int pollId, int answerId)
        {
            ProviderObject.MoveAnswerUpInternal(pollId, answerId);
        }


        /// <summary>
        /// Moves the answer down in order.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="answerId">Answer ID</param>
        public static void MoveAnswerDown(int pollId, int answerId)
        {
            ProviderObject.MoveAnswerDownInternal(pollId, answerId);
        }


        /// <summary>
        /// Gets number which will be the highest AnswerOrder number of the specified Poll or 0.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static int GetLastAnswerOrder(int pollId)
        {
            if (pollId > 0)
            {
                var dummyInfo = new PollAnswerInfo()
                {
                    AnswerPollID = pollId,
                };
                int lastorder = dummyInfo.Generalized.GetLastObjectOrder();

                return lastorder;
            }

            return 0;
        }


        /// <summary>
        /// Increment AnswerCount of the specified answer.
        /// </summary>
        /// <param name="answerId">Answer ID</param>
        public static void Vote(int answerId)
        {
            ProviderObject.VoteInternal(answerId);
        }


        /// <summary>
        /// Sets AnswerCount to '0' to all answers of the specified poll.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static void ResetAnswers(int pollId)
        {
            ProviderObject.ResetAnswersInternal(pollId);
        }


        /// <summary>
        /// Delete all Poll's answers.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        public static void DeleteAnswers(int pollId)
        {
            ProviderObject.DeleteAnswersInternal(pollId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Moves the answer up in order.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="answerId">Answer ID</param>
        protected void MoveAnswerUpInternal(int pollId, int answerId)
        {
            if ((pollId > 0) && (answerId > 0))
            {
                PollAnswerInfo pollAnswerObj = GetPollAnswerInfo(answerId);
                if (pollAnswerObj != null)
                {
                    pollAnswerObj.Generalized.MoveObjectUp();
                }
            }
        }


        /// <summary>
        /// Moves the answer down in order.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="answerId">Answer ID</param>
        protected void MoveAnswerDownInternal(int pollId, int answerId)
        {
            if ((pollId > 0) && (answerId > 0))
            {
                PollAnswerInfo pollAnswerObj = GetPollAnswerInfo(answerId);
                if (pollAnswerObj != null)
                {
                    pollAnswerObj.Generalized.MoveObjectDown();
                }
            }
        }


        /// <summary>
        /// Increment AnswerCount of the specified answer.
        /// </summary>
        /// <param name="answerId">Answer ID</param>
        protected void VoteInternal(int answerId)
        {
            var answer = GetPollAnswerInfo(answerId);
            answer.AnswerCount++;

            SetPollAnswerInfo(answer);
        }


        /// <summary>
        /// Sets AnswerCount to '0' to all answers of the specified poll.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        protected void ResetAnswersInternal(int pollId)
        {
            var values = new Dictionary<string, object>
                {
                    {"AnswerCount", 0}
                };

            UpdateData(new WhereCondition().WhereEquals("AnswerPollID", pollId), values);
        }


        /// <summary>
        /// Delete all Poll's answers.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        protected void DeleteAnswersInternal(int pollId)
        {
            ProviderObject.BulkDelete(new WhereCondition().WhereEquals("AnswerPollID", pollId));
        }

        #endregion
    }
}