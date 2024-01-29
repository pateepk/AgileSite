﻿using System;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CMS.EventLog;
using CMS.DataEngine;


namespace CMS.TranslationServices
{
    /// <summary>
    /// Class providing access to the Microsoft Translator API.
    /// </summary>
    public class MicrosoftTranslatorService : AbstractMachineTranslationService
    {
        private const string EVENT_SOUCRE_NAME = "MSTranslator";

        private static Dictionary<string, AzureAuthTokenClient> mClients;
        private bool? mIsAvailable;


        /// <summary>
        /// Hash table of clients indexed by site name.
        /// </summary>
        private static Dictionary<string, AzureAuthTokenClient> Clients
        {
            get
            {
                return mClients ?? (mClients = new Dictionary<string, AzureAuthTokenClient>());
            }
        }


        /// <summary>
        /// Subscription key from Translator Text API azure cognitive service <see href="http://docs.microsofttranslator.com/text-translate.html"/>.
        /// </summary>
        public string MSTranslatorTextAPISubscriptionKey
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteName + ".CMSMSTranslatorTextAPISubscriptionKey");
            }
        }


        /// <summary>
        /// Translates given text using Translator Text API azure cognitive service.
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

            return TranslateInternal(text, sourceLang, targetLang);
        }


        /// <summary>
        /// Detects language of given text using Translator Text API azure cognitive service.
        /// </summary>
        /// <param name="text">Text the language of which you want to detect</param>
        public override string Detect(string text)
        {
            return DetectInternal(text);
        }


        /// <summary>
        /// Returns stream of wav file generated by Translator Text API azure cognitive service.
        /// </summary>
        /// <param name="text">Text to speech</param>
        /// <param name="lang">Language in which the speech should be generated (if null or empty, automatic detection is used)</param>
        public override System.IO.Stream Speak(string text, string lang)
        {
            return SpeakInternal(text, lang);
        }


        /// <summary>
        /// Checks if everything required to run the service is in the settings of the service.
        /// </summary>
        public override bool IsAvailable()
        {
            return !string.IsNullOrEmpty(MSTranslatorTextAPISubscriptionKey) && CheckConnection();
        }


        /// <summary>
        /// Checks if it is possible to communicate with service.
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
                mIsAvailable = GetToken() != null;
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                mIsAvailable = false;
            }

            return mIsAvailable.Value;
        }


        /// <summary>
        /// Translates given text using Translator Text API azure cognitive service.
        /// </summary>
        /// <param name="text">Text you want to translate</param>
        /// <param name="sourceLang">Source language (if null or empty, automatic detection is used)</param>
        /// <param name="targetLang">Target language</param>
        private string TranslateInternal(string text, string sourceLang, string targetLang)
        {
            try
            {
                string requestString = "http://api.microsofttranslator.com/V2/Http.svc/Translate?" +
                    "Text=" + HttpUtility.UrlEncode(text) +
                    (string.IsNullOrEmpty(sourceLang) ? "" : "&From=" + HttpUtility.UrlEncode(sourceLang)) +
                    "&To=" + HttpUtility.UrlEncode(targetLang);

                return ContactService(requestString);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(EVENT_SOUCRE_NAME, "Translate", ex);
                return text;
            }
        }


        /// <summary>
        /// Detects language of given text using Translator Text API azure cognitive service.
        /// </summary>
        /// <param name="text">Text the language of which you want to detect</param>
        private string DetectInternal(string text)
        {
            try
            {
                string requestString = "http://api.microsofttranslator.com/V2/Http.svc/Detect?Text=" + HttpUtility.UrlEncode(text);

                return ContactService(requestString);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(EVENT_SOUCRE_NAME, "Detect", ex);
                return null;
            }
        }


        /// <summary>
        /// Returns stream of wav file generated using Translator Text API azure cognitive service.
        /// </summary>
        /// <param name="text">Text to speech</param>
        /// <param name="lang">Language in which the speech should be generated (if null or empty, automatic detection is used)</param>
        private System.IO.Stream SpeakInternal(string text, string lang)
        {
            string requestString = "http://api.microsofttranslator.com/V2/Http.svc/Speak?Text=" + HttpUtility.UrlEncode(text);
            if (!string.IsNullOrEmpty(lang))
            {
                requestString += "&Language=" + HttpUtility.UrlEncode(lang);
            }

            // Get the response from the server
            try
            {
                var request = BuildRequest(requestString);
                var response = (HttpWebResponse)request.GetResponse();

                return response.GetResponseStream();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(EVENT_SOUCRE_NAME, "Service", ex);
            }

            return null;
        }


        /// <summary>
        /// Contacts the service with given URL using the OAuth authorization method and returns the response.
        /// </summary>
        /// <param name="requestString">Service request URL</param>
        private string ContactService(string requestString)
        {
            var request = BuildRequest(requestString);

            // Get the response from the server
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var serializer = new DataContractSerializer(typeof(string));
            return (string)serializer.ReadObject(stream);
        }


        /// <summary>
        /// Builds the request to the service
        /// </summary>
        /// <param name="requestString">Request URL</param>
        private HttpWebRequest BuildRequest(string requestString)
        {
            // Create and initialize the request.
            var request = (HttpWebRequest)WebRequest.Create(requestString);

            var token = GetToken();

            // Add header
            if (token != null)
            {
                request.Headers.Add("Authorization", token);
            }

            return request;
        }


        /// <summary>
        /// Creates the token for given service.
        /// </summary>
        private string GetToken()
        {
            var client = GetAuthTokenClient();

            try
            {
                return client.GetAccessToken();
            }
            catch (HttpRequestException)
            {
                switch (client.RequestStatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        EventLogProvider.LogEvent(EventType.ERROR, EVENT_SOUCRE_NAME, "GetToken", eventDescription: "Request to token service is not authorized (401). Check that the Azure subscription key is valid.");
                        break;
                    case HttpStatusCode.Forbidden:
                        EventLogProvider.LogEvent(EventType.ERROR, EVENT_SOUCRE_NAME, "GetToken", eventDescription: "Request to token service is not authorized (403). For accounts in the free-tier, check that the account quota is not exceeded.");
                        break;
                }
                return null;
            }
        }


        /// <summary>
        /// Initialize a client for token management.
        /// </summary>
        /// <returns></returns>
        private AzureAuthTokenClient GetAuthTokenClient()
        {
            AzureAuthTokenClient client;
            
            if (!Clients.TryGetValue(SiteName, out client))
            {
                client = new AzureAuthTokenClient(MSTranslatorTextAPISubscriptionKey);
                Clients[SiteName] = client;
            }
            return client;
        }
    }
}