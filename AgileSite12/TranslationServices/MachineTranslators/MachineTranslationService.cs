using System;

using CMS.Base;
using CMS.Helpers;

using SystemIO = System.IO;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Computer translation service interface.
    /// </summary>
    public abstract class AbstractMachineTranslationService : BaseTranslationService
    {
        #region "Variables"

        internal static readonly SafeDictionary<string, AbstractMachineTranslationService> mServicesTable = new SafeDictionary<string, AbstractMachineTranslationService>();

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Translates given text using the service.
        /// </summary>
        /// <param name="text">Text you want to translate</param>
        /// <param name="sourceLang">Source language (if null or empty, automatic detection is used)</param>
        /// <param name="targetLang">Target language</param>
        public abstract string Translate(string text, string sourceLang, string targetLang);


        /// <summary>
        /// Detects language of given text using the service.
        /// </summary>
        /// <param name="text">Text the language of which you want to detect</param>
        public abstract string Detect(string text);


        /// <summary>
        /// Returns stream of wav file generated using the service.
        /// </summary>
        /// <param name="text">Text to speech</param>
        /// <param name="lang">Language in which the speech should be generated (if null or empty, automatic detection is used)</param>
        public abstract SystemIO.Stream Speak(string text, string lang);

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
                WebFarmHelper.CreateTask(new ClearMachineTranslationServiceHashtablesWebFarmTask());
            }
        }


        /// <summary>
        /// Dynamically loads the provider from the TranslationServiceInfo object information.
        /// </summary>
        /// <param name="service">TranslationService info object</param>
        /// <param name="siteName">Name of the site (for settings loading)</param>
        public static AbstractMachineTranslationService GetTranslationService(TranslationServiceInfo service, string siteName)
        {
            if ((service == null) || string.IsNullOrEmpty(service.TranslationServiceAssemblyName) || string.IsNullOrEmpty(service.TranslationServiceClassName) || !service.TranslationServiceIsMachine)
            {
                return null;
            }

            try
            {
                // Get service
                string key = (service.TranslationServiceName + ";" + siteName).ToLowerCSafe();
                var serviceInstance = mServicesTable[key];
                if (serviceInstance != null)
                {
                    return serviceInstance;
                }

                // Get service provider instance
                serviceInstance = ClassHelper.GetClass<AbstractMachineTranslationService>(service.TranslationServiceAssemblyName, service.TranslationServiceClassName);
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

        #endregion
    }
}