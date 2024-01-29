using System;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Security;
using System.Web;
using System.Web.Hosting;

using CMS.Core;
using CMS.Base;

// Ensures the IsWebSite flag
[assembly: PreApplicationStartMethod(typeof(SystemContext), "SetIsWebSiteFlag")]

namespace CMS.Base
{
    /// <summary>
    /// Environment variables
    /// </summary>
    public class SystemContext : AbstractContext<SystemContext>
    {
        #region "Variables"

        /// <summary>
        /// English culture.
        /// </summary>
        public static CMSLazy<CultureInfo> EnglishCulture = new CMSLazy<CultureInfo>(() => new CultureInfo("en-us", false));

        // If true, the web site is precompiled
        private static bool? mIsPrecompiledWebsite;
                       

        // Application physical path.
        private static string mWebApplicationPhysicalPath;

        // Development mode.
        private static bool? mDevelopmentMode;

        // Current machine name.
        private static string mMachineName;

        // Current server name.
        private static string mServerName;

        // Unique instance name
        private static string mInstanceName;

        // Application path.
        private static string mApplicationPath;

        // Returns true if web application is installed (instead of web project).
        private static bool? mIsWebApplicationProject;

        // Trust level.
        private static AspNetHostingPermissionLevel mCurrentTrustLevel = AspNetHostingPermissionLevel.None;

        // Indicates if application run in full trust level
        private static bool? mIsFullTrust;


        private static bool? mIsRunningOnAzure;
        private static string mApplicationPoolName;
        private static string mIISWebSiteName;

        /// <summary>
        /// True if the assemblies are compiled in debug mode.
        /// </summary>
#if DEBUG
        public const bool IsAssemblyDebug = true;
#else
        public const bool IsAssemblyDebug = false;
#endif


        /// <summary>
        /// True if web project is compiled in debug mode.
        /// </summary>
        public static bool IsWebProjectDebug = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the application is terminating
        /// </summary>
        public static bool ApplicationTerminating
        {
            get
            {
                return Environment.HasShutdownStarted || (HostingEnvironment.ShutdownReason != ApplicationShutdownReason.None);
            }
        }
        
        
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
        /// Sets value indicating whether current instance running on Azure.
        /// </summary>
        internal static bool? IsRunningOnAzureInternal
        {
            set
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
                        if (HttpContext.Current != null)
                        {
                            name = HttpContext.Current.Server.MachineName;
                        }
                        else
                        {
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
            get
            {
                if (mApplicationPath == null)
                {
                    if (IsWebSite)
                    {
                        mApplicationPath = HttpRuntime.AppDomainAppVirtualPath;
                    }
                    else
                    {
                        return "/";
                    }
                }

                return mApplicationPath;
            }
            set
            {
                mApplicationPath = value;
            }
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
            get
            {
                if (mWebApplicationPhysicalPath == null)
                {
                    var path = IsWebSite ? HttpRuntime.AppDomainAppPath : AppDomain.CurrentDomain.BaseDirectory;

                    WebApplicationPhysicalPath = path.TrimEnd('\\');
                }

                return mWebApplicationPhysicalPath;
            }
            set
            {
                mWebApplicationPhysicalPath = value;
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
        /// The web site is automatically detected during application start (<see cref="SetIsWebSiteFlag"/> initialization method is called).
        /// </para>
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
        /// Sets the <see cref="IsWebSite"/> flag to true.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method is not intended to be used directly from your code.
        /// </para>
        /// <para>
        /// Method is called automatically during the application startup for applications hosted within the web server.
        /// </para>
        /// </remarks>
        [Obsolete("This code is meant for system purposes, it shouldn't be used directly.", true)]
        public static void SetIsWebSiteFlag()
        {
            IsWebSite = true;
        }


        /// <summary>
        /// Current trust level.
        /// </summary>
        [RegisterColumn]
        public static AspNetHostingPermissionLevel CurrentTrustLevel
        {
            get
            {
                if (mCurrentTrustLevel == AspNetHostingPermissionLevel.None)
                {
                    mCurrentTrustLevel = GetTrustLevel();
                }

                return mCurrentTrustLevel;
            }
        }


        /// <summary>
        /// Current application pool name
        /// </summary>
        [RegisterColumn]
        public static string ApplicationPoolName
        {
            get
            {
                return mApplicationPoolName ?? (mApplicationPoolName = GetApplicationPoolName());
            }
        }


        /// <summary>
        /// Current IIS web site name
        /// </summary>
        [RegisterColumn]
        public static string IISWebSiteName
        {
            get
            {
                return mIISWebSiteName ?? (mIISWebSiteName = GetIISWebSiteName());
            }
        }


        /// <summary>
        /// Indicates if is full trust level of code access security.
        /// </summary>
        [RegisterColumn]
        public static bool IsFullTrustLevel
        {
            get
            {
                if (mIsFullTrust == null)
                {
                    mIsFullTrust = (CurrentTrustLevel == AspNetHostingPermissionLevel.Unrestricted);
                }

                return mIsFullTrust.Value;
            }
        }


        /// <summary>
        /// Returns true if the application runs in the integrated mode
        /// </summary>
        public static bool IsIntegratedMode
        {
            get
            {
                return HttpRuntime.UsingIntegratedPipeline;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the automatic server name that is using for current instance
        /// </summary>
        public static string GenerateUniqueInstanceName()
        {
            return CoreServices.Conversion.GetCodeName(MachineName + ApplicationPath);
        }
        

        /// <summary>
        /// Gets the current application trust level
        /// </summary>
        private static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!IsWebSite)
            {
                return AspNetHostingPermissionLevel.Unrestricted;
            }

            try
            {
            // Check the trust level by evaluation of levels
            foreach (AspNetHostingPermissionLevel trustLevel in new[]
            {
                    AspNetHostingPermissionLevel.Unrestricted,
                    AspNetHostingPermissionLevel.High,
                    AspNetHostingPermissionLevel.Medium,
                    AspNetHostingPermissionLevel.Low,
                    AspNetHostingPermissionLevel.Minimal
            })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (SecurityException)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
            }
            catch
            {
                return AspNetHostingPermissionLevel.None;
        }
        }
    

        /// <summary>
        /// Gets the application pool name.
        /// </summary>
        private static string GetApplicationPoolName()
        {
            try
            {
                string appPath = HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"].Replace("/LM/", "IIS://localhost/");

                var root = new DirectoryEntry(appPath);

                return root.Properties["AppPoolId"].Value.ToString();
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the name of the IIS web site
        /// </summary>
        private static string GetIISWebSiteName()
        {
            try
            {
                return HostingEnvironment.SiteName;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
