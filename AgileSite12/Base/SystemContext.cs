using System;
using System.Globalization;
using System.IO;
using System.ComponentModel;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Environment variables
    /// </summary>
    public class SystemContext : AbstractContext<SystemContext>
    {
        #region "Variables"

        internal const string AUTO_EXTERNAL_WEBAPP_SUFFIX = "AutoExternalWeb";

        /// <summary>
        /// English culture.
        /// </summary>
        public static CMSLazy<CultureInfo> EnglishCulture = new CMSLazy<CultureInfo>(() => new CultureInfo("en-us", false));

        private static bool? mIsPrecompiledWebsite;
        private static bool? mDevelopmentMode;
        private static bool? mDiagnosticLogging;
        private static string mMachineName;
        private static string mServerName;
        private static string mInstanceName;
        private static bool? mIsWebApplicationProject;
        private static string mWebApplicationPhysicalPath;
        private static string mApplicationPath;
        private static string mInstanceNameSuffix;

        private static bool? mIsRunningOnAzure;
        

        /// <summary>
        /// True if web project is compiled in debug mode.
        /// </summary>
        public static bool IsWebProjectDebug = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether hosting environment is CMS web-based application (is not running as a part of external application).
        /// </summary>
        /// <remarks>
        /// This property should be used only internally.
        /// </remarks>
        public static bool IsCMSRunningAsMainApplication
        {
            get;
            internal set;
        }


        /// <summary>
        /// Returns true if web application is installed (instead of web project).
        /// </summary>
        [RegisterColumn]
        public static bool IsWebApplicationProject
        {
            get
            {
                if (mIsWebApplicationProject == null)
                {
#pragma warning disable BH1014 // Do not use System.IO
                    mIsWebApplicationProject = Directory.Exists(WebApplicationPhysicalPath + "\\Old_App_Code");
#pragma warning restore BH1014 // Do not use System.IO
                }

                return mIsWebApplicationProject.Value;
            }
            internal set
            {
                mIsWebApplicationProject = value;
            }
        }


        /// <summary>
        /// Indicates whether current instance is an Azure Cloud Services deployment (is false for Azure Web Apps).
        /// </summary>
        [RegisterColumn]
        public static bool IsRunningOnAzure
        {
            get
            {
                if (mIsRunningOnAzure == null)
                {
                    mIsRunningOnAzure = CoreServices.AppSettings["CMSAzureProject"].ToBoolean(false);
                }

                return mIsRunningOnAzure.Value;
            }
            internal set
            {
                mIsRunningOnAzure = value;
            }
        }


        /// <summary>
        /// Returns true if the website is precompiled (checks existence of codebehind file of PortalTemplate).
        /// </summary>
        [RegisterColumn]
        public static bool IsPrecompiledWebsite
        {
            get
            {
                if (mIsPrecompiledWebsite == null)
                {
#pragma warning disable BH1014 // Do not use System.IO
                    mIsPrecompiledWebsite = !File.Exists(WebApplicationPhysicalPath + "\\CMSPages\\PortalTemplate.aspx.cs");
#pragma warning restore BH1014 // Do not use System.IO
                }
                return mIsPrecompiledWebsite.Value;
            }
        }


        /// <summary>
        /// Server name.
        /// </summary>
        [RegisterColumn]
        public static string ServerName
        {
            get
            {
                return mServerName ?? (mServerName = CoreServices.AppSettings["CMSWebFarmServerName"].ToString(String.Empty));
            }
            set
            {
                mServerName = value;
            }
        }


        /// <summary>
        /// Machine name.
        /// </summary>
        [RegisterColumn]
        public static string MachineName
        {
            get
            {
                if (mMachineName == null)
                {
                    // Try to get from web.config
                    string name = CoreServices.AppSettings["CMSMachineName"];
                    if (name == null)
                    {
                        // Cache the machine name if available
                        try
                        {
                            name = Environment.MachineName;
                        }
                        catch
                        {
                            // Add web farm server name if available
                            if (!String.IsNullOrEmpty(ServerName))
                            {
                                name += "(" + ServerName + ")";
                            }
                            else
                            {
                                return "UNKNOWN";
                            }
                        }
                    }

                    mMachineName = name;
                }

                return mMachineName;
            }
            set
            {
                mMachineName = value;
            }
        }


        /// <summary>
        /// Returns unique name of the instance, i.e. ServerName for multi-instance environments or MachineName with ApplicationPath in other cases.
        /// </summary>
        [RegisterColumn]
        public static string InstanceName
        {
            get
            {
                return mInstanceName ?? (mInstanceName = (!String.IsNullOrEmpty(ServerName) ? ServerName : GenerateUniqueInstanceName()));
            }
        }


        /// <summary>
        /// Web application path (e.g. /WebProject1)
        /// </summary>
        [RegisterColumn]
        public static string ApplicationPath
        {
            get => mApplicationPath ?? (mApplicationPath = "/");
            set => mApplicationPath = value;
        }


        /// <summary>
        /// Returns the application virtual path.
        /// </summary>
        [RegisterColumn]
        public static string WebApplicationVirtualPath
        {
            get
            {
                return "~/";
            }
        }


        /// <summary>
        /// Web application physical path. Trimmed backslash '\' at the end. E.g. c:\inetpub\wwwroot\MySite
        /// </summary>
        [RegisterColumn]
        public static string WebApplicationPhysicalPath
        {
            get => mWebApplicationPhysicalPath ?? (mWebApplicationPhysicalPath = AppDomain.CurrentDomain.BaseDirectory?.TrimEnd('\\'));
            set => mWebApplicationPhysicalPath = value;
        }


        /// <summary>
        /// Indicates whether diagnostic information should be additionally logged to the event log.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        /// <exclude />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool DiagnosticLogging
        {
            get
            {
                if (DevelopmentMode)
                {
                    return true;
                }

                if (mDiagnosticLogging == null)
                {
                    mDiagnosticLogging = CoreServices.AppSettings["CMSDiagnosticLogging"].ToBoolean(false);
                }

                return mDiagnosticLogging.Value;
            }
        }


        /// <summary>
        /// Development mode type.
        /// </summary>
        [RegisterColumn(Hidden = true)]
        public static bool DevelopmentMode
        {
            get
            {
                if (mDevelopmentMode == null)
                {
                    mDevelopmentMode = CoreServices.AppSettings["CMSDevelopmentMode"].ToBoolean(false);
                }
                return mDevelopmentMode.Value;
            }
            set
            {
                mDevelopmentMode = value;
            }
        }


        /// <summary>
        /// Indicates if WebApplicationPhysicalPath property should be used to initialize the configuration manager.
        /// </summary>
        public static bool UseWebApplicationConfiguration
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the application runs on a web server, or as part of some other tool (e.g. Console application).
        /// </summary>
        /// <remarks>
        /// <para>
        /// To distinguish a web site from a web application, <see cref="IsWebApplicationProject"/> is suitable.
        /// </para>
        /// </remarks>
        [RegisterColumn]
        public static bool IsWebSite
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the suffix used for <see cref="GenerateUniqueInstanceName"/>.
        /// </summary>
        /// <remarks>
        /// Suffix is optional and helps distinguish applications on the same machine. 
        /// Suffix is added automatically when not defined in application config and application is considered as web application but not main application.
        /// </remarks>
        private static string InstanceNameSuffix
        {
            get
            {
                if (mInstanceNameSuffix == null)
                {
                    var instanceNameSuffix = String.Empty;
                    var customInstancenameSuffix = Convert.ToString(CoreServices.AppSettings["CMSInstanceNameSuffix"]);

                    if (!IsCMSRunningAsMainApplication && IsWebSite && customInstancenameSuffix == null)
                    {
                        customInstancenameSuffix = AUTO_EXTERNAL_WEBAPP_SUFFIX;
                    }

                    if (!String.IsNullOrEmpty(customInstancenameSuffix))
                    {
                        instanceNameSuffix = $"_{customInstancenameSuffix}";
                    }

                    mInstanceNameSuffix = instanceNameSuffix;
                }
                return mInstanceNameSuffix;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the automatic server name that is using for current instance
        /// </summary>
        public static string GenerateUniqueInstanceName()
        {
            return CoreServices.Conversion.GetCodeName(MachineName + ApplicationPath) + InstanceNameSuffix;
        }
        
        #endregion
    }
}
