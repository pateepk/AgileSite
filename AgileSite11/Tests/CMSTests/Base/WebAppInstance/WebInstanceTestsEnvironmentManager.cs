using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;

using CMS.Helpers;

namespace CMS.Tests
{
    /// <summary>
    /// Class for managing test environment for <see cref="WebAppInstanceTests"/>.
    /// </summary>
    public class WebInstanceTestsEnvironmentManager : IWebInstanceTestsEnvironmentManager
    {
        #region "Private variables"

        const string APP_KEY_DELETE_INSTANCE = "CMSTestRecycleWebAppInstance";

        private static readonly Lazy<bool> mRecycleWebAppInstance = new Lazy<bool>(() => ValidationHelper.GetBoolean(TestsConfig.GetTestAppSetting(APP_KEY_DELETE_INSTANCE), true));

        private Process mIisProcess;
        private string mWebAppInstancePath;

        private readonly int mWebAppInstancePort = 8888;
        private readonly string mProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        private static readonly string[] FoldersToCopy =
        {
            "Admin",
            "App_Data",
            "App_Themes",
            "Bin",
            "CMSAdminControls",
            "CMSAPIExamples",
            "CMSDependencies",
            "CMSEdit",
            "CMSFormControls",
            "CMSInlineControls",
            "CMSInstall",
            "CMSMasterPages",
            "CMSMessages",
            "CMSModules",
            "CMSPages",
            "CMSResources",
            "CMSScripts",
            "CMSSiteUtils",
            "CMSTemplates",
            "CMSWebParts",
            "CommunitySite",
            "CorporateSite",
            "DancingGoat",
            "EcommerceSite",
            "IntranetPortal"
        };


        private static readonly string[] FilesToCopy =
        {
            "Default.aspx",
            "Default.aspx.cs",
            "favicon.ico",
            "Global.asax",
            "DocumentRESTService.svc",
            "ObjectTranslationRESTService.svc",
            "RESTService.svc"
        };

        
        private static readonly string[] FoldersToExclude =
        {
            @"app_data\cirepository",
            @"app_data\cmstemp"
        };


