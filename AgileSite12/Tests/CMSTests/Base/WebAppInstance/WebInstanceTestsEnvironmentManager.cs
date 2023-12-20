using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using CMS.Base;
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

        /// <summary>
        /// Folders copied to instance directory.
        /// </summary>
        protected virtual string[] FoldersToCopy => new[]
        {
            "Admin",
            "App_Data",
            "App_Themes",
            "Bin",
            "CMSAdminControls",
            "CMSAPIExamples",
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
            "CorporateSite",
            "DancingGoat",
            "EcommerceSite",
            "IntranetPortal"
        };


        /// <summary>
        /// Files copied to instance directory.
        /// </summary>
        protected virtual string[] FilesToCopy => new[]
        {
            "Default.aspx",
            "Default.aspx.cs",
            "favicon.ico",
            "Global.asax",
            "DocumentRESTService.svc",
            "ObjectTranslationRESTService.svc",
            "RESTService.svc"
        };

        
        /// <summary>
        /// Folders excluded from copying to instance directory.
        /// </summary>
        protected virtual string[] FoldersToExclude => new[]
        {
            @"app_data\cirepository",
            @"app_data\cmstemp"
        };


        /// <summary>
        /// Files excluded from copying to instance directory.
        /// </summary>
        protected virtual string[] FilesToDelete => new[]
        {
            @"App_Data\CMSModules\ImportExport\dev_mode"
        };


        /// <summary>
        /// Dictionary where key is folder to copy to instance directory and value is its new name.
        /// </summary>
        protected virtual Dictionary<string, string> FoldersToCopyAndRename => new Dictionary<string, string>
        {
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


        /// <summary>
        /// Source instance path.
        /// </summary>
        protected virtual string OriginalInstancePath
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


        private Func<string, bool> IncludePredicate
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
            UnRegisterInstanceInIIS();
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
            RegisterInstanceInIIS();

            if ((mIisProcess == null) || mIisProcess.HasExited)
            {
                mIisProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(mProgramFilesPath, "IIS Express\\iisexpress.exe"),
                        Arguments = $"/site:{WebAppInstanceName}"
                    }
                };
                mIisProcess.Start();
            }
        }


        /// <summary>
        /// Sets up connection string.
        /// </summary>
        public void SetConnectionString(string connectionString)
        {
            var config = SettingsHelper.OpenConfiguration(WebAppInstancePath);

            var cs = config.ConnectionStrings.ConnectionStrings["CMSConnectionString"];
            if (cs.ConnectionString != connectionString)
            {
                cs.ConnectionString = connectionString;
                config.Save(ConfigurationSaveMode.Modified);
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

            foreach (var folder in FoldersToCopyAndRename)
            {
                CopyFolderToWebAppInstance(folder.Key, folder.Value);
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


        private void CopyFolderToWebAppInstance(string folderName, string newFolderName = "")
        {
            var sourceFolder = Path.Combine(OriginalInstancePath, folderName);
            var destinationFolder = Path.Combine(WebAppInstancePath, string.IsNullOrEmpty(newFolderName) ? folderName : newFolderName);

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
            var sourceFile = Path.Combine(OriginalInstancePath, fileName);

            if (!File.Exists(sourceFile))
            {
                return;
            }

            var destinationFile = sourceFile.Replace(OriginalInstancePath, WebAppInstancePath);
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

            var bindings = $@"http://localhost:{mWebAppInstancePort}";
            var args = $@"add site /name:{WebAppInstanceName} /physicalPath:{WebAppInstancePath} /bindings:{bindings} /serverAutoStart:true";
            Process.Start(iisExpressPath, args);
        }


        private void UnRegisterInstanceInIIS()
        {
            var iisExpressPath = Path.Combine(mProgramFilesPath, "IIS Express\\appcmd.exe");

            var args = $@"delete site {WebAppInstanceName}";
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