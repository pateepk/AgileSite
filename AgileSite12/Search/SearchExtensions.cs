using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Extension methods for the search module
    /// </summary>
    public static class SearchExtensions
    {
        /// <summary>
        /// Returns true if any field included in the search changed (checks fields defined in Class Search Settings).
        /// </summary>
        public static bool SearchFieldChanged(this DataClassInfo dc, List<string> changedColumns, bool checkSpecialFields = true)
        {
            // Check whether the columns which are part of search changed
            if (dc.ClassSearchSettingsInfos.SearchesAnyOf(changedColumns))
            {
                return true;
            }

            // Check changed columns for class special columns
            var changed = new HashSet<string>();
            foreach (var changedColumn in changedColumns)
            {
                changed.Add(changedColumn.ToLowerCSafe());
            }
            
            // Check special fields
            if (checkSpecialFields)
            {
                return 
                    changed.Contains(dc.ClassSearchImageColumn.ToLowerCSafe()) || 
                    changed.Contains(dc.ClassSearchTitleColumn.ToLowerCSafe()) ||
                    changed.Contains(dc.ClassSearchContentColumn.ToLowerCSafe()) || 
                    changed.Contains(dc.ClassSearchCreationDateColumn.ToLowerCSafe());
            }

            return false;
        }
    }
}
