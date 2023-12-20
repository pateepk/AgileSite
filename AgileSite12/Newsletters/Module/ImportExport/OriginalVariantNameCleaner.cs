using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Ensures that original issue variant has not set a name.
    /// </summary>
    internal class OriginalVariantNameCleaner
    {
        private readonly DataTable issues;


        /// <summary>
        /// Creates an instance of the <see cref="OriginalVariantNameCleaner"/> class.
        /// </summary>
        /// <param name="issues">Issues which need to clear the name of original variant.</param>
        public OriginalVariantNameCleaner(DataTable issues)
        {
            this.issues = issues;
        }


        /// <summary>
        /// Clears the name for issue variant representing the original variant.
        /// </summary>
        public void Clear()
        {
            if (DataHelper.DataSourceIsEmpty(issues))
            {
                return;
            }

            var variantsGroup = GetVariantsGroups();
            foreach (var variants in variantsGroup)
            {
                if (ExistsVariantWithoutName(variants))
                {
                    continue;
                }

                var variantNamedOriginal = GetVariantWithOriginalName(variants);
                if (variantNamedOriginal != null)
                {
                    variantNamedOriginal["IssueVariantName"] = DBNull.Value;
                    continue;
                }

                var first = GetFirstVariantWithNameSet(variants);
                if (first != null)
                {
                    first["IssueVariantName"] = DBNull.Value;
                }
            }
        }


        private IEnumerable<IGrouping<int, DataRow>> GetVariantsGroups()
        {
            return issues.AsEnumerable()
                         .Where(row => row.Field<bool?>("IssueIsABTest") ?? false)
                         .Where(row => row.Field<int?>("IssueVariantOfIssueID") != null)
                         .GroupBy(row => row.Field<int>("IssueVariantOfIssueID"), row => row);
        }


        private static DataRow GetFirstVariantWithNameSet(IGrouping<int, DataRow> variants)
        {
            return variants.FirstOrDefault(row => !string.IsNullOrEmpty(DataHelper.GetStringValue(row, "IssueVariantName")));
        }


        private static DataRow GetVariantWithOriginalName(IGrouping<int, DataRow> variants)
        {
            return variants.FirstOrDefault(variant => DataHelper.GetStringValue(variant, "IssueVariantName").Equals("Original", StringComparison.InvariantCulture));
        }


        private static bool ExistsVariantWithoutName(IGrouping<int, DataRow> variants)
        {
            return variants.Any(variant => string.IsNullOrEmpty(DataHelper.GetStringValue(variant, "IssueVariantName")));
        }
    }
}
