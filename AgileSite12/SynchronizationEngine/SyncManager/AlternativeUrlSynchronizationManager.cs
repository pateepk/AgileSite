using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Support for synchronization of alternative url objects.
    /// </summary>
    internal class AlternativeUrlSynchronizationManager
    {
        private readonly ISyncManager mSyncManager;
        private readonly TreeNode mTreeNode;
        private readonly TranslationHelper mTranslationHelper;


        /// <summary>
        /// Initializes a new instance of <see cref="AlternativeUrlSynchronizationManager"/>.
        /// </summary>
        /// <param name="syncManager">Implementation of <see cref="ISyncManager"/>.</param>
        /// <param name="treeNode">Synchronized document, parent of alternative urls.</param>
        /// <param name="translationHelper">Translation helper used in current synchronization.</param>
        public AlternativeUrlSynchronizationManager(ISyncManager syncManager, TreeNode treeNode, TranslationHelper translationHelper)
        {
            mSyncManager = syncManager ?? throw new ArgumentNullException(nameof(syncManager));
            mTreeNode = treeNode ?? throw new ArgumentNullException(nameof(treeNode));
            mTranslationHelper = translationHelper ?? throw new ArgumentNullException(nameof(translationHelper));
        }


        /// <summary>
        /// Runs synchronization for given <paramref name="alternativeUrls"/>.
        /// </summary>
        public void Synchronize(ICollection<AlternativeUrlInfo> alternativeUrls)
        {
            if (!alternativeUrls.Any())
            {
                var alternativeUrlInfos = AlternativeUrlInfoProvider.GetAlternativeUrls().WhereEquals("AlternativeUrlDocumentID", mTreeNode.DocumentID);

                foreach (var alternativeUrl in alternativeUrlInfos)
                {
                    AlternativeUrlInfoProvider.DeleteAlternativeUrlInfo(alternativeUrl);
                }
                return;
            }

            var existingIds = AlternativeUrlInfoProvider.GetAlternativeUrls()
                                                        .WhereEquals("AlternativeUrlDocumentID", mTreeNode.DocumentID)
                                                        .Column("AlternativeUrlID")
                                                        .Select(i => i.AlternativeUrlID)
                                                        .ToList();

            foreach (var alternativeUrl in alternativeUrls)
            {
                var currentAlternativeUrl = AlternativeUrlInfoProvider.GetAlternativeUrl(alternativeUrl.AlternativeUrlUrl, mTreeNode.NodeSiteID);
                if (currentAlternativeUrl != null && currentAlternativeUrl.AlternativeUrlDocumentID != mTreeNode.DocumentID)
                {
                    // Both values are required to generate correct message
                    alternativeUrl.AlternativeUrlSiteID = mTreeNode.NodeSiteID;
                    alternativeUrl.AlternativeUrlDocumentID = mTreeNode.DocumentID;
                    throw new InvalidAlternativeUrlException(alternativeUrl, currentAlternativeUrl);
                }

                if (currentAlternativeUrl != null)
                {
                    existingIds.Remove(currentAlternativeUrl.AlternativeUrlID);
                }
                else
                {
                    currentAlternativeUrl = alternativeUrl;
                    currentAlternativeUrl.AlternativeUrlID = 0;
                }

                currentAlternativeUrl.AlternativeUrlUrl = alternativeUrl.AlternativeUrlUrl;
                currentAlternativeUrl.AlternativeUrlDocumentID = mTreeNode.DocumentID;
                currentAlternativeUrl.AlternativeUrlSiteID = mTreeNode.NodeSiteID;

                if (mSyncManager.ProceedWithTranslations(mTranslationHelper) && ColumnsTranslationEvents.TranslateColumns.IsBound)
                {
                    ColumnsTranslationEvents.TranslateColumns.StartEvent(mTranslationHelper, AlternativeUrlInfo.OBJECT_TYPE, currentAlternativeUrl);
                }

                AlternativeUrlInfoProvider.SetAlternativeUrlInfo(currentAlternativeUrl);
            }

            existingIds.ForEach(AlternativeUrlInfoProvider.DeleteAlternativeUrlInfo);
        }
    }
}
