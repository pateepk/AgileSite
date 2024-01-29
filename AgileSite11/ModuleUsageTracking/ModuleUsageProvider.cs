using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;

using Newtonsoft.Json;

namespace CMS.ModuleUsageTracking
{
    using Module = ModuleUsageTrackingModule;

    /// <summary>
    /// Processes statistical data collected from module data providers.
    /// </summary>
    internal class ModuleUsageProvider
    {
        #region "Constants"

        private const string ENDPOINT_ADDRESS = "https://usage.kentico.com/";
        private const int REQUEST_TIMEOUT = 300000;

        #endregion


        #region "Variables"

        private Uri mEndPoint;
        private IModuleUsageDataSourceContainer mContainer;

        private readonly DateTime mSendTime = DateTime.UtcNow;
        private readonly HashSet<string> mSentFields = new HashSet<string>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Container providing module usage data sources
        /// </summary>
        protected IModuleUsageDataSourceContainer Container
        {
            get
            {
                return mContainer ?? (mContainer = ObjectFactory<IModuleUsageDataSourceContainer>.StaticSingleton());
            }
        }


        /// <summary>
        /// Module usage server endpoint 
        /// </summary>
        protected Uri EndPoint
        {
            get
            {
                return mEndPoint ?? (mEndPoint = new Uri(ValidationHelper.GetString(SettingsHelper.AppSettings["CMSModuleUsageEndpoint"], ENDPOINT_ADDRESS)));
            }
        }


        /// <summary>
        /// Collection of names of all data fields obtained from datasources and sent to server.
        /// Names of sent fields are logged as information into event log after send process is finished.
        /// </summary>
        protected HashSet<string> SentFields
        {
            get
            {
                return mSentFields;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Hidden constructor
        /// </summary>
        protected ModuleUsageProvider()
        {
        }

        #endregion


        #region "Data methods"

        /// <summary>
        /// Process data (for example send data to server).
        /// </summary>
        internal static void ProcessData()
        {
            LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => new ModuleUsageProvider().ProcessDataInternal());
        }


        /// <summary>
        /// Process data (for example send data to server).
        /// Disables license check.
        /// </summary>
        [CanDisableLicenseCheck("CVjNHWuLx7TeAx6oAs+smVGnbaRwPzMlASp/IYVXgtW5nPGGD4ke2Y0SbsxQmVSsTH10RVqZID4vZqcA+UK3TA==")]
        protected void ProcessDataInternal()
        {
            if (Module.Initialized)
            {
                SendData(PrepareData());
                LogSentFields();
            }
        }


        /// <summary>
        /// Enhances the raw data with metadata.
        /// </summary>
        protected IEnumerable<ModuleUsageData> PrepareData()
        {
            // Collect data from data providers
            foreach (var source in Container.GetDataSources())
            {
                ModuleUsageData moduleData;

                try
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();
                    var moduleInfo = source.GetData();
                    stopWatch.Stop();

                    moduleData = new ModuleUsageData(
                        mSendTime,
                        Module.Identity.ToString(),
                        source.Name,
                        CMSVersion.MainVersion,
                        stopWatch.ElapsedMilliseconds,
                        moduleInfo);

                    SentFields.UnionWith(moduleInfo.Select(record => source.Name + "." + record.Key));
                }
                catch (Exception ex)
                {
                    LogException("PREPAREDATA", ex);

                    moduleData = GetErrorData(source.Name);
                }

                yield return moduleData;
            }
        }


        /// <summary>
        /// Generate data that will be sent to server in case of error.
        /// </summary>
        /// <param name="dataSourceName">Name of the data source that produced the error. When null error source will be flagged as general.</param>
        protected ModuleUsageData GetErrorData(string dataSourceName = null)
        {
            return new ModuleUsageData(
               mSendTime,
               Module.Identity.ToString(),
               "Error." + (dataSourceName ?? "General"),
               CMSVersion.MainVersion,
               0,
               ObjectFactory<IModuleUsageDataCollection>.New());
        }


        /// <summary>
        /// Writes information about sent fields to event log.
        /// </summary>
        protected void LogSentFields()
        {
            var builder = new StringBuilder();
            builder.Append("Kentico improvement program has sent the following data:").Append(Environment.NewLine);
            SentFields.Aggregate(builder, (seed, field) => seed.Append(field).Append(Environment.NewLine));

            using (new CMSActionContext { LogEvents = true })
            {
                CoreServices.EventLog.LogEvent(EventType.INFORMATION, Module.MODULE_USAGE_EVENTS_SOURCE, "SENDDATA", builder.ToString());
            }
        }


        /// <summary>
        /// Writes exception to event log.
        /// </summary>
        protected void LogException(string code, Exception ex)
        {
            using (new CMSActionContext { LogEvents = true })
            {
                CoreServices.EventLog.LogException(Module.MODULE_USAGE_EVENTS_SOURCE, code, ex);
            }
        }


