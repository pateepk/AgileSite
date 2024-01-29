using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Newsletters;
using CMS.Scheduler;
using CMS.Synchronization;

[assembly: RegisterImplementation(typeof(IEmailABTestService), typeof(EmailABTestService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// A/B testing service implementation.
    /// </summary>
    internal class EmailABTestService : IEmailABTestService
    {
        /// <summary>
        /// Creates a new A/B test issue variant.
        /// </summary>
        /// <remarks>The new variant is always created from the original issue variant.</remarks>
        /// <param name="name">Name of the new variant.</param>
        /// <param name="issueId">ID of the source issue.</param>
        /// <returns>Returns the newly created variant.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the original issue for issue with ID <paramref name="issueId"/> 
        /// cannot be found or when its state is other then <see cref="IssueStatusEnum.Idle"/>.
        /// </exception>
        public IssueInfo CreateVariant(string name, int issueId)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("Name of the variant cannot be empty.", nameof(name));
            }

            IssueInfo originalIssue = IssueInfoProvider.GetOriginalIssue(issueId);

            if (originalIssue == null)
            {
                throw new InvalidOperationException($"Could not find original issue for issue with ID: {issueId}.");
            }

            // Allow modifying issues in idle state only
            if (originalIssue.IssueStatus != IssueStatusEnum.Idle)
            {
                throw new InvalidOperationException("Modifying issue in state other than 'Idle' is not allowed.");
            }

            // ID of the first child (if new A/B test is being created (i.e. parent and 2 children)
            int origVariantId = 0;
            var contentIssue = originalIssue;

            // Check if current issue is variant issue
            if (!originalIssue.IssueIsABTest)
            {
                // Variant issue has not been created yet => create original and 2 child variants
                originalIssue.IssueIsABTest = true;

                // Create 1st variant based on parent issue, the 2nd variant will be created as ordinary variant below
                IssueInfo issueOrigVariant = originalIssue.Clone(true);
                issueOrigVariant.IssueVariantOfIssueID = originalIssue.IssueID;
                issueOrigVariant.IssueVariantName = null;
                issueOrigVariant.IssueScheduledTaskID = 0;
                IssueInfoProvider.SetIssueInfo(issueOrigVariant);

                // Create scheduled task for variant mail-out and update issue variant
                issueOrigVariant.IssueScheduledTaskID = CreateScheduledTask(issueOrigVariant);
                IssueInfoProvider.SetIssueInfo(issueOrigVariant);

                // Update parent issue
                IssueInfoProvider.SetIssueInfo(originalIssue);

                try
                {
                    ObjectVersionManager.DestroyObjectHistory(originalIssue.TypeInfo.ObjectType, originalIssue.IssueID);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Newsletter", "ADDVARIANT", ex);
                }

                origVariantId = issueOrigVariant.IssueID;
            }
            else
            {
                // Get original variant as content issue (if not found use original (parent) issue)
                var originalVariant = GetOriginalVariant(originalIssue.IssueID);
                contentIssue = originalVariant ?? originalIssue;
            }

            // Variant issue has been created => create new variant only
            IssueInfo issueVariant = contentIssue.Clone(true);
            issueVariant.IssueVariantName = name;
            issueVariant.IssueVariantOfIssueID = originalIssue.IssueID;
            issueVariant.IssueScheduledTaskID = 0;
            IssueInfoProvider.SetIssueInfo(issueVariant);

            // Duplicate attachments
            MetaFileInfoProvider.CopyMetaFiles(contentIssue.IssueID, issueVariant.IssueID,
                (contentIssue.IssueIsVariant ? IssueInfo.OBJECT_TYPE_VARIANT : IssueInfo.OBJECT_TYPE),
                ObjectAttachmentsCategories.ISSUE, IssueInfo.OBJECT_TYPE_VARIANT, ObjectAttachmentsCategories.ISSUE, null);

            // Create scheduled task for variant mail-out
            issueVariant.IssueScheduledTaskID = CreateScheduledTask(issueVariant);
            IssueInfoProvider.SetIssueInfo(issueVariant);

            if (origVariantId > 0)
            {
                // New A/B test issue created => create new A/B test info
                ABTestInfo abi = new ABTestInfo
                {
                    TestIssueID = originalIssue.IssueID,
                    TestSizePercentage = 50,
                    TestWinnerOption = ABTestWinnerSelectionEnum.OpenRate,
                    TestSelectWinnerAfter = 60
                };
                ABTestInfoProvider.SetABTestInfo(abi);

                // Move attachments (meta files) from parent issue to first variant
                MetaFileInfoProvider.MoveMetaFiles(originalIssue.IssueID, origVariantId, IssueInfo.OBJECT_TYPE, ObjectAttachmentsCategories.ISSUE, IssueInfo.OBJECT_TYPE_VARIANT, ObjectAttachmentsCategories.ISSUE);
                MetaFileInfoProvider.MoveMetaFiles(originalIssue.IssueID, origVariantId, IssueInfo.OBJECT_TYPE_VARIANT, ObjectAttachmentsCategories.ISSUE, IssueInfo.OBJECT_TYPE_VARIANT, ObjectAttachmentsCategories.ISSUE);
            }

            return issueVariant;
        }


        /// <summary>
        /// Deletes email variant given by <paramref name="deleteIssueId"/>. If single variant remains after the variant is deleted,
        /// then the remaining variant and related A/B test is deleted as well.
        /// </summary>
        /// <remarks>
        /// In case of single remaining variant, all its data including attachments is copied to the parent email.
        /// </remarks>
        /// <param name="deleteIssueId">Email variant ID</param>
        /// <exception cref="ArgumentOutOfRangeException">Argument <paramref name="deleteIssueId"/> is zero or negative number.</exception>
        /// <exception cref="InvalidOperationException">
        /// The original issue of issue with ID <paramref name="deleteIssueId"/> not found or when its state is other then <see cref="IssueStatusEnum.Idle"/>.
        /// </exception>
        public void DeleteVariant(int deleteIssueId)
        {
            if (deleteIssueId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deleteIssueId), deleteIssueId, "The given value should represent object ID and must be non-zero positive number.");
            }

            const string IGNORED_COLUMN_NAMES = "issueid;issueguid;issuevariantname";

            var allVariants = IssueHelper.GetIssueVariants(deleteIssueId, additionalWhereCondition: null);
            int variantCount = allVariants.Count;

            if (variantCount <= 0)
            {
                return;
            }

            // Allow modifying issues in idle state only
            IssueInfo parentIssue = IssueInfoProvider.GetOriginalIssue(deleteIssueId);
            if ((parentIssue == null) || (parentIssue.IssueStatus != IssueStatusEnum.Idle))
            {
                throw new InvalidOperationException("Modifying issue in state other than 'Idle' is not allowed.");
            }

            IssueInfoProvider.DeleteIssueInfo(deleteIssueId);

            if (variantCount <= 2)
            {
                // Get remaining variant
                var issueVariant = allVariants.FirstOrDefault(v => v.IssueID != deleteIssueId);
                IssueInfo remainingIssue = IssueInfoProvider.GetIssueInfo(issueVariant.IssueID);

                if (remainingIssue != null)
                {
                    var ignoredColumns = new HashSet<string>(IGNORED_COLUMN_NAMES.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.InvariantCultureIgnoreCase);

                    // Transfer all data from child to parent and delete child
                    foreach (string column in parentIssue.ColumnNames)
                    {
                        if (!ignoredColumns.Contains(column))
                        {
                            parentIssue.SetValue(column, remainingIssue.GetValue(column));
                        }
                    }

                    parentIssue.IssueIsABTest = false;
                    parentIssue.IssueVariantName = null;
                    parentIssue.IssueVariantOfIssueID = 0;
                    IssueInfoProvider.SetIssueInfo(parentIssue);

                    // Delete files from parent issue if any
                    MetaFileInfoProvider.DeleteFiles(parentIssue.IssueID, IssueInfo.OBJECT_TYPE, ObjectAttachmentsCategories.ISSUE);
                    MetaFileInfoProvider.DeleteFiles(parentIssue.IssueID, IssueInfo.OBJECT_TYPE_VARIANT, ObjectAttachmentsCategories.ISSUE);

                    // Move attachments (meta files) from child to parent
                    MetaFileInfoProvider.MoveMetaFiles(remainingIssue.IssueID, parentIssue.IssueID, IssueInfo.OBJECT_TYPE_VARIANT, ObjectAttachmentsCategories.ISSUE, IssueInfo.OBJECT_TYPE, ObjectAttachmentsCategories.ISSUE);
                    MetaFileInfoProvider.MoveMetaFiles(remainingIssue.IssueID, parentIssue.IssueID, IssueInfo.OBJECT_TYPE, ObjectAttachmentsCategories.ISSUE, IssueInfo.OBJECT_TYPE, ObjectAttachmentsCategories.ISSUE);

                    // Delete last variant
                    IssueInfoProvider.DeleteIssueInfo(remainingIssue);

                    // A/B variants have been deleted => deleting A/B test itself
                    ABTestInfo abi = ABTestInfoProvider.GetABTestInfoForIssue(parentIssue.IssueID);
                    ABTestInfoProvider.DeleteABTestInfo(abi);
                }
            }
        }


        /// <summary>
        /// Returns email variant which is considered as 'original'. Original variant is a clone of email which is A/B tested.
        /// </summary>
        /// <param name="issueId">ID of issue which A/B tested.</param>
        public IssueInfo GetOriginalVariant(int issueId)
        {
            var originalVariant = IssueInfoProvider.GetIssues()
                                                   .WhereEquals("IssueVariantOfIssueID", issueId)
                                                   .And()
                                                   .WhereNull("IssueVariantName")
                                                   .FirstObject;

            return originalVariant;
        }


        /// <summary>
        /// Creates new scheduled task for the given issue and newsletter.
        /// </summary>
        /// <param name="issue">Issue</param>
        private int CreateScheduledTask(IssueInfo issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            // Create new scheduled task
            TaskInfo task = NewsletterTasksManager.CreateMailoutTask(issue, DateTime.Now, false);
            TaskInfoProvider.SetTaskInfo(task);
            return task.TaskID;
        }
    }
}
