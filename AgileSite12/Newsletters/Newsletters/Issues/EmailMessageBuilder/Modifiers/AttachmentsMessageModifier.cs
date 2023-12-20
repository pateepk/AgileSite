using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include attachments.
    /// </summary>
    internal sealed class AttachmentsMessageModifier : IEmailMessageModifier
    {
        private readonly IssueInfo issue;


        /// <summary>
        /// Creates an instance of <see cref="AttachmentsMessageModifier"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        public AttachmentsMessageModifier(IssueInfo issue)
        {
            this.issue = issue;
        }


        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to modify.</param>
        public void Apply(EmailMessage message)
        {
            if (issue == null || !IssueIsTemplateBased())
            {
                return;
            }

            var metafiles = GetMetafiles();
            EmailHelper.ResolveMetaFileImages(message, metafiles);
        }


        private bool IssueIsTemplateBased()
        {
            return issue.IssueTemplateID > 0;
        }


        private IEnumerable<MetaFileInfo> GetMetafiles()
        {
            List<MetaFileInfo> metafiles;

            // Try to get metafiles from cache
            if (CacheHelper.TryGetItem(GetCacheKey(), out metafiles))
            {
                return metafiles;
            }

            int issueId = issue.IssueID;
            string objectType = IssueInfo.OBJECT_TYPE;

            // Use winner variant issue ID if the winner is being sent
            if (issue.IssueIsABTest && !issue.IssueIsVariant)
            {
                // Get ID of winner variant from A/B test's result
                ABTestInfo abTest = ABTestInfoProvider.GetABTestInfoForIssue(issueId);
                if (abTest != null && abTest.TestWinnerIssueID > 0)
                {
                    issueId = abTest.TestWinnerIssueID;
                    objectType = IssueInfo.OBJECT_TYPE_VARIANT;
                }
            }
            else if (issue.IssueIsVariant)
            {
                objectType = IssueInfo.OBJECT_TYPE_VARIANT;
            }

            // Prepare where condition to retrieve metafiles for issue and template
            string where = MetaFileInfoProvider.GetWhereCondition(issueId, objectType, ObjectAttachmentsCategories.ISSUE);
            where = SqlHelper.AddWhereCondition(where, MetaFileInfoProvider.GetWhereCondition(issue.IssueTemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE), "OR");

            // Get all meta-files associated to issue and template
            InfoDataSet<MetaFileInfo> ds = MetaFileInfoProvider.GetMetaFiles(where, null);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                metafiles = new List<MetaFileInfo>(ds.Tables[0].Rows.Count);

                // Fill the metafile list
                foreach (MetaFileInfo mfi in ds)
                {
                    // Get binary data from the disk if not present
                    if (mfi.MetaFileBinary == null)
                    {
                        // Get the site name
                        var siteName = SiteInfoProvider.GetSiteName(mfi.MetaFileSiteID);

                        mfi.MetaFileBinary = MetaFileInfoProvider.GetFileBinary(siteName, MetaFileInfoProvider.GetFullFileName(mfi.MetaFileGUID.ToString(), mfi.MetaFileExtension));
                    }

                    metafiles.Add(mfi);
                }

                CacheHelper.Add(GetCacheKey(), metafiles, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(1), CMSCacheItemPriority.Normal);
            }

            return metafiles;
        }


        private string GetCacheKey()
        {
            return $"newsletterissue|{issue.IssueID}|neslettertemplate|{issue.IssueTemplateID}";
        }
    }
}
