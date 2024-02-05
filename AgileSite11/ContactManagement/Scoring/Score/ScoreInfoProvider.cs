using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Automation;
using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Scheduler;
using CMS.WorkflowEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ScoreInfo management.
    /// </summary>
    public class ScoreInfoProvider : AbstractInfoProvider<ScoreInfo, ScoreInfoProvider>
    {
        private static readonly string SCORE_TASK_ASSEMBLY_NAME = typeof(ScoreEvaluator).Assembly.GetName().Name;

        private static readonly string SCORE_TASK_CLASS = typeof(ScoreEvaluator).FullName;
        

        #region "Constructor"

        /// <summary>
        /// Constructor using ID hashtable.
        /// </summary>
        public ScoreInfoProvider()
            : base(ScoreInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ScoreInfo objects.
        /// </summary>
        public static ObjectQuery<ScoreInfo> GetScores()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns score with specified ID.
        /// </summary>
        /// <param name="scoreId">Score ID</param>        
        public static ScoreInfo GetScoreInfo(int scoreId)
        {
            return ProviderObject.GetInfoById(scoreId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified score.
        /// </summary>
        /// <param name="scoreObj">Score to be set</param>
        public static void SetScoreInfo(ScoreInfo scoreObj)
        {
            ProviderObject.SetInfo(scoreObj);
        }


        /// <summary>
        /// Deletes specified score.
        /// </summary>
        /// <param name="scoreObj">Score to be deleted</param>
        public static void DeleteScoreInfo(ScoreInfo scoreObj)
        {
            ProviderObject.DeleteInfo(scoreObj);
        }


        /// <summary>
        /// Deletes score with specified ID.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        public static void DeleteScoreInfo(int scoreId)
        {
            ScoreInfo scoreObj = GetScoreInfo(scoreId);
            DeleteScoreInfo(scoreObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes scores specified by where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteAllScores(string where)
        {
            ProviderObject.DeleteAllScoresInternal(where);
        }


        /// <summary>
        /// Returns IDs and values of scores where specified contact exceeded score limit. Used to send notification emails.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <returns>Collection containing score, contact and values where the limit was exceeded.</returns>
        public static ContactWithScoreValueCollection GetScoresWhereContactExceededLimit(int contactId)
        {
            return ProviderObject.GetScoresWhereContactExceededLimitInternal(contactId);
        }


        /// <summary>
        /// Returns IDs and values of scores where specified contacts exceeded score limit. Used to send notification emails.
        /// </summary>
        /// <param name="contactIds">Contact IDs</param>
        /// <returns>Collection containing score, contact and values where the limit was exceeded.</returns>
        public static ContactWithScoreValueCollection GetScoresWhereContactsExceededLimit(IList<int> contactIds)
        {
            return ProviderObject.GetScoresWhereContactsExceededLimitInternal(contactIds);
        }


        /// <summary>
        /// Returns existing scheduled task for recalculation or creates new one based on the score info and other settings.
        /// </summary>
        /// <param name="score">Score info</param>
        /// <param name="scheduledInterval">Scheduled interval string for new task</param>
        /// <param name="nextRunTime">Next run time for new task</param>
        /// <param name="taskEnabled">Indicates if new task should be enabled</param>
        /// <param name="saveNewTask">Indicates if new task should be saved (inserted into DB)</param>
        public static TaskInfo EnsureScheduledTask(ScoreInfo score, string scheduledInterval, DateTime nextRunTime, bool taskEnabled, bool saveNewTask)
        {
            if (score == null)
            {
                return null;
            }

            if (score.ScoreScheduledTaskID > 0)
            {
                // Get existing task info or create new one
                return TaskInfoProvider.GetTaskInfo(score.ScoreScheduledTaskID) ?? GetNewScheduledTask(score, scheduledInterval, nextRunTime, taskEnabled, saveNewTask);
            }
            else
            {
                // Create new scheduled task
                return GetScheduledTask(score, scheduledInterval, nextRunTime, taskEnabled, saveNewTask);
            }
        }


        /// <summary>
        /// If appropriate scheduled task for Score exists then it is returned. Instead new 
        /// </summary>
        private static TaskInfo GetScheduledTask(ScoreInfo score, string scheduledInterval, DateTime nextRunTime, bool taskEnabled, bool saveNewTask)
        {
            TaskInfo ti = TaskInfoProvider.GetTaskInfo("ScoreRecalculation_" + score.ScoreName, null);

            return ti ?? GetNewScheduledTask(score, scheduledInterval, nextRunTime, taskEnabled, saveNewTask);
        }


        /// <summary>
        /// Deletes scheduled task of score with given ID.
        /// </summary>
        private static void DeleteScoreScheduledTask(int scoreID)
        {
            var oldScore = GetScoreInfo(scoreID);
            if (oldScore != null)
            {
                TaskInfoProvider.DeleteTaskInfo(oldScore.ScoreScheduledTaskID);
            }
        }


        /// <summary>
        /// Creates new scheduled task with basic properties set.
        /// </summary>
        /// <param name="score">Score info</param>
        /// <param name="scheduledInterval">Scheduled interval string</param>
        /// <param name="nextRunTime">Next run time</param>
        /// <param name="taskEnabled">Indicates if task should be enabled</param>
        /// <param name="saveNewTask">Indicates if new task should be saved (inserted into DB)</param>
        private static TaskInfo GetNewScheduledTask(ScoreInfo score, string scheduledInterval, DateTime nextRunTime, bool taskEnabled, bool saveNewTask)
        {
            TaskInfo result = new TaskInfo()
            {
                TaskAssemblyName = SCORE_TASK_ASSEMBLY_NAME,
                TaskClass = SCORE_TASK_CLASS,
                TaskEnabled = taskEnabled,
                TaskLastResult = string.Empty,
                TaskSiteID = 0,
                TaskData = score.ScoreID.ToString(),
                TaskDisplayName = "Score '" + score.ScoreDisplayName + "' recalculation",
                TaskName = "ScoreRecalculation_" + score.ScoreName,
                TaskType = ScheduledTaskTypeEnum.System,
                TaskInterval = scheduledInterval,
                TaskNextRunTime = nextRunTime
            };

            if (saveNewTask)
            {
                // Create new scheduled task
                TaskInfoProvider.SetTaskInfo(result);
            }

            return result;
        }


        /// <summary>
        /// Processes workflow triggers based on given score and contact ID.
        /// </summary>
        public static void ProcessTriggers(ScoreInfo score, int? contactID, DataSet oldScoreValues)
        {
            ProviderObject.ProcessTriggersInternal(score, contactID, oldScoreValues);
        }


        /// <summary>
        /// Marks score object as ready. This method does not log changes to the event log, staging log and export log.
        /// </summary>
        /// <param name="score">Score info object</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        public static void MarkScoreAsReady(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            ProviderObject.SetScoreStatusInternal(score, ScoreStatusEnum.Ready);
        }


        /// <summary>
        /// Marks score object as failed. This method does not log changes to the event log, staging log and export log.
        /// </summary>
        /// <param name="score">Score info object</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        public static void MarkScoreAsFailed(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            ProviderObject.SetScoreStatusInternal(score, ScoreStatusEnum.RecalculationFailed);
        }


        /// <summary>
        /// Marks score object as being recalculated. This method does not log changes to the event log, staging log and export log.
        /// </summary>
        /// <param name="score">Score info object</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        public static void MarkScoreAsRecalculating(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            ProviderObject.SetScoreStatusInternal(score, ScoreStatusEnum.Recalculating);
        }


        /// <summary>
        /// Marks score object as recalculation required. This method does not log changes to the event log, staging log and export log.
        /// </summary>
        /// <param name="score">Score info object</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        public static void MarkScoreAsRecalculationRequired(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            ProviderObject.SetScoreStatusInternal(score, ScoreStatusEnum.RecalculationRequired);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ScoreInfo info)
        {
            if (info.ScoreScheduledTaskID == 0)
            {
                DeleteScoreScheduledTask(info.ScoreID);
            }
            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Deletes scores specified by where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteAllScoresInternal(string where)
        {
            var whereCondition = new WhereCondition(where);

            // We need to define object type explicitly to delete all object types
            BulkDelete(whereCondition, new BulkDeleteSettings { ObjectType = ScoreInfo.OBJECT_TYPE });
            BulkDelete(whereCondition, new BulkDeleteSettings { ObjectType = ScoreInfo.OBJECT_TYPE_PERSONA });
        }


        /// <summary>
        /// Returns IDs and values of scores where specified contact exceeded score limit. Used to send notification emails.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <returns>Collection containing score, contact and values where the limit was exceeded.</returns>
        protected virtual ContactWithScoreValueCollection GetScoresWhereContactExceededLimitInternal(int contactId)
        {
            return GetScoresWhereContactsExceededLimitInternal(new[]
            {
                contactId
            });
        }


        /// <summary>
        /// Returns IDs and values of scores where specified contacts exceeded score limit. Used to send notification emails.
        /// </summary>
        /// <param name="contactIds">Contact IDs</param>
        /// <returns>Collection containing score, contact and values where the limit was exceeded.</returns>
        protected virtual ContactWithScoreValueCollection GetScoresWhereContactsExceededLimitInternal(IList<int> contactIds)
        {
            var query = ScoreContactRuleInfoProvider.GetScoreContactRules()
                                                    .Columns("OM_Score.ScoreID", "ContactID")
                                                    .Source(s => s.Join<ScoreInfo>("ScoreID", "ScoreID"))
                                                    .AddColumn(new AggregatedColumn(AggregationType.Sum, "Value").As("Score"))
                                                    .Where(w => w.WhereNull("Expiration")
                                                                 .Or()
                                                                 .Where("Expiration", QueryOperator.LargerThan, DateTime.Now))
                                                    .WhereIn("ContactID", contactIds)
                                                    .GroupBy("OM_Score.ScoreID", "ContactID", "ScoreEmailAtScore")
                                                    .Having(w => w.Where(new AggregatedColumn(AggregationType.Sum, "Value"), QueryOperator.GreaterOrEquals, "ScoreEmailAtScore".AsColumn()));

            var scoreIDsWithValues = query.Select(row => new ContactWithScoreValue
            {
                ScoreID = DataHelper.GetIntValue(row, "ScoreID"),
                ScoreValue = DataHelper.GetIntValue(row, "Score"),
                ContactID = DataHelper.GetIntValue(row, "ContactID"),
            });

            return new ContactWithScoreValueCollection(scoreIDsWithValues);
        }


        /// <summary>
        /// Processes triggers based on given score and contact ID.
        /// </summary>
        protected virtual void ProcessTriggersInternal(ScoreInfo score, int? contactID, DataSet oldScoreValues)
        {
            if (!TriggerHelper.HasTriggerTypes(ScoreInfo.OBJECT_TYPE))
            {
                return;
            }

            // Create a hash table with dataset values for easier lookup later.
            var oldScoreTable = CreateHashtableFromScoreValues(oldScoreValues);

            // Create where condition
            WhereCondition condition = new WhereCondition().WhereEquals("ScoreID", score.ScoreID);
            if (contactID.HasValue)
            {
                condition.WhereEquals("ContactID", contactID.Value);
            }

            var contactsWithScores = ScoreContactRuleInfoProvider.GetContactsWithScore()
                                        .Where(condition)
                                        .Select(row => new {
                                            ContactID = (int)row["ContactID"],
                                            ScoreID = (int)row["ScoreID"],
                                            Score = (int)row["Score"]
                                        });

            ScoreQueueWorker.Current.Enqueue(TextHelper.Merge("|", "AutomationTriggerWorker", ScoreInfo.OBJECT_TYPE, WorkflowTriggerTypeEnum.Change.ToString(), condition.ToString(true)),
                () => {
                        var options = contactsWithScores.Select(contactScore =>
                        {
                            var oldScore = 0;
                            var contactTable = oldScoreTable[contactScore.ScoreID];
                            if (contactTable != null)
                            {
                                oldScore = contactTable[contactScore.ContactID];
                            }

                            if (oldScore == contactScore.Score)
                            {
                                return null;
                            }

                            return CreateTriggerOptions(score, contactScore.ContactID, contactScore.Score, oldScore);

                        }).Where(o => o != null);

                        TriggerHelper.ProcessTriggers(options);
                    });
        }


        /// <summary>
        /// Creates trigger options based on given parameters
        /// </summary>
        /// <param name="score">Score</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="newScore">New score value</param>
        /// <param name="oldScore">Old score value</param>
        internal static TriggerOptions CreateTriggerOptions(ScoreInfo score, int contactId, int newScore, int oldScore)
        {
            // Create trigger worker to run trigger processing asynchronously
            var contact = ContactInfoProvider.GetContactInfo(contactId);

            var resolver = MacroResolver.GetInstance();
            resolver.SetNamedSourceData("Contact", contact);
            resolver.SetNamedSourceData("Score", score);

            return new TriggerOptions
            {
                Resolver = resolver,
                Info = contact,
                ObjectType = ScoreInfo.OBJECT_TYPE,
                Types = new List<WorkflowTriggerTypeEnum>
                {
                    WorkflowTriggerTypeEnum.Change
                },
                PassFunction = info =>
                {
                    // Need to get score value from info
                    if (score.ScoreID != info.TriggerTargetObjectID)
                    {
                        return false;
                    }

                    // Get Value
                    var triggerScoreValue = info.TriggerParameters["ScoreValue"];

                    return (ValidationHelper.GetInteger(triggerScoreValue, int.MinValue) <= newScore) && (oldScore < ValidationHelper.GetInteger(triggerScoreValue, int.MaxValue));
                }
            };
        }


        /// <summary>
        /// Creates hash table from dataset of contact ID - score ID - score value.
        /// </summary>
        private static SafeDictionary<int, SafeDictionary<int, int>> CreateHashtableFromScoreValues(DataSet oldScoreValues)
        {
            var oldScoreTable = new SafeDictionary<int, SafeDictionary<int, int>>();
            if (oldScoreValues?.Tables[0] != null)
            {
                foreach (DataRow row in oldScoreValues.Tables[0].Rows)
                {
                    int rowContactID = (int)row["ContactID"];
                    int rowScore = (int)row["Score"];
                    int rowScoreID = (int)row["ScoreID"];

                    SafeDictionary<int, int> dict;
                    if (oldScoreTable.TryGetValue(rowScoreID, out dict))
                    {
                        dict.Add(rowContactID, rowScore);
                    }
                    else
                    {
                        var newDict = new SafeDictionary<int, int> { { rowContactID, rowScore } };
                        oldScoreTable.Add(rowScoreID, newDict);
                    }
                }
            }
            return oldScoreTable;
        }


        /// <summary>
        /// Changes status of the Score and sets it immediately to the database. This method does not log changes to the event log, staging log and export log.
        /// </summary>
        protected virtual void SetScoreStatusInternal(ScoreInfo score, ScoreStatusEnum scoreStatus)
        {
            using (var context = new CMSActionContext())
            {
                context.DisableLogging();

                score.ScoreStatus = scoreStatus;
                SetScoreInfo(score);
            }
        }

        #endregion
    }
}