using System;
using System.Web;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using CMS.Base;
using CMS.DataEngine;

using SystemIO = System.IO;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class providing access to the Google Translator Service API.
    /// </summary>
    public class GoogleTranslatorService : AbstractMachineTranslationService
    {
        #region "Variables"

        private bool? mIsAvailable;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the API key needed to access the service.
        /// </summary>
        public string GoogleTranslateAPIKey
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteName + ".CMSGoogleTranslateAPIKey");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Translates given text using free Google Translation API.
        /// </summary>
        /// <param name="text">Text you want to translate</param>
        /// <param name="sourceLang">Source language (if null or empty, automatic detection is used)</param>
        /// <param name="targetLang">Target language</param>
        public override string Translate(string text, string sourceLang, string targetLang)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            try
            {
                string requestString = String.Format("{0}&target={1}&q={2}", String.IsNullOrEmpty(sourceLang) ? "" : "&source=" + GetLangCode(sourceLang), 
                                                                            GetLangCode(targetLang), 
                                                                            HttpUtility.UrlEncode(text));

                var translation = GetGoogleResponse(requestString);

                return HttpUtility.HtmlDecode(translation.TranslatedText);
            }
            catch (Exception ex)
            {
                EventLog.EventLogProvider.LogException("GoogleTranslator", "Translate", ex);
                return text;
            }
        }


        /// <summary>
        /// Detects language of given text using free Google Translation API.
        /// </summary>
        /// <param name="text">Text the language of which you want to detect</param>
        public override string Detect(string text)
        {
            try
            {
                string requestString = String.Format("&target=en&q={0}", HttpUtility.UrlEncode(text));

                var translation = GetGoogleResponse(requestString);

                return translation.DetectedSourceLanguage;
            }
            catch (Exception ex)
            {
                EventLog.EventLogProvider.LogException("GoogleTranslator", "Detect", ex);
                return null;
            }
        }


        /// <summary>
        /// Not supported by Google Translation API.
        /// </summary>
        /// <param name="text">Text to speech</param>
        /// <param name="lang">Language in which the speech should be generated (if null or empty, automatic detection is used)</param>
        public override SystemIO.Stream Speak(string text, string lang)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Checks if everything required to run the service is in the settings of the service.
        /// </summary>
        public override bool IsAvailable()
        {
            return !string.IsNullOrEmpty(GoogleTranslateAPIKey) && CheckConnection();
        }

        #endregion


        #region "Helper methods"
        
        /// <summary>
        /// Checks if the credentials are set correctly.
        /// </summary>
        private bool CheckConnection()
        {
            if (mIsAvailable.HasValue)
            {
                return mIsAvailable.Value;
            }

            try
            {
                // Check if connection is correct
                mIsAvailable = GetGoogleResponse("&target=en&q=") != null;
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                mIsAvailable = false;
            }

            return mIsAvailable.Value;
        }


        /// <summary>
        /// Returns only first part of a culture code string (i.e. it returns "cs" from "cs-CZ").
        /// </summary>
        /// <param name="cultureCode">Culture code to process</param>
        private static string GetLangCode(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return cultureCode;
            }

            // Chinese cultures (starts with "zh") are special case
            if (cultureCode.StartsWithCSafe("zh", true))
            {
                return cultureCode;
            }

            int index = cultureCode.IndexOf('-');
            if (index <= 0)
            {
                return cultureCode;
            }

            return cultureCode.Substring(0, index);
        }


        /// <summary>
        /// Contacts the service with given URL and returns serialized response object.
        /// </summary>
        /// <param name="requestString">Service request URL</param>
        private GoogleTranslatorTranslation GetGoogleResponse(string requestString)
        {
            string baseRequestUrl = String.Format("https://www.googleapis.com/language/translate/v2?key={0}{1}", HttpUtility.UrlEncode(GoogleTranslateAPIKey), requestString);

            // Create and initialize the request.
            var request = (HttpWebRequest)WebRequest.Create(baseRequestUrl);

            // Get the response from the server
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            var serializer = new DataContractJsonSerializer(typeof(GoogleTranslatorWrapper));
            var translation = (GoogleTranslatorWrapper)serializer.ReadObject(stream);

            if ((translation == null) || (translation.Data == null) || (translation.Data.Translations == null))
            {
                throw new NullReferenceException(String.Format("Google Translator did not return any data for a request '{0}'.", baseRequestUrl));
            }

            return translation.Data.Translations[0];
        }

        #endregion


        #region "Helper classes"

        /// <summary>
        /// Wrapper class for Google Translator service response.
        /// </summary>
        [DataContract]
        private class GoogleTranslatorWrapper
        {
            /// <summary>
            /// Token type.
            /// </summary>
            [DataMember(Name = "data")]
            public GoogleTranslatorData Data
            {
                get;
                set;
            }
        }


        /// <summary>
        /// Translator service token encapsulation class.
        /// </summary>
        [DataContract(Name = "data")]
        private class GoogleTranslatorData
        {
            #region "Properties"

            /// <summary>
            /// Access token value.
            /// </summary>
            [DataMember(Name = "translations")]
            public GoogleTranslatorTranslation[] Translations
            {
                get;
                set;
            }

            #endregion
        }


        /// <summary>
        /// Wrapper for translations node in Google Translator response.
        /// </summary>
        [DataContract(Name = "translations")]
        private class GoogleTranslatorTranslation
        {
            /// <summary>
            /// Translated text.
            /// </summary>
            [DataMember(Name = "translatedText")]
            public string TranslatedText
            {
                get;
                set;
            }


            /// <summary>
            /// Detected language of the text.
            /// </summary>
            [DataMember(Name = "detectedSourceLanguage")]
            public string DetectedSourceLanguage
            {
                get;
                set;
            }
        }

        #endregion
    }
}
