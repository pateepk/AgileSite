using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class representing codename filter for continuous integration.
    /// </summary>
    public class CodenameFilter
    {
        private IWhereCondition mWhereCondition;

        private readonly string mFilterColumn;
        private readonly List<string> mPrefixes;
        private readonly List<string> mSuffixes;
        private readonly List<string> mCodenames;


        /// <summary>
        /// Gets a Where condition which filters objects by codename.
        /// </summary>
        public IWhereCondition WhereCondition
        {
            get
            {
                return mWhereCondition ?? (mWhereCondition = CreateWhereCondition());
            }
        }


        /// <summary>
        /// Creates new instance of Codename filter.
        /// </summary>
        /// <param name="filterColumn">Column name, which is filtered by codename.</param>
        /// <param name="prefixes">Prefixes of filtered codenames.</param>
        /// <param name="suffixes">Suffixes of filtered codenames.</param>
        /// <param name="codenames">Filtered codenames.</param>
        public CodenameFilter(string filterColumn, List<string> prefixes, List<string> suffixes, List<string> codenames)
        {
            mFilterColumn = filterColumn;
            mPrefixes = prefixes;
            mSuffixes = suffixes;
            mCodenames = codenames;
        }


        /// <summary>
        /// Returns true, if object meets the codename filter.
        /// </summary>
        /// <param name="baseInfo">Base info.</param>
        public bool IsObjectIncluded(BaseInfo baseInfo)
        {
            var codeName = baseInfo.GetStringValue(mFilterColumn, "");

            bool isIncluded = true;

            for (var i = 0; i < mPrefixes.Count && isIncluded; i++)
            {
                isIncluded &= !codeName.StartsWith(mPrefixes[i], StringComparison.InvariantCultureIgnoreCase);
            }

            for (var i = 0; i < mSuffixes.Count && isIncluded; i++)
            {
                isIncluded &= !codeName.EndsWith(mSuffixes[i], StringComparison.InvariantCultureIgnoreCase);
            }

            for (var i = 0; i < mCodenames.Count && isIncluded; i++)
            {
                isIncluded &= !codeName.Equals(mCodenames[i], StringComparison.InvariantCultureIgnoreCase);
            }

            return isIncluded;
        }


        /// <summary>
        /// Creates a Where condition, which represents this filter.
        /// </summary>
        private IWhereCondition CreateWhereCondition()
        {
            var condition = new WhereCondition();

            if (mCodenames.Any())
            {
                condition = condition.WhereNotIn(mFilterColumn, mCodenames);
            }

            foreach (var prefix in mPrefixes)
            {
                condition = condition.WhereNotStartsWith(mFilterColumn, prefix);
            }

            foreach (var suffix in mSuffixes)
            {
                condition = condition.WhereNotEndsWith(mFilterColumn, suffix);
            }

            return condition;
        }
    }
}