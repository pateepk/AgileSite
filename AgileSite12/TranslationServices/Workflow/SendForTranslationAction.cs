using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class for sending the document for translation via workflow.
    /// </summary>
    public class SendForTranslationAction : DocumentWorkflowAction
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

            // Prepare translation settings
            var settings = new TranslationSettings
            {
                SourceLanguage = Node.DocumentCulture,
                Priority = GetResolvedParameter("Priority", 1),
                TranslateAttachments = GetResolvedParameter("ProcessBinary", false),
                TranslationServiceName = GetResolvedParameter("ServiceName", ""),
                Instructions = GetResolvedParameter("Instructions", ""),
                TranslationDeadline = GetDeadlineDate(GetResolvedParameter("Deadline", ""))
            };

            // Check that the culture we are translating into is allowed in target site
            var targetCultures = GetTargetCultures();

            var message = new StringBuilder();
            
            // Create submission per culture
            foreach (string targetCulture in targetCultures)
            {
                settings.TargetLanguages.Clear();
                settings.TargetLanguages.Add(targetCulture);

                // Submit the document to translation using given settings
                TranslationSubmissionInfo submissionInfo;
                string result = TranslationServiceHelper.SubmitToTranslation(settings, Node, out submissionInfo);

                // Operation did not succeed
                if (!string.IsNullOrEmpty(result))
                {
                    message.AppendLine(result + "(" + targetCulture + ")");
                }
            }

            var errorMessages = message.ToString();
            if (!String.IsNullOrEmpty(errorMessages))
            {
                throw new Exception(errorMessages);
            }
        }


        /// <summary>
        /// Gets validated target cultures
        /// </summary>
        private IEnumerable<string> GetTargetCultures()
        {
            var allowedCultures = CultureSiteInfoProvider.GetSiteCultureCodes(Node.NodeSiteName);
            var targetCultures = new List<string>(GetResolvedParameter("TargetLanguage", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            var notAllowed = targetCultures.Except(allowedCultures, StringComparer.InvariantCultureIgnoreCase).ToList();
            if (notAllowed.Count > 0)
            {
                throw new Exception(String.Format("Cultures '{0}' are not allowed on the site.", String.Join(", ", notAllowed)));
            }

            return targetCultures;
        }


        /// <summary>
        /// Get deadline date in DateTime format
        /// </summary>
        /// <param name="deadline">Deadline settings</param>
        private static DateTime GetDeadlineDate(string deadline)
        {
            if (string.IsNullOrEmpty(deadline))
            {
                return DateTimeHelper.ZERO_TIME;
            }

            string[] parts = deadline.Split(';');
            if (parts.Length != 2)
            {
                return DateTimeHelper.ZERO_TIME;
            }

            DateTime deadlineDate = DateTime.Now;

            int number = ValidationHelper.GetInteger(parts[0], 0);
            switch (parts[1].ToLowerCSafe())
            {
                case SchedulingHelper.PERIOD_HOUR:
                    deadlineDate = deadlineDate.AddHours(number);
                    break;

                case SchedulingHelper.PERIOD_DAY:
                    deadlineDate = deadlineDate.AddDays(number);
                    break;

                case SchedulingHelper.PERIOD_WEEK:
                    deadlineDate = deadlineDate.AddDays(number * 7);
                    break;

                case SchedulingHelper.PERIOD_MONTH:
                    deadlineDate = deadlineDate.AddMonths(number);
                    break;

                case SchedulingHelper.PERIOD_YEAR:
                    deadlineDate = deadlineDate.AddYears(number);
                    break;
            }

            return deadlineDate;
        }
    }
}
