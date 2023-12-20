using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Checker for conflicts between page URLs and alternative URLs.
    /// </summary>
    internal sealed class AlternativeUrlConflictChecker
    {
        private readonly ICollection<ConflictCheckerItem> items;
        private readonly int siteId;
        private readonly StringBuilder stringBuilder;


        /// <summary>
        /// Checks for conflicts between page URLs and alternative URLs.
        /// </summary>
        /// <paramref name="nodes">Documents to check conflicts for.</paramref>
        /// <paramref name="siteId">Conflicts are checked against Alternative URLs on this site.</paramref>
        /// <paramref name="stringBuilder">String builder where the discovered conflict warnings should be appended.</paramref>
        public AlternativeUrlConflictChecker(IEnumerable<TreeNode> nodes, int siteId, StringBuilder stringBuilder)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (stringBuilder == null)
            {
                throw new ArgumentNullException(nameof(stringBuilder));
            }

            this.stringBuilder = stringBuilder;
            items = nodes.Select(x => new ConflictCheckerItem(x)).ToList();
            this.siteId = siteId;         
        }


        /// <summary>
        /// Checks for conflicts between page URLs and alternative URLs and appends them to the <see cref="stringBuilder"/>.
        /// </summary>
        public void CheckConflicts()
        {
            var urlsToCheck = items.Select(x => x.Url.NormalizedUrl).ToList();

            var query = new ObjectQuery<AlternativeUrlInfo>()
                        .OnSite(siteId)
                        .Source(i => i.Join<DocumentCultureDataInfo>("AlternativeUrlDocumentID", "DocumentID", JoinTypeEnum.Inner,
                            new WhereCondition().WhereIn("AlternativeUrlUrl", urlsToCheck)))
                        .Columns("DocumentCulture", "DocumentNamePath", "AlternativeUrlUrl");

            var result = query.Result;

            if (!DataHelper.DataSourceIsEmpty(result))
            {
                foreach (DataRow dataRow in result.Tables[0].Rows)
                {
                    var altUrl = DataHelper.GetStringValue(dataRow, "AlternativeUrlUrl", null);
                    var documentNamePath = DataHelper.GetStringValue(dataRow, "DocumentNamePath");
                    var documentCulture = DataHelper.GetStringValue(dataRow, "DocumentCulture");

                    foreach (var node in items.Where(x => String.Equals(x.Url.NormalizedUrl, altUrl, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Node))
                    {
                        stringBuilder.AppendLine($"The URL of page {AlternativeUrlHelper.GetDocumentIdentification(node.DocumentNamePath, node.DocumentCulture)} is in conflict with alternative URL '{altUrl}' of page {AlternativeUrlHelper.GetDocumentIdentification(documentNamePath, documentCulture)}.");
                        stringBuilder.AppendLine();
                    }
                }
            }
        }


        private class ConflictCheckerItem
        {
            public ConflictCheckerItem(TreeNode node)
            {
                Node = node ?? throw new ArgumentNullException(nameof(node));
                Url = AlternativeUrlHelper.NormalizeAlternativeUrl(DocumentURLProvider.GetUrl(node));
            }


            public TreeNode Node
            {
                get;
            }


            public NormalizedAlternativeUrl Url
            {
                get;
            }
        }
    }
}
