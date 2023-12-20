using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

using CMS.Helpers;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.DocumentEngine;
using CMS.SiteProvider;

using GlobalLink.Connect;
using GlobalLink.Connect.Config;
using GlobalLink.Connect.Model;

using Project = GlobalLink.Connect.Model.Project;
using ProjectLanguageDirection = GlobalLink.Connect.Model.LanguageDirection;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translations.com provider.
    /// </summary>
    public class TranslationsComService : AbstractHumanTranslationService
    {
        private const string TRANSLATIONS_COM_CODE_NAME = "TranslationsCom";
        private const int MAX_TARGETS_COUNT = 100;
        private const string PROCESS_COMPLETED_CODE = "PROCESSCOMPLETED";
        private const string SUBMIT_TRANSLATION_CODE = "SUBMITTRANSLATION";
        private const string USER_AGENT = "kentico";
        private const string FILE_FORMAT = "XLIFF-Kentico";


        private readonly Lazy<HashSet<string>> mSupportedTargetLanguages;
        private readonly Lazy<HashSet<string>> mSupportedSourceLanguages;
        private readonly Lazy<GLExchange> mClient;
        private readonly Lazy<Project> mProject;
        private readonly Lazy<Regex> mReplaceTargetLanguageRegex = new Lazy<Regex>(() => new Regex("target-language=\".[^\"]*\"", RegexOptions.IgnoreCase));
        private readonly Lazy<bool> mIsServiceAvailable;


        private string ClientUserName => SettingsKeyInfoProvider.GetValue(SiteName + ".CMSTranslationsComUserName");
        private string ClientPassword => SettingsKeyInfoProvider.GetValue(SiteName + ".CMSTranslationsComPassword");
        private string ProjectShortID => SettingsKeyInfoProvider.GetValue(SiteName + ".CMSTranslationsComProjectCode");
        private string ProjectDirectorURL => SettingsKeyInfoProvider.GetValue(SiteName + ".CMSTranslationsComURL");
        private GLExchange Client => mClient.Value;
        private Project Project => mProject.Value;
        private bool IsServiceAvailable => mIsServiceAvailable.Value;
        private HashSet<string> SupportedTargetLanguages => mSupportedTargetLanguages.Value;
        private HashSet<string> SupportedSourceLanguages => mSupportedSourceLanguages.Value;
        private Regex ReplaceTargetLanguageRegex => mReplaceTargetLanguageRegex.Value;


        /// <summary>
        /// Creates an isntance of <see cref="TranslationsComService"/>
        /// </summary>
        public TranslationsComService()
        {
            mClient = new Lazy<GLExchange>(GetClient);
            mProject = new Lazy<Project>(() => Client.getProject(ProjectShortID));
            mSupportedSourceLanguages = new Lazy<HashSet<string>>(() => GetSupportedLanguages(GetSourceLanguage));
            mSupportedTargetLanguages = new Lazy<HashSet<string>>(() => GetSupportedLanguages(GetTargetLanguage));
            mIsServiceAvailable = new Lazy<bool>(GetServiceAvailibility);
        }


        /// <summary>
        /// Checks if everything required to run the service is in the settings of the service.
        /// </summary>
        public override bool IsAvailable()
        {
            return !string.IsNullOrEmpty(ClientUserName)
                   && !string.IsNullOrEmpty(ClientPassword)
                   && !string.IsNullOrEmpty(ProjectShortID)
                   && !string.IsNullOrEmpty(ProjectDirectorURL)
                   && IsServiceAvailable;
        }


        /// <summary>
        /// Checks if target language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public override bool IsTargetLanguageSupported(string langCode)
        {
            return !string.IsNullOrEmpty(langCode) && SupportedTargetLanguages.Contains(langCode);
        }


        /// <summary>
        /// Checks if source language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public override bool IsSourceLanguageSupported(string langCode)
        {
            return !string.IsNullOrEmpty(langCode) && SupportedSourceLanguages.Contains(langCode);
        }


        /// <summary>
        /// Creates new submission (or resubmits existing if submission ticket is present).
        /// </summary>
        /// <param name="submission">Submission object</param>
        /// <exception cref="Exception">Thrown when missing license for translation feature</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="submission"/> is <c>null</c></exception>
        public override string CreateSubmission(TranslationSubmissionInfo submission)
        {
            TranslationServiceHelper.CheckLicense();

            if (submission == null)
            {
                throw new ArgumentNullException(nameof(submission));
            }

            try
            {
                var glSubmission = CreateGLSubmission(submission);
                Client.initSubmission(glSubmission);

                var submissionItems = GetSubmissionItems(submission).ToList();
                UploadSubmissionItems(submissionItems, submission);

                // Start submission in Project Director
                var submissionTickets = Client.startSubmission();
                if (submissionTickets != null && submissionTickets.Length > 0)
                {
                    submission.SubmissionTicket = submissionTickets[0];
                    TranslationServiceHelper.LogInformation(SUBMIT_TRANSLATION_CODE, $"Submission '{submission.SubmissionName}' submitted with username '{ClientUserName}' to Translations.com with submission ticket '{submission.SubmissionTicket}'.");
                }
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return ex.Message;
            }

            return null;
        }


        /// <summary>
        /// Cancels given submission.
        /// </summary>
        /// <param name="submission">Submission to cancel</param>
        public override string CancelSubmission(TranslationSubmissionInfo submission)
        {
            // This provider does not support automatic cancel.
            return null;
        }


        /// <summary>
        /// Retrieves completed XLIFF files from the service and processes them (imports them into the system). Returns null if everything went well.
        /// </summary>
        /// <param name="siteName">Name of site for which this method downloads completed XLIFF files.</param>
        public override string DownloadCompletedTranslations(string siteName)
        {
            // Check the license
            TranslationServiceHelper.CheckLicense();

            try
            {
                var siteId = SiteInfoProvider.GetSiteID(siteName);
                CheckCanceledSubmissions(siteId);
                return CheckCompletedSubmission(siteId);
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return ex.Message;
            }
        }


        private bool GetServiceAvailibility()
        {
            var isAvailable = false;
            try
            {
                isAvailable = Project != null;
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
            }

            return isAvailable;
        }


        private GLExchange GetClient()
        {
            var config = new ProjectDirectorConfig
            {
                username = ClientUserName,
                password = ClientPassword,
                url = ProjectDirectorURL,
                userAgent = USER_AGENT
            };

            return new GLExchange(config);
        }


        private static string GetSourceLanguage(LanguageDirection direction)
        {
            return direction.sourceLanguage;
        }


        private static string GetTargetLanguage(LanguageDirection direction)
        {
            return direction.targetLanguage;
        }


        private HashSet<string> GetSupportedLanguages(Func<LanguageDirection, string> getLanguage)
        {
            var languages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                // Get allowed languages
                var directions = Project.languageDirections;

                foreach (var direction in directions)
                {
                    var locale = getLanguage(direction);
                    if (locale == null)
                    {
                        continue;
                    }

                    languages.Add(locale.ToLowerInvariant());
                }
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
            }

            return languages;
        }


        private static IEnumerable<TranslationSubmissionItemInfo> GetSubmissionItems(TranslationSubmissionInfo submission)
        {
            return TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems()
                                          .WhereEquals("SubmissionItemSubmissionID", submission.SubmissionID);
        }


        private void UploadSubmissionItems(IReadOnlyCollection<TranslationSubmissionItemInfo> submissionItems, TranslationSubmissionInfo submission)
        {
            var encodingObj = TranslationServiceHelper.GetTranslationsEncoding(SiteName);

            var sourceCulture = submission.GetServiceSourceCulture();

            var condition = new WhereCondition()
                .WhereEquals("SubmissionItemSubmissionID", submission.SubmissionID)
                .Immutable();

            var itemGroups = submissionItems.GroupBy(item => item.SubmissionItemObjectID);
            foreach (var itemGroup in itemGroups)
            {
                var submissionItem = itemGroup.First();
                var targetCultures = itemGroup.Select(item => TranslateCultureCode(item.SubmissionItemTargetCulture));

                var document = GetGLDocument(submissionItem, sourceCulture, targetCultures, encodingObj);

                // Upload new submission item to Project Director
                var docTicket = Client.uploadTranslatable(document);

                if (string.IsNullOrEmpty(docTicket))
                {
                    continue;
                }

                // For all cultures for the submitted document update document ticket to able to find it after translation
                var updateCondition = condition.WhereEquals("SubmissionItemObjectID", submissionItem.SubmissionItemObjectID);
                TranslationSubmissionItemInfoProvider.UpdateData($"SubmissionItemCustomData = '{docTicket}'", updateCondition.ToString(), updateCondition.Parameters);
            }
        }


        private static string TranslateCultureCode(string targetCultureCode)
        {
            return TranslationServiceHelper.GetCultureCode(targetCultureCode, TranslationCultureMappingDirectionEnum.SystemToService);
        }


        private Submission CreateGLSubmission(TranslationSubmissionInfo submission)
        {
            var glSubmission = new Submission
            {
                name = submission.SubmissionName,
                pmNotes = submission.SubmissionInstructions,
                project = Project
            };

            if (submission.SubmissionDeadline != DateTimeHelper.ZERO_TIME && DateTime.Now < submission.SubmissionDeadline)
            {
                glSubmission.dueDate = submission.SubmissionDeadline;
            }

            return glSubmission;
        }


        private static Document GetGLDocument(TranslationSubmissionItemInfo submissionItem, string sourceCulture, IEnumerable<string> targetCultures, Encoding encodingObj)
        {
            return new Document
            {
                sourceLanguage = sourceCulture,
                targetLanguages = targetCultures.ToArray(),
                name = submissionItem.SubmissionItemName,
                data = encodingObj.GetBytes(submissionItem.SubmissionItemSourceXLIFF),
                encoding = encodingObj.WebName.ToUpperInvariant(),
                fileformat = FILE_FORMAT
            };
        }


        /// <summary>
        /// Processes submissions the translation of which is completed.
        /// </summary>
        private string CheckCompletedSubmission(int siteId)
        {
            var submissionTickets = GetSubmissionTicketsToBeProcessed(siteId);
            if (submissionTickets.Count <= 0)
            {
                return null;
            }

            int counter;
            var translatedSubmissions = new HashSet<TranslationSubmissionInfo>(new TranslationSubmissionComparer());

            do
            {
                var targets = Client.getCompletedTargetsBySubmissions(submissionTickets.ToArray(), MAX_TARGETS_COUNT);
                if (targets == null || targets.Length <= 0 || targets[0] == null)
                {
                    // Reset counter to stop loop
                    counter = 0;
                    continue;
                }

                counter = targets.Length;
                var batchTranslatedSubmissions = ProcessCompletedTargets(targets);
                translatedSubmissions.AddRangeToSet(batchTranslatedSubmissions);

                // Ask the server for another batch when we have same than MAX_TARGETS_COUNT items
            } while (counter == MAX_TARGETS_COUNT);


            return AutomaticallyImportTranslatedSubmissions(translatedSubmissions);
        }


        private static string AutomaticallyImportTranslatedSubmissions(IEnumerable<TranslationSubmissionInfo> translatedSubmissions)
        {
            foreach (var submission in translatedSubmissions)
            {
                var result = TranslationServiceHelper.AutoImportSubmission(submission);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return null;
        }


        private HashSet<TranslationSubmissionInfo> ProcessCompletedTargets(IEnumerable<Target> targets)
        {
            var translatedSubmissions = new HashSet<TranslationSubmissionInfo>(new TranslationSubmissionComparer());
            foreach (var target in targets)
            {
                var processedSubmissionItem = ProcessCompletedTarget(target.documentTicket, target.ticket, target.targetLocale);
                if (processedSubmissionItem == null)
                {
                    continue;
                }

                var processedSubmission = TranslationSubmissionInfoProvider.GetTranslationSubmissionInfo(processedSubmissionItem.SubmissionItemSubmissionID);
                if (processedSubmission == null)
                {
                    continue;
                }

                UpdateProcessedSubmissionStatus(processedSubmission);

                // Collect submissions for automatic import since all targets need to be translated before import
                if (TranslationServiceHelper.AutoImportEnabled)
                {
                    translatedSubmissions.Add(processedSubmission);
                }
            }

            return translatedSubmissions;
        }

        private static void UpdateProcessedSubmissionStatus(TranslationSubmissionInfo processedSubmission)
        {
            // Do not set status to TranslationReady if submission was cancelled
            if (processedSubmission.SubmissionStatus == TranslationStatusEnum.TranslationCanceled)
            {
                return;
            }

            processedSubmission.SubmissionStatus = TranslationStatusEnum.TranslationReady;
            TranslationSubmissionInfoProvider.SetTranslationSubmissionInfo(processedSubmission);
        }


        /// <summary>
        /// Processes submissions the translation of which is canceled.
        /// </summary>
        private void CheckCanceledSubmissions(int siteId)
        {
            var submissionTickets = GetSubmissionTicketsToBeProcessed(siteId);
            if (submissionTickets.Count <= 0)
            {
                return;
            }

            var canceledTickets = GetCanceledTickets(submissionTickets);
            if (canceledTickets.Count <= 0)
            {
                return;
            }

            var submissionsWhere = new WhereCondition().WhereIn("SubmissionTicket", canceledTickets);
            UpdateCanceledSubmissionsStatus(submissionsWhere);
            UpdateDocumentsWaitingForTranslationFlag(submissionsWhere);
        }


        private static IList<string> GetSubmissionTicketsToBeProcessed(int siteId)
        {
            var service = GetTranslationComServiceInfo();

            return TranslationSubmissionInfoProvider
                .GetTranslationSubmissions()
                .Column("SubmissionTicket")
                .WhereEquals("SubmissionSiteID", siteId)
                .WhereNotEmpty("SubmissionTicket")
                .WhereEquals("SubmissionServiceID", service.TranslationServiceID)
                .WhereEquals("SubmissionStatus", (int)TranslationStatusEnum.WaitingForTranslation)
                .GetListResult<string>();
        }


        private static void UpdateDocumentsWaitingForTranslationFlag(IWhereCondition submissionsWhere)
        {
            var where = new WhereCondition().WhereIn("DocumentID", GetCanceledDocumentIds(submissionsWhere));
            DocumentHelper.ChangeDocumentCultureDataField("DocumentIsWaitingForTranslation", false, where);
        }


        private static ObjectQuery<TranslationSubmissionItemInfo> GetCanceledDocumentIds(IWhereCondition submissionsWhere)
        {
            return TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems()
                                      .WhereEquals("SubmissionItemObjectType", "cms.document")
                                      .WhereIn("SubmissionItemSubmissionID", new IDQuery<TranslationSubmissionInfo>().Where(submissionsWhere))
                                      .Column("SubmissionItemTargetObjectID");
        }


        private static void UpdateCanceledSubmissionsStatus(WhereCondition submissionsWhere)
        {
            TranslationSubmissionInfoProvider.UpdateStatuses(TranslationStatusEnum.TranslationCanceled, submissionsWhere.ToString(true));
        }


        private IList<string> GetCanceledTickets(IEnumerable<string> submissionsTickets)
        {
            var tickets = submissionsTickets.ToHashSetCollection(StringComparer.OrdinalIgnoreCase);
            var cancelledSubmissionsTickets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int counter;

            do
            {
                var ticketsToProcess = tickets.Except(cancelledSubmissionsTickets, StringComparer.OrdinalIgnoreCase).ToArray();
                if (ticketsToProcess.Length == 0)
                {
                    break;
                }

                var targets = Client.getCancelledTargetsBySubmissions(ticketsToProcess, MAX_TARGETS_COUNT);
                if (targets == null || targets.Length <= 0 || targets[0] == null)
                {
                    // Reset counter to stop loop
                    counter = 0;
                    continue;
                }

                counter = targets.Length;

                var cancelledDocumentTickets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var target in targets)
                {
                    cancelledDocumentTickets.Add(target.documentTicket);
                }

                cancelledSubmissionsTickets.AddRangeToSet(GetSubmissionTicketFromCanceledItems(cancelledDocumentTickets));
            } while (counter == MAX_TARGETS_COUNT);

            return cancelledSubmissionsTickets.ToList();
        }


        private static ObjectQuery<TranslationSubmissionItemInfo> GetSubmissionIdsFromCanceledItems(ICollection<string> canceledTickets)
        {
            return TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems()
                .Distinct()
                .Columns("SubmissionItemSubmissionID")
                .WhereIn("SubmissionItemCustomData", canceledTickets);
        }


        private static IEnumerable<string> GetSubmissionTicketFromCanceledItems(ICollection<string> canceledTickets)
        {
            var submissionIds = GetSubmissionIdsFromCanceledItems(canceledTickets);

            return TranslationSubmissionInfoProvider
                .GetTranslationSubmissions()
                .Column("SubmissionTicket")
                .WhereIn("SubmissionID", submissionIds)
                .GetListResult<string>();
        }


        private static TranslationServiceInfo GetTranslationComServiceInfo()
        {
            var service = TranslationServiceInfoProvider.GetTranslationServiceInfo(TRANSLATIONS_COM_CODE_NAME);
            if (service == null)
            {
                throw new NullReferenceException($"The translation service with a code name '{TRANSLATIONS_COM_CODE_NAME}' was not found.");
            }

            return service;
        }


        /// <summary>
        /// Processes document with given ticket. Returns ID of the submission to which the submission item belongs.
        /// </summary>
        /// <param name="documentTicket">Target document ticket</param>
        /// <param name="targetTicket">Target item ticket</param>
        /// <param name="targetLanguage">Target language in which was translation requested</param>
        private TranslationSubmissionItemInfo ProcessCompletedTarget(string documentTicket, string targetTicket, string targetLanguage)
        {
            // Map service culture to the system culture
            var targetSystemLanguage = TranslationServiceHelper.GetCultureCode(targetLanguage, TranslationCultureMappingDirectionEnum.ServiceToSystem);

            // Receive the data
            var resource = Client.downloadCompletedTarget(targetTicket);

            if (!IsTargetResourceValid(targetTicket, resource))
            {
                return null;
            }

            // Get the submission item based on document ticket and the target language
            var submissionItem = TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems()
                                                    .WhereEquals("SubmissionItemCustomData", documentTicket)
                                                    .WhereEquals("SubmissionItemTargetCulture", targetSystemLanguage)
                                                    .TopN(1)
                                                    .FirstOrDefault();
            if (submissionItem == null)
            {
                TranslationServiceHelper.LogWarning(PROCESS_COMPLETED_CODE, $"No translation submission items found for a document ticket '{documentTicket}'.");

                return null;
            }

            var xliff = TranslationServiceHelper.GetTranslationsEncoding(SiteName).GetString(resource.ToArray());

            // Replace target language in the XLIFF because we need correct value in further processing (Translations.com doesn't care about target-language attribute)
            submissionItem.SubmissionItemTargetXLIFF = ReplaceTargetLanguageRegex.Replace(xliff, $"target-language=\"{targetSystemLanguage}\"", 1);

            TranslationSubmissionItemInfoProvider.SetTranslationSubmissionItemInfo(submissionItem);

            // Send confirmation of successful download
            Client.sendDownloadConfirmation(targetTicket);

            return submissionItem;
        }


        private static bool IsTargetResourceValid(string targetTicket, MemoryStream resource)
        {
            if (resource != null)
            {
                return true;
            }

            TranslationServiceHelper.LogWarning(PROCESS_COMPLETED_CODE, $"Response for a ticket '{targetTicket}' does not have any data.");

            return false;
        }


        private class TranslationSubmissionComparer : IEqualityComparer<TranslationSubmissionInfo>
        {
            public bool Equals(TranslationSubmissionInfo first, TranslationSubmissionInfo second)
            {
                return first.SubmissionGUID == second.SubmissionGUID;
            }

            public int GetHashCode(TranslationSubmissionInfo item)
            {
                return StringComparer.InvariantCultureIgnoreCase.GetHashCode(item.SubmissionGUID);
            }
        }
    }
}