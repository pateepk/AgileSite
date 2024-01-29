using System;
using System.Collections.Generic;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.DataEngine;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactGroupedByAgeQueryBuilder
    {
        private readonly DateTime mCurrentDateTime;
        private readonly List<Tuple<int, int, string>> mAgeColumns = new List<Tuple<int, int, string>>();
        
        public Dictionary<AgeCategoryEnum, string> AgeColumnMapping
        {
            get;
            set;
        } = new Dictionary<AgeCategoryEnum, string>();


        public ContactGroupedByAgeQueryBuilder(DateTime currentDateTime)
        {
            mCurrentDateTime = currentDateTime;
        }


        public ContactGroupedByAgeQueryBuilder AddAgeColumn(int lowerBoundar, int upperBoundary, string columnName, AgeCategoryEnum ageCategory)
        {
            mAgeColumns.Add(new Tuple<int, int, string>(lowerBoundar, upperBoundary, columnName));
            AgeColumnMapping.Add(ageCategory, columnName);

            return this;
        }


        public ObjectQuery<ContactInfo> GetQuery(ObjectQuery<ContactInfo> originalQuery)
        {
            string unknownColumnName = "Unknown";

            AddAllSumColumns(originalQuery, unknownColumnName);
            RemoveAllColumnsColumn(originalQuery);

            return originalQuery;
        }


        private void RemoveAllColumnsColumn(ObjectQuery<ContactInfo> query)
        {
            for (int i = 0; i < query.SelectColumnsList.Count; i++)
            {
                var column = query.SelectColumnsList[i];
                if (column.Name.Equals(SqlHelper.COLUMNS_ALL, StringComparison.Ordinal))
                {
                    query.SelectColumnsList.Remove(column);
                    return;
                }
            }
        }


        private void AddAllSumColumns(ObjectQuery<ContactInfo> query, string unknownColumnName)
        {
            foreach (var ageColumn in mAgeColumns)
            {
                AddAgeSumColumn(query, ageColumn.Item1, ageColumn.Item2, ageColumn.Item3);
            }
            AddUnknownAgeSumColumn(query, unknownColumnName);
        }


        private void AddUnknownAgeSumColumn(ObjectQuery<ContactInfo> query, string columnName)
        {
            query.AddColumn(GetAgeSumColumn(new WhereCondition().WhereNull("ContactBirthday").ToString(true), columnName));
        }


        private void AddAgeSumColumn(ObjectQuery<ContactInfo> query, int lowerBound, int upperBound, string columnName)
        {
            query.AddColumn(GetAgeSumColumn(GetAgeSumWhereCondition(lowerBound, upperBound), columnName));
        }


        private IQueryColumn GetAgeSumColumn(string whereCondition, string columnName)
        {
            return new AggregatedColumn(AggregationType.Sum, $"CASE WHEN {whereCondition} THEN 1 ELSE 0 END").As(columnName);
        }


        private string GetAgeSumWhereCondition(int lowerBound, int upperBound)
        {
            return new WhereCondition().WhereLessOrEquals("ContactBirthday", mCurrentDateTime.AddYears(-lowerBound))
                                       .WhereGreaterThan("ContactBirthday", mCurrentDateTime.AddYears(-upperBound))
                                       .ToString(true);
        }
    }
}