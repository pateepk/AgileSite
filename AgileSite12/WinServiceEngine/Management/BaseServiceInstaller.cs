using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Base class for windows service installer.
    /// </summary>
    [RunInstaller(true)]
    public class BaseServiceInstaller : Installer
    {
        #region "Variables"

        private ServiceInstaller mServiceInstaller = null;
        private ServiceProcessInstaller mServiceProcessInstaller = null;
        private WinServiceItem mServiceDefinition = null;
        private bool disposed;

        #endregion


        #region "Properties"

        /// <summary>
        /// Service installer.
        /// </summary>
        protected ServiceInstaller ServiceInstaller
        {
            get
            {
                if (mServiceInstaller == null)
                {
                    mServiceInstaller = new ServiceInstaller();

                    mServiceInstaller.StartType = ServiceStartMode.Automatic;
                    mServiceInstaller.DelayedAutoStart = true;
                    mServiceInstaller.ServiceName = "KenticoCMSService";
                    mServiceInstaller.DisplayName = "Kentico CMS Service";
                    mServiceInstaller.Description = "Service description.";
                }

                return mServiceInstaller;
            }
        }


        /// <summary>
        /// Service process installer.
        /// </summary>
        protected ServiceProcessInstaller ServiceProcessInstaller
        {
            get
            {
                if (mServiceProcessInstaller == null)
                {
                    mServiceProcessInstaller = new ServiceProcessInstaller();

                    mServiceProcessInstaller.Account = ServiceAccount.NetworkService;
                    mServiceProcessInstaller.Username = null;
                    mServiceProcessInstaller.Password = null;
                    mServiceProcessInstaller.Committed += SetRestartServiceAclPermissions;
                }

                return mServiceProcessInstaller;
            }
        }


        /// <summary>
        /// Service definition.
        /// </summary>
        protected WinServiceItem ServiceDefinition
        {
            get
            {
                if (mServiceDefinition == null)
                {
                    mServiceDefinition = WinServiceHelper.GetServiceDefinition(BaseName);
                    if (mServiceDefinition == null)
                    {
                        // Missing service definition
                        throw new InstallException(string.Format("Service Installer - Missing '{0}' service definition.", BaseName));
                    }
                }

                return mServiceDefinition;
            }
        }


        /// <summary>
        /// Service assembly path.
        /// </summary>
        protected string ServiceAssemblyPath
        {
            get;
            set;
        }


        /// <summary>
        /// Full path of the web application.
        /// </summary>
        protected string WebApplicationPath
        {
            get;
            set;
        }


        /// <summary>
        /// Service base name.
        /// </summary>
        public string BaseName
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="baseServiceName">Service base name.</param>
        public BaseServiceInstaller(string baseServiceName)
            : base()
        {
            BaseName = baseServiceName;

            Installers.Add(ServiceProcessInstaller);
            Installers.Add(ServiceInstaller);
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Installs service.
        /// </summary>
        /// <param name="stateSaver">Dictionary with the state of the computer</param>
        public override void Install(IDictionary stateSaver)
        {
            if (CheckParameters())
            {
                SetServiceParameters();

                base.Install(stateSaver);
            }
            else
            {
                DisplayHelp();
            }
        }


        /// <summary>
        /// Uninstalls service.
        /// </summary>
        /// <param name="savedState">Dictionary with the state of the computer</param>
        public override void Uninstall(IDictionary savedState)
        {
            if (CheckParameters())
            {
                SetServiceParameters();

                base.Uninstall(savedState);
            }
            else
            {
                DisplayHelp();
            }
        }


        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="BaseServiceInstaller" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resource, false to release only managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                mServiceInstaller?.Dispose();
                mServiceProcessInstaller?.Dispose();
            }

            disposed = true;

            base.Dispose(disposing);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Gets help text message.
        /// </summary>
        protected virtual string GetHelpText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(@"USAGE: InstallUtil /i /logtoconsole=true|false /WEBPATH=<\""webpath \""> Service.exe");
            sb.AppendLine();
            sb.AppendLine("WHERE: /i             means to install the service contained in the EXE file and described by the following parameters.");
            sb.AppendLine("       /logtoconsole  is true or false to control displaying the full output of the installer.");
            sb.AppendLine("       <webpath>      is path of web application.");
            sb.AppendLine("       Service.exe    is the full path and file name of the new service you want to install.");
            sb.AppendLine();
            sb.AppendLine("EXAMPLE:");
            sb.AppendLine(@"InstallUtil /i /logtoconsole=false /WEBPATH=\""C:\inetpub\www\MyWeb\"" C:\WindowsServices\Service.exe");
            sb.AppendLine();

            return sb.ToString();
        }


        /// <summary>
        /// Checks base input parameters from command line (Service assembly path and Web application path).
        /// </summary>
        protected virtual bool CheckParameters()
        {
            // Get default parameters
            ServiceAssemblyPath = Context.Parameters["assemblypath"];
            WebApplicationPath = Context.Parameters["webpath"];

            if (string.IsNullOrEmpty(ServiceAssemblyPath) || string.IsNullOrEmpty(WebApplicationPath))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Sets parameters of service.
        /// </summary>
        private void SetServiceParameters()
        {
            // Set web application physical path
            SystemContext.WebApplicationPhysicalPath = WebApplicationPath;

            // Use web application settings
            SystemContext.UseWebApplicationConfiguration = true;

            // Initialize assebly path parameter
            // Framework 2.0
            if (Environment.Version.Major == 2)
            {
                // Set path + parameter (web path)
                ServiceAssemblyPath += "\"" + " \"" + ServiceHelper.WEB_PATH_PREFIX + WebApplicationPath;
            }
            // Other frameworks
            else
            {
                // Set path + parameter (web path)
                ServiceAssemblyPath += " \"" + ServiceHelper.WEB_PATH_PREFIX + WebApplicationPath + "\"";
            }

            Context.Parameters["assemblypath"] = ServiceAssemblyPath;
            SetServiceNameParameters();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">Message</param>
        private void Log(string message)
        {
            if (Context.IsParameterTrue("logtoconsole"))
            {
                Context.LogMessage(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }


        /// <summary>
        /// Displayes help text.
        /// </summary>
        private void DisplayHelp()
        {
            string text = GetHelpText();
            if (!string.IsNullOrEmpty(text))
            {
                Log(text);
            }

            // Throw error
            throw new InstallException(string.Format("Service Installer - Missing {0} service required parameters.", BaseName));
        }


        /// <summary>
        /// Initializes properties related to the service name.
        /// </summary>
        private void SetServiceNameParameters()
        {
            var configurationFilePath = Path.Combine(WebApplicationPath, "web.config");
            var settings = new ApplicationSettings(configurationFilePath);

            var applicationGuid = Guid.Empty;
            if (!Guid.TryParse(settings[SystemHelper.APP_GUID_KEY_NAME], out applicationGuid))
            {
                throw new Exception("The configuration file does not contain the application identifier.");
            }
            var applicationIdentifier = SystemHelper.FormatApplicationIdentifier(applicationGuid);
            var applicationName = SystemHelper.FormatApplicationName(settings[SystemHelper.APP_NAME_KEY_NAME], applicationGuid);

            ServiceInstaller.ServiceName = WinServiceHelper.FormatServiceName(ServiceDefinition.Name, applicationName, applicationIdentifier);
            ServiceInstaller.DisplayName = WinServiceHelper.FormatServiceDisplayName(ServiceDefinition.DisplayName, applicationName);
            ServiceInstaller.Description = ServiceDefinition.Description;
        }


        private void SetRestartServiceAclPermissions(object sender, InstallEventArgs e)
        {
            DiscretionaryAclHelper.SetServicePermissions(ServiceInstaller.ServiceName, "Network service");
            ServiceProcessInstaller.Committed -= SetRestartServiceAclPermissions;
        }

        #endregion
    }
}