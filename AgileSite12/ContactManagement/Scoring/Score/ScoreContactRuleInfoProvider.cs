using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Core.Internal;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ScoreContactRuleInfo management.
    /// </summary>
    public class ScoreContactRuleInfoProvider : AbstractInfoProvider<ScoreContactRuleInfo, ScoreContactRuleInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ScoreContactRuleInfo objects.
        /// </summary>
        public static ObjectQuery<ScoreContactRuleInfo> GetScoreContactRules()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ScoreContactRuleInfo with specified IDs.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="ruleId">Rule ID</param>
        public static ScoreContactRuleInfo GetScoreContactRuleInfo(int scoreId, int contactId, int ruleId)
        {
            return ProviderObject.GetScoreContactRuleInfoInternal(scoreId, contactId, ruleId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ScoreContactRuleInfo.
        /// </summary>
        /// <param name="infoObj">ScoreContactRuleInfo to be set.</param>
        public static void SetScoreContactRuleInfo(ScoreContactRuleInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ScoreContactRuleInfo.
        /// </summary>
        /// <param name="infoObj">ScoreContactRuleInfo to be deleted.</param>
        public static void DeleteScoreContactRuleInfo(ScoreContactRuleInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ScoreContactRuleInfo with specified IDs.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="ruleId">Rule ID</param>
        public static void DeleteScoreContactRuleInfo(int scoreId, int contactId, int ruleId)
        {
            ScoreContactRuleInfo infoObj = GetScoreContactRuleInfo(scoreId, contactId, ruleId);
            DeleteScoreContactRuleInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes score-contact-rule relation.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="ruleId">Rule ID</param>
        public static void DeleteScoreContactRule(int? scoreId, int? contactId, int? ruleId)
        {
            ProviderObject.DeleteScoreContactRuleInternal(scoreId, contactId, ruleId);
        }


        /// <summary>
        /// Deletes points for the given contacts and rule.
        /// </summary>
        /// <param name="ruleID">Id of the scoring rule. Points for this rule will be cleared</param>
        /// <param name="contactIds">Points of this contacts for rule <paramref name="ruleID"/> will be deleted</param>
        internal static void DeleteScoreContactRules(int ruleID, IEnumerable<int> contactIds)
        {
            if (contactIds == null)
            {
                throw new ArgumentNullException("contactIds");
            }

            var deleteQuery = CreateDeleteQuery(ruleID, contactIds);
            if (deleteQuery != null)
            {
                deleteQuery.Execute();
            }
        }


        private static DataQuery CreateDeleteQuery(int ruleID, IEnumerable<int> contactIds)
        {
            var contactIdsList = contactIds.ToList();
            if (contactIdsList.Count == 0)
            {
                return null;
            }

            var contactIDsIntTable = SqlHelper.BuildOrderedIntTable(contactIdsList);

            var deleteQuery = new DataQuery
            {
                ConnectionStringName = DatabaseSeparationHelper.OM_CONNECTION_STRING,
                CustomQueryText = "DELETE OM_ScoreContactRule FROM ##SOURCE## WHERE ##WHERE##"
            };

            deleteQuery
                .From(
                    new QuerySource("OM_ScoreContactRule")
                        .Join("@ContactIDs", "ContactID", "Value")
                )
                .WhereEquals("RuleID", ruleID);

            deleteQuery.Parameters.Add("@ContactIDs", contactIDsIntTable, SqlHelper.OrderedIntegerTableType);

            return deleteQuery;
        }


        /// <summary>
        /// Returns IDs of contacts with score values who gained more than minimum score value.
        /// </summary>
        /// <param name="minScoreValue">Minimum score value</param>
        public static ObjectQuery<ScoreContactRuleInfo> GetContactsWithScore(int minScoreValue)
        {
            return ProviderObject.GetContactsWithScoreInternal().Having("SUM(Value) >= " + minScoreValue);
        }


        /// <summary>
        /// Returns IDs of contacts with score values.
        /// </summary>
        public static ObjectQuery<ScoreContactRuleInfo> GetContactsWithScore()
        {
            return ProviderObject.GetContactsWithScoreInternal();
        }


        /// <summary>
        /// Returns score value of specified contact.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <returns>Score value of contact</returns>
        public static int GetContactScore(int scoreId, int contactId)
        {
            return ProviderObject.GetContactScoreInternal(scoreId, contactId);
        }


        /// <summary>
        /// Recalculates rules that are internally specified by where condition (attribute and activity rule types). Contact's points affected by recalculated rule has to be cleared before calling this method.
        /// </summary>
        /// <param name="ri">RuleInfo with specified rule</param>
        /// <param name="contactId">Contacts which will be recalculated. If null, all contacts will be recalculated</param>
        internal static void RecalculateRuleWithWhereCondition(RuleInfo ri, int contactId)
        {
            RecalculateRuleWithWhereCondition(ri, new[] { contactId });
        }


        /// <summary>
        /// Recalculates rules that are internally specified by where condition (attribute and activity rule types). Contact's points affected by recalculated rule has to be cleared before calling this method.
        /// </summary>
        /// <param name="ri">RuleInfo with specified rule</param>
        /// <param name="contactIDs">Contacts which will be recalculated. If null, all contacts will be recalculated</param>
        internal static void RecalculateRuleWithWhereCondition(RuleInfo ri, IEnumerable<int> contactIDs = null)
        {
            // Get WHERE condition and run recalculation
            string whereCond = ri.RuleWhereCondition;
            if (String.IsNullOrEmpty(whereCond))
            {
                return;
            }

            var parameters = new QueryDataParameters
            {
                { "@RuleID", ri.RuleID },
                { "@WhereCond", whereCond },
                { "@RuleValidUnits", (int)ri.RuleValidity },
                { "@RuleValidFor", ri.RuleValidFor }
            };

            if (contactIDs != null)
            {
                var contactIDsList = contactIDs.ToList();
                if (contactIDsList.Count > 0)
                {
                    var contactIDsIntTable = SqlHelper.BuildOrderedIntTable(contactIDsList);
                    parameters.Add("@ContactIDs", contactIDsIntTable, SqlHelper.OrderedIntegerTableType);
                }
            }

            if ((ri.RuleValidity != ValidityEnum.Until) || (ri.RuleValidUntil == DateTimeHelper.ZERO_TIME))
            {
                parameters.Add("@RuleExpiration", null);
            }
            else
            {
                parameters.Add("@RuleExpiration", ri.RuleValidUntil);
            }

            ConnectionHelper.ExecuteQuery("om.score.recalculate", parameters);
        }


        /// <summary>
        /// Adds score contact rules to the database. All bindings must be cleared before calling this procedure.
        /// </summary>
        /// <param name="rule">Rule for which the contacts will get <paramref name="value"/> points.</param>
        /// <param name="contactIDs">Contacts to whom <paramref name="value"/> points will be added for <paramref name="rule"/> score rule</param>
        /// <param name="value">Score value that will be set to the contacts</param>
        internal static void AddScoreContactRules(RuleInfo rule, IEnumerable<int> contactIDs, int value)
        {
            foreach (var contactIDsBatch in contactIDs.Batch(10000))
            {
                var contactsTable = SqlHelper.BuildOrderedIntTable(contactIDsBatch);
                var parameters = new QueryDataParameters();
                parameters.Add("@RuleID", rule.RuleID);
                parameters.Add("@ScoreID", rule.RuleScoreID);
                parameters.Add("@Value", value);
                parameters.Add("@ContactIDs", contactsTable, SqlHelper.OrderedIntegerTableType);
                ConnectionHelper.ExecuteQuery("om.scorecontactrule.addcontacts", parameters);
            }
        }


        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns ScoreContactRuleInfo with specified IDs.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="ruleId">Rule ID</param>
        protected virtual ScoreContactRuleInfo GetScoreContactRuleInfoInternal(int scoreId, int contactId, int ruleId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("ScoreID", scoreId)
                .WhereEquals("ContactID", contactId)
                .WhereEquals("RuleID", ruleId)
                .FirstOrDefault();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Deletes score-contact-rule relation.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="ruleId">Rule ID</param>
        protected virtual void DeleteScoreContactRuleInternal(int? scoreId, int? contactId, int? ruleId)
        {
            var where = new WhereCondition();

            // Append score ID to condition
            if (scoreId != null)
            {
                where.WhereEquals("ScoreID", scoreId);
            }

            // Append contact ID to condition
            if (contactId != null)
            {
                where.WhereEquals("ContactID", contactId);
            }

            // Append rule ID to condition
            if (ruleId != null)
            {
                where.WhereEquals("RuleID", ruleId);
            }

            BulkDelete(where);
        }


        /// <summary>
        /// Returns contact IDs, score IDs with their score values.
        /// </summary>
        protected virtual ObjectQuery<ScoreContactRuleInfo> GetContactsWithScoreInternal()
        {
			var dateTimeService = Service.Resolve<IDateTimeNowService>();

			return GetObjectQuery()
				.Distinct()
				.Columns("ContactID", "ScoreID")
				.AddColumn(
					new AggregatedColumn(AggregationType.Sum, "Value").As("Score")
				)
				.WhereNotExpired(dateTimeService.GetDateTimeNow())
				.GroupBy("ContactID", "ScoreID");
		}


        /// <summary>
        /// Returns score value of specified contact.
        /// </summary>
        /// <param name="scoreId">Score ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <returns>Score value of contact</returns>
        protected virtual int GetContactScoreInternal(int scoreId, int contactId)
        {
            var objectQuery = GetContactsWithScore().WhereEquals("ScoreID", scoreId)
                                                    .WhereEquals("ContactID", contactId);
            
            return objectQuery
                .Select(row => DataHelper.GetIntValue(row, "Score"))
                .SingleOrDefault();
        }

        #endregion
    }
}