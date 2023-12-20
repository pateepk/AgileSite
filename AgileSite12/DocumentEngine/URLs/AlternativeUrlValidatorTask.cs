using System;
using System.Text;

using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides a scheduled task that validates alternative URLs against site settings (Excluded alternative URLs), checks conflicts between page URLs and alternative URLs and logs detected issues to event log.
    /// </summary>
    public class AlternativeUrlValidatorTask : ITask
    {
        internal const string VALIDATOR_EVENT_LOG_SOURCE = "AlternativeURLValidator";

        /// <summary>
        /// Executes the task given in a task info.
        /// </summary>
        /// <param name="task">Container with task information</param>
        /// <returns>Textual description of task run's failure if any.</returns>
        public string Execute(TaskInfo task)
        {
            // Only execute site specific tasks
            if ((task == null) || (task.TaskSiteID <= 0))
            {
                return null;
            }

            try
            {
                SiteInfo site = SiteInfoProvider.GetSiteInfo(task.TaskSiteID);
                if (site == null)
                {
                    return "Task site not found.";
                }

                if (!site.SiteIsContentOnly)
                {
                    return "Task cannot be executed on on non-content-only site.";
                }

                // No need to proceed if there are no alternative URLs on site
                if (AlternativeUrlInfoProvider.GetAlternativeUrls().Columns("AlternativeUrlID").OnSite(site.SiteID).TopN(1).Count == 0)
                {
                    return null;
                }

                CheckConflictsWithAlternativeUrls(site.SiteID, out var anyConflictDetected);
                ValidateAlternativeUrlsAgainstSettings(site.SiteID, out var anyUrlDoesNotMatchSettings);

                return (anyConflictDetected || anyUrlDoesNotMatchSettings) ? $"Issues detected. See the event log (Source: '{VALIDATOR_EVENT_LOG_SOURCE}') for more details." : null;
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("AlternativeUrlValidatorTask", "EXCEPTION", e);

                return e.Message;
            }
        }


        private static void CheckConflictsWithAlternativeUrls(int siteId, out bool conflictsDetected)
        {
            var stringBuilder = new StringBuilder();

            var classesWithClassUrlPattern = DataClassInfoProvider.GetClasses().Column("ClassName").WhereTrue("ClassIsContentOnly").WhereNotEmpty("ClassURLPattern").GetListResult<string>();

            foreach (var className in classesWithClassUrlPattern)
            {
                var query = new DocumentQuery(className).OnSite(siteId);

                // Process in batches for better performance when there are many documents on site
                query.ForEachPage(q =>
                    {
                        var nodesToCheck = q.TypedResult;
                        new AlternativeUrlConflictChecker(nodesToCheck, siteId, stringBuilder).CheckConflicts();
                    },
                    TreeProvider.PROCESSING_BATCH
                );
            }

            conflictsDetected = false;

            var message = stringBuilder.ToString();

            if (!String.IsNullOrEmpty(message))
            {
                EventLogProvider.LogEvent(EventType.ERROR, VALIDATOR_EVENT_LOG_SOURCE, "CONFLICTSDETECTED", message, siteId: siteId);
                conflictsDetected = true;
            }
        }

        
        private static void ValidateAlternativeUrlsAgainstSettings(int siteId, out bool anyUrlDoesNotMatchSetting)
        {
            var stringBuilderError = new StringBuilder();
            var stringBuilderWarning = new StringBuilder();

            bool issueDetected = false;

            AlternativeUrlInfoProvider.GetAlternativeUrls().OnSite(siteId).ForEachObject((info) =>
            {
                var excludedUrl = AlternativeUrlHelper.GetConflictingExcludedUrl(info.AlternativeUrlUrl, info.AlternativeUrlSiteID);

                bool conflictsWithExcludedUrl = !String.IsNullOrEmpty(excludedUrl);

                if (conflictsWithExcludedUrl || !AlternativeUrlHelper.UrlMatchesConstraint(info))
                {
                    issueDetected = true;

                    var documentIdentification = AlternativeUrlHelper.GetDocumentIdentification(info);

                    if (conflictsWithExcludedUrl)
                    {
                        stringBuilderError.AppendLine($"Alternative URL '{info.AlternativeUrlUrl}' of page {documentIdentification} is in conflict with '{excludedUrl}' excluded URL.");
                        stringBuilderError.AppendLine();
                    }
                    else
                    {
                        stringBuilderWarning.AppendLine($"Alternative URL '{info.AlternativeUrlUrl}' of page {documentIdentification} does not satisfy pattern constraint.");
                        stringBuilderWarning.AppendLine();
                    }
                }
            }, TreeProvider.PROCESSING_BATCH);

            var errorMessage = stringBuilderError.ToString();
             
            if (!String.IsNullOrEmpty(errorMessage))
            {
                EventLogProvider.LogEvent(EventType.ERROR, VALIDATOR_EVENT_LOG_SOURCE, "ISSUESDETECTED", errorMessage, siteId: siteId);
            }

            var warningMessage = stringBuilderWarning.ToString();

            if (!String.IsNullOrEmpty(warningMessage))
            {
                EventLogProvider.LogEvent(EventType.WARNING, VALIDATOR_EVENT_LOG_SOURCE, "ISSUESDETECTED", warningMessage, siteId: siteId);
            }

            anyUrlDoesNotMatchSetting = issueDetected;
        }
    }
}
