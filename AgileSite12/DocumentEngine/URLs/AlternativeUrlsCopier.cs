using System;
using System.Collections.Generic;
using System.Text;

using CMS.Core;
using CMS.EventLog;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides methods to copy alternative URLs between 2 documents.
    /// </summary>
    /// <remarks>
    /// Copier does copy alternative URLs only when both documents belong to different content only site.
    /// </remarks>
    internal sealed class AlternativeUrlsCopier
    {
        private readonly TreeNode sourceDocument;
        private readonly TreeNode targetDocument;
        private readonly IEventLogService eventLogService;


        /// <summary>
        /// Initialize a new instance of <see cref="AlternativeUrlsCopier"/>.
        /// </summary>
        /// <param name="sourceDocument">Source document.</param>
        /// <param name="targetDocument">Target document.</param>
        /// <exception cref="ArgumentNullException">When any of given document is null.</exception>
        public AlternativeUrlsCopier(TreeNode sourceDocument, TreeNode targetDocument)
            : this(sourceDocument, targetDocument, Service.Resolve<IEventLogService>())
        {
        }


        /// <summary>
        /// Initialize a new instance of <see cref="AlternativeUrlsCopier"/>.
        /// </summary>
        public AlternativeUrlsCopier(TreeNode sourceDocument, TreeNode targetDocument, IEventLogService eventLogService)
        {
            this.sourceDocument = sourceDocument ?? throw new ArgumentNullException(nameof(sourceDocument));
            this.targetDocument = targetDocument ?? throw new ArgumentNullException(nameof(targetDocument));
            this.eventLogService = eventLogService;
        }


        /// <summary>
        /// Method attempts to copy all alternative URLs belonging to source document to target document.
        /// </summary>
        /// <remarks>
        /// In case that copied alternative URL doesn't pass the validation, event is logged and processing continues by next alternative URL.
        /// No other exceptions are handled.
        /// </remarks>
        public void Copy()
        {
            if (sourceDocument.NodeSiteID == targetDocument.NodeSiteID)
            {
                return;
            }

            if (!sourceDocument.Site.SiteIsContentOnly)
            {
                return;
            }

            if (!targetDocument.Site.SiteIsContentOnly)
            {
                return;
            }

            var existingOnSourceSite = AlternativeUrlInfoProvider.GetAlternativeUrls()
                                                                 .WhereEquals("AlternativeUrlDocumentID", sourceDocument.DocumentID);

            CopyAlternativeUrls(existingOnSourceSite);
        }


        private void CopyAlternativeUrls(IEnumerable<AlternativeUrlInfo> alternativeUrlsToCopy)
        {
            var builder = new StringBuilder();
            var exceptionOccured = false;

            foreach (var alternativeUrl in alternativeUrlsToCopy)
            {
                try
                {
                    var copiedAlternativeUrl = alternativeUrl.Clone(true);
                    copiedAlternativeUrl.AlternativeUrlDocumentID = targetDocument.DocumentID;
                    copiedAlternativeUrl.AlternativeUrlSiteID = targetDocument.NodeSiteID;

                    copiedAlternativeUrl.Insert();
                }
                catch (InvalidAlternativeUrlException exception)
                {
                    exceptionOccured = true;
                    builder.AppendLine(exception.Message);
                }
            }

            if (exceptionOccured)
            {
                eventLogService.LogEvent(EventType.ERROR, "Content", "COPYALTERNATIVEURL", builder.ToString());
            }
        }
    }
}