        private static readonly string[] FilesToDelete =
        {
            @"App_Data\CMSModules\ImportExport\dev_mode"
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of test web app instance.
        /// </summary>
        public string WebAppInstanceName
        {
            get;
            private set;
        }


        /// <summary>
        /// URL of test web app instance.
        /// </summary>
        public string WebAppInstanceUrl
        {
            get
            {
                return @"http://localhost:" + mWebAppInstancePort;
            }
        }


        /// <summary>
        /// Physical path of test web app instance.
        /// </summary>
        public string WebAppInstancePath
        {
            get
            {
                return mWebAppInstancePath ?? (mWebAppInstancePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Insts", WebAppInstanceName));
            }
        }


        private string OriginalInstanceFolder
        {
            get
            {
                return TestsConfig.GetTestAppSetting("CMSTestSourceWebInstancePath") ?? Path.Combine(TestsConfig.SolutionFolderPath, "CMS");
            }
        }


        private static bool RecycleWebAppInstance
        {
            get
            {
                return mRecycleWebAppInstance.Value;
            }
        }


        private static Func<string, bool> IncludePredicate
        {
            get
            {
                return i => !FoldersToExclude.Any(excl => (i.ToLowerInvariant() + '\\').Contains('\\' + excl + '\\'));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="instanceName">Name of web application used for tests.</param>
        /// <param name="port">Port to run the web application on.</param>
        public WebInstanceTestsEnvironmentManager(string instanceName, int port = 0)
        {
            WebAppInstanceName = instanceName;

            if (port > 0)
            {
                mWebAppInstancePort = port;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sets up test web app instance. 
        /// </summary>
        public void SetUp()
        {
            PrepareWebAppInstance();
            RegisterInstanceInIIS();
            EnsureIISProcess();
        }


        /// <summary>
        /// Cleans up test web app instance.
        /// </summary>
        public void CleanUp()
        {
            StopIIS();
            UnRegisterInstanceInIIS();

            if (RecycleWebAppInstance)
            {
                TestsDirectoryHelper.DeleteDirectoryIgnoreIOErrors(WebAppInstancePath);
            }
        }


        /// <summary>
        /// Ensures IIS Express process is running.
        /// </summary>
        public void EnsureIISProcess()
        {
            if ((mIisProcess == null) || mIisProcess.HasExited)
            {
                mIisProcess = new Process();
                mIisProcess.StartInfo.FileName = Path.Combine(mProgramFilesPath, "IIS Express\\iisexpress.exe");
                mIisProcess.StartInfo.Arguments = string.Format("/path:{0} /port:{1}", WebAppInstancePath, mWebAppInstancePort);
                mIisProcess.Start();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Copies files and folders to test web app instance directory.
        /// </summary>
        protected virtual void PrepareWebAppInstance()
        {
            // Delete test web app instance files and folders from previous tests run
            if (RecycleWebAppInstance)
            {
                TestsDirectoryHelper.DeleteDirectoryIgnoreIOErrors(WebAppInstancePath);
            }

            var dirInfo = new DirectoryInfo(WebAppInstancePath);
            dirInfo.Create();

            var dirSecurity = dirInfo.GetAccessControl();
            dirSecurity.AddAccessRule(new FileSystemAccessRule("Network Service", FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            dirInfo.SetAccessControl(dirSecurity);

            foreach (var folder in FoldersToCopy)
            {
                CopyFolderToWebAppInstance(folder);
            }

            foreach (var file in FilesToCopy)
            {
                CopyFileToWebAppInstance(file);
            }

            foreach (var file in FilesToDelete)
            {
                DeleteFile(file);
            }

            CreateWebConfig();
        }


        private void CopyFolderToWebAppInstance(string folderName)
        {
            var sourceFolder = Path.Combine(OriginalInstanceFolder, folderName);
            var destinationFolder = Path.Combine(WebAppInstancePath, folderName);

            if (!Directory.Exists(sourceFolder))
            {
                return;
            }

            Directory.CreateDirectory(destinationFolder);

            // Copy folder structure
            foreach (var sourceSubFolder in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories).Where(IncludePredicate))
            {
                Directory.CreateDirectory(sourceSubFolder.Replace(sourceFolder, destinationFolder));
            }

            // Copy files
            foreach (var sourceFile in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories).Where(IncludePredicate))
            {
                var destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);

                // We expect that majority of files doesn't change, so we don't want to overwrite them
                if (RecycleWebAppInstance || !File.Exists(destinationFile) || !CompareFiles(sourceFile, destinationFile))
                {
                    File.Copy(sourceFile, destinationFile, true);
                }
            }
        }


        private bool CompareFiles(string sourceFile, string destinationFile)
        {
            var source = new FileInfo(sourceFile);
            var dest = new FileInfo(destinationFile);

            if (source.Length != dest.Length)
            {
                return false;
            }

            var sourceBinary = File.ReadAllBytes(sourceFile);
            var destBinary = File.ReadAllBytes(destinationFile);

            if (sourceBinary.Length != destBinary.Length)
            {
                return false;
            }

            for (var i = 0; i < sourceBinary.Length; i++)
            {
                if (sourceBinary[i] != destBinary[i])
                {
                    return false;
                }
            }

            return true;
        }


        private void CopyFileToWebAppInstance(string fileName)
        {
            var sourceFile = Path.Combine(OriginalInstanceFolder, fileName);

            if (!File.Exists(sourceFile))
            {
                return;
            }

            var destinationFile = sourceFile.Replace(OriginalInstanceFolder, WebAppInstancePath);
            File.Copy(sourceFile, destinationFile, true);
        }


        private void DeleteFile(string fileName)
        {
            var filePath = Path.Combine(WebAppInstancePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        private void CreateWebConfig()
        {
            var webConfigPath = Path.Combine(WebAppInstancePath, "web.config");
            if (File.Exists(webConfigPath))
            {
                File.Delete(webConfigPath);
            }

            var resourceReader = new EmbeddedResourceReader(Assembly.GetAssembly(typeof(WebInstanceTestsEnvironmentManager)));
            resourceReader.CopyBytesToFileSystem(@"Data\WebAppInstance\web.config", webConfigPath);
        }


        private void RegisterInstanceInIIS()
        {
            var iisExpressPath = Path.Combine(mProgramFilesPath, "IIS Express\\appcmd.exe");

            var bindings = string.Format(@"http/:{0}:localhost", mWebAppInstancePort);
            var args = string.Format(@"add site /name:{0} /physicalPath:{1} /bindings:{2} /serverAutoStart:true", WebAppInstanceName, WebAppInstancePath, bindings);
            Process.Start(iisExpressPath, args);

            args = string.Format(@"add app /site.name:{0} /path:/", WebAppInstanceName);
            Process.Start(iisExpressPath, args);
        }


        private void UnRegisterInstanceInIIS()
        {
            var iisExpressPath = Path.Combine(mProgramFilesPath, "IIS Express\\appcmd.exe");

            var args = string.Format(@"delete site {0}", WebAppInstanceName);
            Process.Start(iisExpressPath, args);
        }


        private void StopIIS()
        {
            if ((mIisProcess != null) && !mIisProcess.HasExited)
            {
                mIisProcess.Kill();
            }
        }

        #endregion
    }
}