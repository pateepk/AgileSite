using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Human translation service interface.
    /// </summary>
    public abstract class AbstractHumanTranslationService : BaseTranslationService
    {
        #region "Variables"

        internal static readonly SafeDictionary<string, AbstractHumanTranslationService> mServicesTable = new SafeDictionary<string, AbstractHumanTranslationService>();

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Creates new submission (or resubmits existing if submission ticket is present).
        /// </summary>
        /// <param name="submission">Submission info</param>
        public abstract string CreateSubmission(TranslationSubmissionInfo submission);


        /// <summary>
        /// Cancels given submission. Return 
        /// </summary>
        /// <param name="submission">Submission info</param>
        public abstract string CancelSubmission(TranslationSubmissionInfo submission);


        /// <summary>
        /// Retrieves completed XLIFF files from the service and saves them to appropriate submission items.
        /// </summary>
        /// <param name="siteName">Name of site for which this method downloads completed XLIFF files.</param>
        public abstract string DownloadCompletedTranslations(string siteName);


        /// <summary>
        /// Checks if target language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public abstract bool IsTargetLanguageSupported(string langCode);

        /// <summary>
        /// Checks if source language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public abstract bool IsSourceLanguageSupported(string langCode);

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears the hashtables of cached services and optionally logs a web farm task to propagate the change.
        /// </summary>
        /// <param name="logTask">A value indicating whether to log a web farm task.</param>

        internal static void ClearHashtables(bool logTask)
        {
            mServicesTable.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearHumanTranslationServiceHashtablesWebFarmTask());
            }
        }


        /// <summary>
        /// Dynamically loads the provider from the TranslationServiceInfo object information.
        /// </summary>
        /// <param name="service">TranslationService info object</param>
        /// <param name="siteName">Name of the site (for settings loading)</param>
        public static AbstractHumanTranslationService GetTranslationService(TranslationServiceInfo service, string siteName)
        {
            if ((service == null) || string.IsNullOrEmpty(service.TranslationServiceAssemblyName) || string.IsNullOrEmpty(service.TranslationServiceClassName) || service.TranslationServiceIsMachine)
            {
                return null;
            }

            try
            {
                // Get the service
                string key = (service.TranslationServiceName + ";" + siteName).ToLowerCSafe();
                AbstractHumanTranslationService serviceInstance = mServicesTable[key];
                if (serviceInstance != null)
                {
                    return serviceInstance;
                }

                // Try to get service provider instance
                serviceInstance = ClassHelper.GetClass<AbstractHumanTranslationService>(service.TranslationServiceAssemblyName, service.TranslationServiceClassName);
                if (serviceInstance == null)
                {
                    return null;
                }

                // Initialize service
                serviceInstance.SiteName = siteName;
                serviceInstance.CustomParameter = service.TranslationServiceParameter;
                mServicesTable[key] = serviceInstance;

                return serviceInstance;
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
            }

            return null;
        }


        /// <summary>
        /// Checks that all given target languages are supported by selected service.
        /// </summary>
        /// <param name="targetLanguages">List of target languages</param>
        /// <returns>List of the unsupported languages</returns>
        public List<string> CheckTargetLanguagesAvailability(ICollection<string> targetLanguages)
        {
            return targetLanguages.Where(language =>
            {
                // Check supported languages in context of service culture codes
                var serviceLang = TranslationServiceHelper.GetCultureCode(language, TranslationCultureMappingDirectionEnum.SystemToService);
                return !IsTargetLanguageSupported(serviceLang);
            }).ToList();
        }


        /// <summary>
        /// Checks that given source language is supported by selected service.
        /// </summary>
        /// <param name="sourceLanguage">Source language</param>
        /// <returns>True if given language is supported by translation service, otherwise returns false</returns>
        public bool CheckSourceLanguageAvailability(string sourceLanguage)
        {
            var serviceLang = TranslationServiceHelper.GetCultureCode(sourceLanguage, TranslationCultureMappingDirectionEnum.SystemToService);
            return IsSourceLanguageSupported(serviceLang);
        }

        #endregion
    }
}