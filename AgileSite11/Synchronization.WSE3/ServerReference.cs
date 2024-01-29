using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;

using CMS.Base;
using CMS.Helpers;
using CMS.Synchronization.WSE3.Properties;

using Microsoft.Web.Services3;

#pragma warning disable 1591

namespace CMS.Synchronization.WSE3.Server
{
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [WebServiceBinding(Name = "SyncServerSoap", Namespace = "http://localhost/SyncWebService/SyncServer")]
    public partial class SyncServerWse : WebServicesClientProtocol
    {
        #region "Variables"

        private SendOrPostCallback ProcessSynchronizationTaskOperationCompleted;
        private bool useDefaultCredentialsSetExplicitly;

        #endregion


        #region "Events"

        public event ProcessSynchronizationTaskCompletedEventHandler ProcessSynchronizationTaskCompleted;

        #endregion


        #region "Properties"

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if (((IsLocalFileSystemWebService(base.Url)
                      && (useDefaultCredentialsSetExplicitly == false))
                     && (IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }


        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                useDefaultCredentialsSetExplicitly = true;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SyncServerWse()
        {
            Url = Settings.Default.CMS_SynchronizationEngine_SyncServer;
            if (IsLocalFileSystemWebService(Url))
            {
                UseDefaultCredentials = true;
                useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                useDefaultCredentialsSetExplicitly = true;
            }

            // Ensure certificate validation
            SecurityHelper.EnsureCertificateSecurity();
        }

        #endregion


        #region "Public Methods"
        
        /// <summary>
        /// Synchronizes StagingTaskData to target server.
        /// </summary>
        /// <param name="stagingTaskData">StagingTaskData to be synchronized</param>
        public string ProcessSynchronizationTaskData(IStagingTaskData stagingTaskData)
        {
            return ProcessSynchronizationTaskData(stagingTaskData.Serialize());
        }


        /// <summary>
        /// Synchronizes StagingTaskData to target server.
        /// </summary>
        /// <param name="stagingTaskData">Serialized StagingTaskData</param>
        [SoapDocumentMethod("http://localhost/SyncWebService/SyncServer/ProcessSynchronizationTaskData", RequestNamespace = "http://localhost/SyncWebService/SyncServer", ResponseNamespace = "http://localhost/SyncWebService/SyncServer", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public string ProcessSynchronizationTaskData(string stagingTaskData)
        {
            object[] results = Invoke("ProcessSynchronizationTaskData", new object[]
                                                          {
                                                              stagingTaskData
                                                          });

            return ((string)(results[0]));
        }


        /// <summary>
        /// Synchronizes StagingTaskData to target server in asynchronous manner.
        /// </summary>
        /// <param name="stagingTaskData"></param>
        /// <param name="userState"></param>
        public void ProcessSynchronizationTaskDataAsync(string stagingTaskData, object userState)
        {
            if ((ProcessSynchronizationTaskOperationCompleted == null))
            {
                ProcessSynchronizationTaskOperationCompleted = new SendOrPostCallback(OnProcessSynchronizationTaskOperationCompleted);
            }

            InvokeAsync("ProcessSynchronizationTaskData", new object[]
                                                          {
                                                              stagingTaskData
                                                          }, ProcessSynchronizationTaskOperationCompleted, userState);
        }


        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }

        #endregion


        #region "Private Methods"

        private bool IsLocalFileSystemWebService(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            Uri wsUri = new Uri(url);
            if (((wsUri.Port >= 1024)
                 && (CMSString.Compare(wsUri.Host, "localHost", StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }


        private void OnProcessSynchronizationTaskOperationCompleted(object arg)
        {
            if ((ProcessSynchronizationTaskCompleted != null))
            {
                InvokeCompletedEventArgs invokeArgs = ((InvokeCompletedEventArgs)(arg));
                ProcessSynchronizationTaskCompleted(this, new ProcessSynchronizationTaskCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        #endregion
    }


    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [WebServiceBinding(Name = "SyncServerSoap", Namespace = "http://localhost/SyncWebService/SyncServer")]
    public partial class SyncServer : SoapHttpClientProtocol
    {
        private bool useDefaultCredentialsSetExplicitly;


        public SyncServer()
        {
            Url = Settings.Default.CMS_SynchronizationEngine_SyncServer;
            if (IsLocalFileSystemWebService(Url))
            {
                UseDefaultCredentials = true;
                useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                useDefaultCredentialsSetExplicitly = true;
            }
        }


        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if (((IsLocalFileSystemWebService(base.Url)
                      && (useDefaultCredentialsSetExplicitly == false))
                     && (IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }


        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }


        private bool IsLocalFileSystemWebService(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            Uri wsUri = new Uri(url);
            if (((wsUri.Port >= 1024)
                 && (CMSString.Compare(wsUri.Host, "localHost", StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }
    }


    public delegate void ProcessSynchronizationTaskCompletedEventHandler(object sender, ProcessSynchronizationTaskCompletedEventArgs e);


    [DebuggerStepThrough]
    [DesignerCategory("code")]
    public partial class ProcessSynchronizationTaskCompletedEventArgs : AsyncCompletedEventArgs
    {
        private object[] results;


        internal ProcessSynchronizationTaskCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState)
            : base(exception, cancelled, userState)
        {
            this.results = results;
        }


        public string Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return ((string)(results[0]));
            }
        }
    }
}

#pragma warning restore 1591