﻿using System;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

using CMS.EventLog;
using CMS.DataEngine;

using Newtonsoft.Json;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class providing access to the Microsoft Translator API.
    /// </summary>
    public class MicrosoftTranslatorService : AbstractMachineTranslationService
    {
        private const string EVENT_SOURCE_NAME = "MSTranslator";
        private const string BASE_URL = "https://api.cognitive.microsofttranslator.com/";

        private static Dictionary<string, AzureAuthTokenClient> mClients;
        private bool? mIsAvailable;


        /// <summary>
        /// Hash table of clients indexed by site name.
        /// </summary>
        private static Dictionary<string, AzureAuthTokenClient> Clients
            => mClients ?? (mClients = new Dictionary<string, AzureAuthTokenClient>());


        /// <summary>
        /// Subscription key from Translator Text API azure cognitive service <see href="https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-text-how-to-signup"/>.
        /// </summary>
        public string MSTranslatorTextAPISubscriptionKey
            => SettingsKeyInfoProvider.GetValue(SiteName + ".CMSMSTranslatorTextAPISubscriptionKey");


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
            throw new NotSupportedException();
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
            var queryString = "&from=" + (string.IsNullOrEmpty(sourceLang) ? HttpUtility.UrlEncode(Detect(text)) : HttpUtility.UrlEncode(sourceLang)) +
                    "&to=" + HttpUtility.UrlEncode(targetLang) +
                    "&textType=html";

            var config = ContactService<MicrosoftTranslatorTranslateResponse[]>("Translate", text, queryString);

            var result =  config?.FirstOrDefault()?
                    .Translations?.FirstOrDefault()?
                    .Text;

            if (string.IsNullOrEmpty(result))
            {
                return string.Empty;
            }

            return result;
        }


        /// <summary>
        /// Detects language of given text using Translator Text API azure cognitive service.
        /// </summary>
        /// <param name="text">Text the language of which you want to detect</param>
        private string DetectInternal(string text)
        {
            var config = ContactService<MicrosoftTranslatorDetectResponse[]>("Detect", text);

            var result = config?.FirstOrDefault()?.Language;

            if (string.IsNullOrEmpty(result))
            {
                return string.Empty;
            }

            return result;
        }


        /// <summary>
        /// Contacts the service using the OAuth authorization method and returns the deserialized JSON response.
        /// </summary>
        /// <typeparam name="ResponseType">Type used for deserialization of JSON response</typeparam>
        /// <param name="endpointName">Name of endpoint to be contacted</param>
        /// <param name="text">Text used in request body</param>
        /// <param name="queryString">Query string parameters for API endpoint</param>
        /// <returns>JSON response deserialized to <typeparamref name="ResponseType"/>.</returns>
        private ResponseType ContactService<ResponseType>(string endpointName, string text, string queryString = "")
        {
            string jsonResponse = null;
            try
            {
                var requestURL = GetBaseUrlFor(endpointName) + queryString;
                var request = GenerateRequest(requestURL, text);

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request).Result;
                    jsonResponse = response.Content.ReadAsStringAsync().Result;

                    return JsonConvert.DeserializeObject<ResponseType>(jsonResponse);
                }
            }
            catch (JsonSerializationException)
            {
                var errorResponse = JsonConvert.DeserializeObject<MicrosoftTranslatorErrorResponse>(jsonResponse);
                EventLogProvider.LogEvent(EventType.ERROR, EVENT_SOURCE_NAME, endpointName, eventDescription: errorResponse.Error.Message);
                return default(ResponseType);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(EVENT_SOURCE_NAME, endpointName, ex);
                return default(ResponseType);
            }
        }


        /// <summary>
        /// Generates request for translation API using URI and text to be translated.
        /// </summary>
        /// <param name="uriString">URI of translation API including necessary query strings</param>
        /// <param name="text">Text for request body</param>
        /// <returns>Complete request ready to be sent.</returns>
        private HttpRequestMessage GenerateRequest(string uriString, string text)
        {
            var request = new HttpRequestMessage();

            // Set the method to POST
            request.Method = HttpMethod.Post;

            // Construct the full URI
            request.RequestUri = new Uri(uriString);

            // Add the serialized JSON object to your request
            request.Content = new StringContent(GetRequestBody(text), Encoding.UTF8, "application/json");

            // Add the authorization header
            request.Headers.Add("Authorization", GetToken());

            return request;
        }


        /// <summary>
        /// Creates the token for given service.
        /// </summary>
        /// <returns>Authentication token.</returns>
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
                        EventLogProvider.LogEvent(EventType.ERROR, EVENT_SOURCE_NAME, "GetToken", eventDescription: "Request to token service is not authorized (401). Check that the Azure subscription key is valid.");
                        break;
                    case HttpStatusCode.Forbidden:
                        EventLogProvider.LogEvent(EventType.ERROR, EVENT_SOURCE_NAME, "GetToken", eventDescription: "Request to token service is not authorized (403). For accounts in the free-tier, check that the account quota is not exceeded.");
                        break;
                }
                return null;
            }
        }


        /// <summary>
        /// Initialize a client for token management.
        /// </summary>
        /// <returns>Initialized client for token management.</returns>
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


        /// <summary>
        /// Gets base URL for specified method.
        /// </summary>
        /// <param name="apiMethod">API method name</param>
        /// <returns>Base request URL for specified method.</returns>
        private string GetBaseUrlFor(string apiMethod)
        {
            return $"{ BASE_URL }{ apiMethod }?api-version=3.0";
        }


        /// <summary>
        /// Gets body for requests.
        /// </summary>
        /// <param name="text">Text specified in body</param>
        /// <returns>Body for request containing specified text.</returns>
        private string GetRequestBody(string text)
        {
            return JsonConvert.SerializeObject(
                new []
                {
                    new MicrosoftTranslatorRequest
                    {
                        Text = text
                    }
                });
        }
        private class MicrosoftTranslatorRequest
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        private class MicrosoftTranslatorTranslateResponse
        {
            [JsonProperty("translations")]
            public MicrosoftTranslatorTranslation[] Translations { get; set; }
        }

        private class MicrosoftTranslatorTranslation
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        private class MicrosoftTranslatorDetectResponse
        {
            [JsonProperty("language")]
            public string Language { get; set; }
        }        

        private class MicrosoftTranslatorErrorResponse
        {
            [JsonProperty("error")]
            public MicrosoftTranslatorError Error;
        }

        private class MicrosoftTranslatorError
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
