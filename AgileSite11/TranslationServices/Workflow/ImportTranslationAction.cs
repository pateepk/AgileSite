using System;

using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class for document translation import
    /// </summary>
    public class ImportTranslationAction : DocumentWorkflowAction
    {
        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            if (Node == null)
            {
                return;
            }

            var query = TranslationServiceHelper.GetLatestTranslatedSubmissionItemQuery(Node)
                .Columns("SubmissionItemTargetXLIFF, SubmissionItemSubmissionID");

            if (DataHelper.DataSourceIsEmpty(query))
            {
                return;
            }

            var row = query.Tables[0].Rows[0];
            string xliff = ValidationHelper.GetString(row[0], "");
            int submissionId = ValidationHelper.GetInteger(row[1], 0);

            // Get submission
            var submission = TranslationSubmissionInfoProvider.GetTranslationSubmissionInfo(submissionId);
            if (!TranslationServiceHelper.IsSubmissionReady(submission))
            {
                return;
            }

            try
            {
                submission.SubmissionStatus = TranslationStatusEnum.ProcessingSubmission;
                submission.Update();

                TranslationServiceHelper.ProcessTranslation(xliff);

                submission.SubmissionStatus = TranslationStatusEnum.TranslationCompleted;
                submission.Update();
            }
            catch (Exception)
            {
                submission.SubmissionStatus = TranslationStatusEnum.ProcessingError;
                submission.Update();
            }
        }
    }
}