        /// <summary>
        /// Writes error to event log.
        /// </summary>
        protected void LogError(string code, string description)
        {
            using (new CMSActionContext { LogEvents = true })
            {
                CoreServices.EventLog.LogEvent(EventType.ERROR, Module.MODULE_USAGE_EVENTS_SOURCE, code, description);
            }
        }

        #endregion


        #region "Send & Http methods"

        /// <summary>
        /// Sends module usage data to server.
        /// </summary>
        /// <param name="moduleData">Data to sent</param>
        protected void SendData(IEnumerable<ModuleUsageData> moduleData)
        {
            try
            {
                // Send data
                using (var response = PostAsJson("api/ModuleUsage", moduleData))
                {
                    // Failure
                    if (((int)response.StatusCode < 200) || ((int)response.StatusCode >= 300))
                    {
                        ProcessHttpError(response);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        ProcessHttpError(response);
                    }
                }
                else
                {
                    ProcessHttpError(ex);
                }
            }
            catch (Exception ex)
            {
                LogException("SENDDATA", ex);

                PostAsJson("api/ModuleUsage", new[] { GetErrorData() }).Close();
            }
        }


        /// <summary>
        /// Sends given object to module usage server as json.
        /// </summary>
        /// <param name="relativeUriString">Relative uri the data should be sent to</param>
        /// <param name="data">Object to send</param>
        /// <returns>Returns http response object. Response object must be closed in code that called this method.</returns>
        protected HttpWebResponse PostAsJson(string relativeUriString, object data)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(EndPoint, relativeUriString));

            request.AllowAutoRedirect = true;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Timeout = REQUEST_TIMEOUT;

            var serializer = new JsonSerializer();

#pragma warning disable BH1014 // Do not use System.IO
            using (var jsonWriter = new JsonTextWriter(new StreamWriter(request.GetRequestStream())))
#pragma warning restore BH1014 // Do not use System.IO
            {
                serializer.Serialize(jsonWriter, data);
            }

            return (HttpWebResponse)request.GetResponse();
        }


        /// <summary>
        /// Processes http error that occurred in data transmition. Information about error is are logged and sent to server.
        /// </summary>
        /// <param name="response">Http response of request that failed</param>
        protected void ProcessHttpError(HttpWebResponse response)
        {
            string message = null;

            try
            {
                var stream = response.GetResponseStream();
                if (stream != null)
                {
#pragma warning disable BH1014 // Do not use System.IO
                    using (var reader = new StreamReader(stream))
#pragma warning restore BH1014 // Do not use System.IO
                    {
                        message = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                // Suppress the big variety of possible exceptions
            }

            LogError("SENDDATA", "Status code: " + (int)response.StatusCode + Environment.NewLine + message);

            PostAsJson("api/ModuleUsage", new[] { GetHttpErrorData(response.StatusCode) }).Close();
        }


        /// <summary>
        /// Processes http error that occured in data transmition. Information about error is are logged and sent to server.
        /// </summary>
        /// <param name="exception">Web exceptions throwed during transmition</param>
        protected void ProcessHttpError(WebException exception)
        {
            LogError("SENDDATA", "Status " + exception.Status + Environment.NewLine + exception.Message);

            PostAsJson("api/ModuleUsage", new[] { GetHttpErrorData(exception) }).Close();
        }


        /// <summary>
        /// Generate data that will be sent to server in case of http error.
        /// </summary>
        /// <param name="statusCode">Http response status code</param>
        protected ModuleUsageData GetHttpErrorData(HttpStatusCode statusCode)
        {
            // Create information about the timeout
            var errorInfo = ObjectFactory<IModuleUsageDataCollection>.New();

            errorInfo.Add("StatusCode", (int)statusCode);
            errorInfo.Add("Status", statusCode.ToString());

            return CreateHttpErrorData(errorInfo);
        }


        /// <summary>
        /// Generate data that will be sent to server in case of http error.
        /// </summary>
        /// <param name="exception">Web exception describing http error</param>
        protected ModuleUsageData GetHttpErrorData(WebException exception)
        {
            // Create information about the timeout
            var errorInfo = ObjectFactory<IModuleUsageDataCollection>.New();

            errorInfo.Add("Status", exception.Status.ToString());

            return CreateHttpErrorData(errorInfo);
        }


        /// <summary>
        /// Creates module usage data object flagged to carry http error data collection.
        /// </summary>
        /// <param name="data">Data collection with data describing http error</param>
        private ModuleUsageData CreateHttpErrorData(IModuleUsageDataCollection data)
        {
            return new ModuleUsageData(
               mSendTime,
               Module.Identity.ToString(),
               "Error.Http",
               CMSVersion.MainVersion,
               0,
               data);
        }

        #endregion
    }
}
