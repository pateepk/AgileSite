using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;

using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Allows integration tests to run in isolation as for each test a clean database with current schema and default objects is created.
    /// </summary>
    /// <remarks>
    /// The isolated integration tests use LocalDB to create and drop databases when needed. It works with two types of database.
    /// The master database is created using SQL scripts from the solution. For each modification of this folder a new master database is created.
    /// The master database is detached after it has been created and its files are copied to create an instance database that a running test will use.
    /// The instance and master databases share a name, but the file names are different.
    /// The instance database is removed when a test finishes and a master database, that is out of date, is removed automatically.
    /// </remarks>
    public abstract class AutomatedTestsWithLocalDB : AutomatedTestsWithData
    {
        #region "Variables"

        private const string DB_SCHEMA = "dbo";
        private const string DEFAULT_HASH_STRING_SALT = "test";

        #endregion


        #region "Exception messages"

        private static readonly Lazy<string> ExceptionMessage_EmptySetting_CMSTestDatabaseFolderPath = new Lazy<string>(() => String.Format(@"
            The path to the folder with database files is not specified.

            The integration test requires a path to the folder where the test database will be created.
            By default the test database will be created in the TestDatabases folder of the Kentico solution.
            When the test runs, it assumes that it is located in a subfolder of the Kentico solution, and tries to find the solution root folder.
            To override this behavior you can specify the folder path in the configuration file via the CMSTestDatabaseFolderPath setting in the appSettings section.
            The path must be absolute. If you want to create the test database in C:\Data\TestDatabases\{0}, you need to add the following line to the appSettings section:
                            
            <add key=""CMSTestDatabaseFolderPath"" value=""C:\Data\TestDatabases\{0}"" />", CMSVersion.GetVersion(true, true, false, false)));
        
        private const string ExceptionMessage_ConnectionFailed = @"
            Could not establish connection to the Microsoft SQL Server Express LocalDB.

            Please make sure that the Microsoft SQL Server 2012 Express LocalDB (or higher) is installed.
            For more information please visit http://technet.microsoft.com/en-us/library/hh510202.aspx.
            When the test runs, it assumes that the Microsoft SQL Server 2012 Express LocalDB should be used to create test databases.
            To override this behavior you can specify a name of another instance of LocalDB via the CMSTestDatabaseInstanceName setting in the appSettings section.
            If you want to duplicate the default behavior, you need to add the following line to the appSettings section:

            <add key=""CMSTestDatabaseInstanceName"" value=""(LocalDB)\v11.0"" />";

        private static Lazy<string> ExceptionMessage_ScriptsNotFound = new Lazy<string>(() => String.Format(@"
            The SQL scripts required for the initialization of the database were not found.
            Please check whether the CMS\App_Data\Install folder in the tests output or solution folder contains a SQL.zip file or a SQL subfolder.

            You can specify the path to folder containing SQL.zip file or a SQL subfolder in the configuration file via the CMSTestDatabaseScriptFolderPath setting.
            The path must be absolute. If the solution is located in C:\Projects\{0}, you need to add the following line to the appSettings section:
                            
            <add key=""CMSTestDatabaseScriptFolderPath"" value=""C:\Projects\{0}\CMS\App_Data\Install"" />", CMSVersion.GetVersion(true, true, false, false)));

        private const string ExceptionMessageFormat_InstallationFailed = @"
            The initialization of the test database failed with the following message: {0}.";

        #endregion


        #region "Private members and properties"

        /// <summary>
        /// The path to the 'SQL' folder or 'SQL.zip' file that contains SQL scripts.
        /// </summary>
        private static readonly Lazy<string> mDatabaseScriptsPath = new Lazy<string>(GetDatabaseScriptPath);


        /// <summary>
        /// The path to the folder with database files.
        /// </summary>
        private static string mDatabaseFolderPath;


        /// <summary>
        /// A name of the instance of Microsoft SQL Server Express LocalDB that will be used to run the tests.
        /// </summary>
        private static string mDatabaseInstanceName;


        /// <summary>
        /// The regular expression that matches test database file names.
        /// </summary>
        private static readonly Regex mDatabaseFileNameRegex = new Regex(@"^CMSTEST_\d{8}_\d{6}_(MASTER|INSTANCE)(_log\.ldf|\.mdf)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static MutexSecurity mMutexSecuritySettings;

        private static DatabaseProperties mMasterDatabase;
        private static DatabaseProperties mTestDatabase;
        private static DatabaseProperties mFixtureDatabase;

        private List<string> mExtraDatabaseNames;
        private List<DatabaseProperties> mExtraDatabases;
        
        private int mFixtureTestIndex = -1;
        
        /// <summary>
        /// Returns Mutex security settings to allow access to it by multiple users
        /// </summary>
        private static MutexSecurity MutexSecuritySettings
        {
            get
            {
                return mMutexSecuritySettings ?? (mMutexSecuritySettings = GetMutexSecuritySettings());
            }
        }


        /// <summary>
        /// The path to the 'SQL' folder or 'SQL.zip' file that contains SQL scripts.
        /// </summary>
        internal static string DatabaseScriptsPath
        {
            get
            {
                return mDatabaseScriptsPath.Value;
            }
        }


        /// <summary>
        /// Gets the path to the folder with database files.
        /// </summary>
        private static string DatabaseFolderPath
        {
            get
            {
                if (mDatabaseFolderPath == null)
                {
                    var databaseFolderPath = TestsConfig.GetTestAppSetting("CMSTestDatabaseFolderPath");
                    if (String.IsNullOrWhiteSpace(databaseFolderPath))
                    {
                        if (TestsConfig.SolutionFolderPath == null)
                        {
                            throw new Exception(ExceptionMessage_EmptySetting_CMSTestDatabaseFolderPath.Value);
                        }
                        databaseFolderPath = Path.Combine(TestsConfig.SolutionFolderPath, "TestDatabases");
                    }
                    mDatabaseFolderPath = databaseFolderPath;
                }

                return mDatabaseFolderPath;
            }
        }


        /// <summary>
        /// Gets a name of the instance of Microsoft SQL Server Express LocalDB that will be used to create a database.
        /// </summary>
        private static string DatabaseInstanceName
        {
            get
            {
                if (mDatabaseInstanceName == null)
                {
                    var instanceName = TestsConfig.GetTestAppSetting("CMSTestDatabaseInstanceName");
                    if (String.IsNullOrWhiteSpace(instanceName))
                    {
                        instanceName = @"(LocalDB)\v11.0";
                    }
                    mDatabaseInstanceName = instanceName;
                }

                return mDatabaseInstanceName;
            }
        }


        /// <summary>
        /// List of extra database names
        /// </summary>
        private List<string> ExtraDatabaseNames
        {
            get
            {
                return mExtraDatabaseNames ?? (mExtraDatabaseNames = GetExtraDatabaseNames());
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// <para>Indicates if the same database is used for all tests in same test class.</para>
        /// </summary>
        public bool SharedDatabaseForAllTests
        {
            get;
        }

        #endregion


        #region "Setup and cleanup methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.Tests.IsolatedIntegrationTests"/> class.
        /// </summary>
        protected AutomatedTestsWithLocalDB()
        {
            SharedDatabaseForAllTests = GetType().GetCustomAttributes(typeof(SharedDatabaseForAllTestsAttribute), true).Any();
        }


        private List<string> GetExtraDatabaseNames()
        {
            var extraDatabases = GetType().GetCustomAttributes(typeof(ExtraDatabaseAttribute), true);

            return extraDatabases.Cast<ExtraDatabaseAttribute>().Select(db => db.Name).ToList();
        }


        /// <summary>
        /// Initializes the current test environment.
        /// </summary>
        [TestInitialize]
        [SetUp]
        public void AutomatedTestsWithLocalDBSetup()
        {
            mFixtureTestIndex++;

            if (!SharedDatabaseForAllTests)
            {
                EnsureLocalDatabases();
            }
            else
            {
                // Ensure connection string when database is shared
                SetConnectionAndHashSalts(mFixtureDatabase);
            }

            ProviderHelper.LoadHashTablesSettings = true;

            InitApplication();            

            RunExtenderAction<AutomatedTestsWithLocalDB>(e => e.SetUp());
        }


        /// <summary>
        /// Sets connection strings and hash salt for all set up databases
        /// </summary>
        /// <param name="database">Test database</param>
        private void SetConnectionAndHashSalts(DatabaseProperties database)
        {
            SetConnectionAndHashSalt(database);

            SetExtraDatabasesConnectionStrings();
        }


        /// <summary>
        /// Initializes the test class for all tests<para/>
        /// Note: Calling this method in NUnit tests causes duplicate initialization
        /// </summary>
        internal override void InitializeTestClassInternal()
        {
            base.InitializeTestClassInternal();

            InitAppAndDatabaseBeforeTests();
        }


        /// <summary>
        /// Cleans up the class after all tests are run <para/>
        /// Note: Calling this method in NUnit tests causes duplicate clean up
        /// </summary>
        internal override void CleanUpTestClassInternal()
        {
            CleanUpSharedDatabase();
            
            base.CleanUpTestClassInternal();
        }


        /// <summary>
        /// Inits shared database (if required)
        /// </summary>
        private void InitAppAndDatabaseBeforeTests()
        {
            if (SharedDatabaseForAllTests)
            {
                var suffix = Guid.NewGuid() + "_SHARED";

                InitSharedDatabase(suffix);

                InitApplication();
            }
        }


        /// <summary>
        /// Init shared instance database in NUnit tests.
        /// </summary>
        [OneTimeSetUp]
        public void AutomatedTestsWithLocalDBFixtureSetup()
        {
            InitAppAndDatabaseBeforeTests();

            RunExtenderAction<AutomatedTestsWithLocalDB>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Cleans the current test environment.
        /// </summary>
        [TestCleanup]
        [TearDown]
        public void AutomatedTestsWithLocalDBTearDown()
        {
            RunExtenderAction<AutomatedTestsWithLocalDB>(e => e.TearDown(), true);

            EndApplication();

            if (!SharedDatabaseForAllTests)
            {
                ReleaseDatabases(mTestDatabase);
                mTestDatabase = null;
            }
        }


        /// <summary>
        /// Removes shared instance database in NUnit tests.
        /// Note: This method is called automatically in NUnit tests.
        /// </summary>
        [OneTimeTearDown]
        public void AutomatedTestsWithLocalDBFixtureTearDown()
        {
            RunExtenderAction<AutomatedTestsWithLocalDB>(e => e.FixtureTearDown(), true);

            CleanUpSharedDatabase();
        }
               
        #endregion


        #region "Methods"

        /// <summary>
        /// Gets a unique index for the test run.
        /// All calls from a single test run return the same index.
        /// </summary>
        public int GetTestUniqueIndex()
        {
            return mFixtureTestIndex;
        }
        

        /// <summary>
        /// Ensures that the given action is executed only once at a time across all application domains using <see cref="Mutex" />
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="mutexName">Name of the <see cref="Mutex" /> to use</param>
        /// <param name="waitForCompletion">If true, the other callers wait for the action completion</param>
        internal static void ExecuteOnceAcrossAppDomains(Action action, string mutexName, bool waitForCompletion = true)
        {
            // Allow multi-user usage
            string mutexId = string.Format("Global\\CMS_{0}", mutexName);

            bool createdNew;

            // Get the mutex
            using (var mtx = new Mutex(true, mutexId, out createdNew, MutexSecuritySettings))
            {
                try
                {
                    if (createdNew)
                    {
                        // Run the action only by thread which created the Mutex
                        action();
                    }
                    else if (waitForCompletion)
                    {
                        // Wait for the completion of the action in another thread
                        mtx.WaitOne();
                    }
                }
                finally
                {
                    if (createdNew || waitForCompletion)
                    {
                        mtx.ReleaseMutex();
                    }
                }
            }
        }


        /// <summary>
        /// Returns Mutex security settings to allow access to it by multiple users
        /// </summary>
        private static MutexSecurity GetMutexSecuritySettings()
        {
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            return securitySettings;
        }


        /// <summary>
        /// Provides a current version of the instance database.
        /// </summary>
        private void EnsureLocalDatabases(string masterDatabaseName = null, string instanceDatabaseNameSuffix = null)
        {
            mMasterDatabase = EnsureMasterDatabase(masterDatabaseName);

            // Suffix for instance database - if not specified use unique guid
            instanceDatabaseNameSuffix = instanceDatabaseNameSuffix ?? Guid.NewGuid().ToString();

            var instanceDatabaseName = mMasterDatabase.Name + "_" + instanceDatabaseNameSuffix;

            // Create default database
            mTestDatabase = EnsureLocalDatabase(mMasterDatabase, instanceDatabaseName);

            SetConnectionAndHashSalt(mTestDatabase);

            EnsureExtraDatabases(instanceDatabaseName);
        }


        /// <summary>
        /// Ensures the master database as a source for local databases
        /// </summary>
        /// <param name="masterDatabaseName">Master database name</param>
        private static DatabaseProperties EnsureMasterDatabase(string masterDatabaseName)
        {
            // Master database properties - either new database which will be created or use existing database
            var masterDatabase = (masterDatabaseName == null) ? GetMasterDatabaseProperties() : GetInstanceDatabaseProperties(masterDatabaseName);

            ExecuteOnceAcrossAppDomains(() => EnsureMasterDatabase(masterDatabase), "CMSTestsSetupMasterDB");
            return masterDatabase;
        }


        /// <summary>
        /// Provides a current version of the instance database.
        /// </summary>
        private static DatabaseProperties EnsureLocalDatabase(DatabaseProperties masterDatabase, string instanceDatabaseName)
        {
            var database = GetInstanceDatabaseProperties(instanceDatabaseName);

            CreateInstanceDatabase(database, masterDatabase);

            return database;
        }


        /// <summary>
        /// Removes the instance database with the specified name.
        /// </summary>
        private void ReleaseDatabases(DatabaseProperties database, bool cleanupDataContext = true)
        {
            try
            {
                if (database != null)
                {
                    // Reset flags that indicates availability of DB
                    DatabaseHelper.Clear();

                    ReleaseLocalDatabase(database);

                    ReleaseExtraDatabases();
                }
            }
            finally
            {
                if (cleanupDataContext)
                {
                    CleanUpDataContext();
                }
            }
        }


        /// <summary>
        /// Removes the instance database with the specified name.
        /// </summary>
        /// <param name="database">Name of instance database</param>
        private static void ReleaseLocalDatabase(DatabaseProperties database)
        {
            // Delete test database asynchronously in extra thread to release thread for the next test
            new Thread(
                () => RemoveDatabase(database)
            )
            .Start();
        }


        /// <summary>
        /// Creates an instance database from the specified master database.
        /// </summary>
        /// <param name="database">Properties of the instance database to create.</param>
        /// <param name="masterDatabase">Properties of the master database.</param>
        private static void CreateInstanceDatabase(DatabaseProperties database, DatabaseProperties masterDatabase)
        {
            File.Copy(masterDatabase.FileName, database.FileName, true);
            File.Copy(masterDatabase.LogFileName, database.LogFileName, true);
        }


        /// <summary>
        /// Removes the specified database.
        /// </summary>
        /// <param name="database">Properties of the database to remove.</param>
        private static void RemoveDatabase(DatabaseProperties database)
        {
            var connectionString = GetServerConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception exception)
                {
                    throw new Exception(ExceptionMessage_ConnectionFailed, exception);
                }

                var command = connection.CreateCommand();

                command.CommandText = String.Format(@"select name from master.{0}.sysdatabases where name = '{1}'", DB_SCHEMA, database.Name);
                var databaseName = command.ExecuteScalar();

                if (databaseName != null)
                {
                    command.CommandText = String.Format(
@"
alter database [{0}] set single_user with rollback immediate

drop database [{0}]
",
                        database.Name
                    );

                    command.ExecuteNonQuery();
                }
                else if (File.Exists(database.FileName))
                {
                    File.Delete(database.FileName);
                    File.Delete(database.LogFileName);
                }
            }
        }


        /// <summary>
        /// Creates a current master database if necessary.
        /// </summary>
        /// <param name="database">Properties of the master database to create.</param>
        private static void EnsureMasterDatabase(DatabaseProperties database)
        {
            // Initialize hash string salt for existing database
            SettingsHelper.AppSettings[ValidationHelper.APP_SETTINGS_HASH_STRING_SALT] = DEFAULT_HASH_STRING_SALT;

            if (File.Exists(database.FileName))
            {
                return;
            }

            // When master database is recreated, all other test databases are invalid, because they are based on previous master DB
            Directory.CreateDirectory(DatabaseFolderPath);
            PurgeDatabaseFolder();

            CreateNewDatabase(database, InitializeMasterDatabase, true);
        }


        /// <summary>
        /// Creates a new database based on the given properties
        /// </summary>
        /// <param name="database">Database properties</param>
        /// <param name="setup">Database data setup actions</param>
        /// <param name="isMaster">If true, the database is a master database as a source for other databases</param>
        internal static void CreateNewDatabase(DatabaseProperties database, Action<DatabaseProperties> setup = null, bool isMaster = false)
        {
            bool dbCreated = false;

            try
            {
                var connectionString = GetServerConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(ExceptionMessage_ConnectionFailed, exception);
                    }

                    var command = connection.CreateCommand();

                    command.CommandText = String.Format(@"create database [{0}] on (name = N'{0}', filename = '{1}')", database.Name, database.FileName);
                    command.ExecuteNonQuery();

                    command.CommandText = String.Format("alter database [{0}] set recovery simple", database.Name);
                    command.ExecuteNonQuery();

                    dbCreated = true;

                    setup?.Invoke(database);

                    // If the database is master to be copied, shrink and detach
                    if (isMaster)
                    {
                        command.CommandText = String.Format("alter database [{0}] set single_user with rollback immediate", database.Name);
                        command.ExecuteNonQuery();

                        command.CommandText = String.Format("DBCC SHRINKDATABASE('{0}')", database.Name);
                        command.ExecuteNonQuery();

                        command.CommandText = String.Format("exec master.{0}.sp_detach_db '{1}'", DB_SCHEMA, database.Name);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                // Remove the broken master database
                if (dbCreated)
                {
                    RemoveDatabase(database);
                }

                throw;
            }
        }


        /// <summary>
        /// Removes files of all test databases.
        /// </summary>
        private static void PurgeDatabaseFolder()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var fileNames = Directory.EnumerateFiles(DatabaseFolderPath).Where(x => mDatabaseFileNameRegex.IsMatch(Path.GetFileName(x))).ToArray();
            foreach (var fileName in fileNames)
            {
                File.Delete(fileName);
            }
        }


        /// <summary>
        /// Initializes the specified master database using current SQL scripts and default data.
        /// </summary>
        /// <param name="database">Properties of the master database to initialize.</param>
        private static void InitializeMasterDatabase(DatabaseProperties database)
        {
            var connectionString = GetConnectionString(database);

            var scriptPath = DatabaseScriptsPath;
            if (File.Exists(scriptPath))
            {
                scriptPath = Path.Combine(Path.GetDirectoryName(scriptPath), IO.ZipStorageProvider.GetZipFileName(Path.GetFileName(scriptPath)));
            }

            SqlInstallationHelper.AfterDataGet += UpdateMacroSignatures;

            try
            {
                SettingsHelper.AppSettings[ValidationHelper.APP_SETTINGS_HASH_STRING_SALT] = DEFAULT_HASH_STRING_SALT;
                SqlInstallationHelper.InstallDatabase(connectionString, scriptPath, null, null, HandleDatabaseInstallationMessage);
            }
            finally
            {
                SqlInstallationHelper.AfterDataGet -= UpdateMacroSignatures;
            }
        }


        /// <summary>
        /// Updates macro signatures in default data.
        /// </summary>
        private static void UpdateMacroSignatures(object sender, DataSetPostProcessingEventArgs args)
        {
            MacroSecurityProcessor.RefreshSecurityParameters(args.Data, new MacroIdentityOption { IdentityName = MacroIdentityInfoProvider.DEFAULT_GLOBAL_ADMINISTRATOR_IDENTITY_NAME });
        }


        /// <summary>
        /// Handles messages reported during the initialization of the database and throws en exception if there is a problem.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">The message type.</param>
        private static void HandleDatabaseInstallationMessage(string message, MessageTypeEnum messageType)
        {
            if (messageType == MessageTypeEnum.Error || messageType == MessageTypeEnum.Warning)
            {
                var exceptionMessage = String.Format(ExceptionMessageFormat_InstallationFailed, message);
                throw new Exception(exceptionMessage);
            }
        }


        /// <summary>
        /// Gets the UTC date and time when the SQL scripts were last modified.
        /// </summary>
        /// <returns>The UTC date and time when the SQL scripts were last modified.</returns>
        private static DateTime GetDatabaseScriptTimestamp()
        {
            if (File.Exists(DatabaseScriptsPath))
            {
                return File.GetLastWriteTimeUtc(DatabaseScriptsPath);
            }

            var folderPath = new[]
            {
                DatabaseScriptsPath
            };

            return folderPath.Concat(Directory.EnumerateDirectories(DatabaseScriptsPath, "*", SearchOption.AllDirectories)).Max(x => Directory.GetLastWriteTimeUtc(x));
        }


        /// <summary>
        /// Gets the path to the file or folder with SQL scripts that the installer requires.
        /// </summary>
        /// <returns>The path to the file or folder with SQL scripts.</returns>
        private static string GetDatabaseScriptPath()
        {
            foreach(var scriptsPath in GetPossibleDatabaseScriptFolders())
            {
                var scriptFilePath = Path.Combine(scriptsPath, "SQL.zip");
                if (File.Exists(scriptFilePath))
                {
                    return scriptFilePath; 
                }

                var scriptFolderPath = Path.Combine(scriptsPath, "SQL");
                if (Directory.Exists(scriptFolderPath))
                {
                    return scriptFolderPath;
                }
            }

            throw new Exception(ExceptionMessage_ScriptsNotFound.Value);
        }


        /// <summary>
        /// Returns all possible directories that might contain SQL scripts. 
        /// </summary>
        private static IEnumerable<string> GetPossibleDatabaseScriptFolders()
        {
            var databaseScriptFolderPath = TestsConfig.GetTestAppSetting("CMSTestDatabaseScriptFolderPath");
            if (!String.IsNullOrWhiteSpace(databaseScriptFolderPath))
            {
                yield return databaseScriptFolderPath;
            }

            if (TestsConfig.SolutionFolderPath != null)
            {
                yield return Path.Combine(TestsConfig.SolutionFolderPath, "CMS", "App_Data", "Install");
            }

            // For our Kentico.Libraries.Tests NuGet package, we're copying SQL folder into the 
            // TargetDir folder (usually bin folder). That's why we're trying to locate this 
            // folder in the executing assembly location.
            yield return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }


        /// <summary>
        /// Gets properties of the current master database.
        /// </summary>
        /// <returns>Properties of the current master database.</returns>
        private static DatabaseProperties GetMasterDatabaseProperties()
        {
            var timestamp = GetDatabaseScriptTimestamp();
            var databaseName = String.Format("CMSTEST_{0}", timestamp.ToString("yyyyMMdd_HHmmss"));

            return DatabaseProperties.CreateForMaster(databaseName, DatabaseFolderPath);
        }


        /// <summary>
        /// Gets properties of the instance database using the specified database name.
        /// </summary>
        /// <param name="databaseName">A name of the instance database.</param>
        /// <returns>Properties of the instance database.</returns>
        private static DatabaseProperties GetInstanceDatabaseProperties(string databaseName)
        {
            return DatabaseProperties.CreateForInstance(databaseName, DatabaseFolderPath);
        }


        /// <summary>
        /// Gets a connection string for the LocalDB instance.
        /// </summary>
        /// <returns>A connection string to the LocalDB instance.</returns>
        private static string GetServerConnectionString()
        {
            return String.Format("Data Source={0};Initial Catalog=master;Integrated Security=True;Connection Timeout=240;", DatabaseInstanceName);
        }


        /// <summary>
        /// Gets a connection string for the specified instance database.
        /// </summary>
        /// <param name="database">Properties of the instance database to connect to.</param>
        /// <returns>A connection string for the specified instance database.</returns>
        private static string GetConnectionString(DatabaseProperties database)
        {
            return String.Format(@"Data Source={0};Initial Catalog={1};AttachDBFileName={2};Integrated Security=True;Connection Timeout=240;", DatabaseInstanceName, database.Name, database.FileName);
        }
        

        /// <summary>
        /// Sets connection strings and hash salt for all set up databases
        /// </summary>
        /// <param name="database">Test database</param>
        private void SetConnectionAndHashSalt(DatabaseProperties database)
        {
            ConnectionHelper.ConnectionString = GetConnectionString(database);
            SettingsHelper.AppSettings[ValidationHelper.APP_SETTINGS_HASH_STRING_SALT] = DEFAULT_HASH_STRING_SALT;
        }


        /// <summary>
        /// Init shared instance database in MSTest tests.
        /// Note: Use this method in method marked with [ClassInitialize] attribute.
        /// </summary>
        /// <param name="suffix">Database suffix</param>
        public void InitSharedDatabase(string suffix = "SHARED")
        {
            EnsureLocalDatabases(instanceDatabaseNameSuffix: suffix);

            mFixtureDatabase = mTestDatabase;
            mTestDatabase = null;
        }


        /// <summary>
        /// Removes shared instance database in MSTest tests.
        /// Note: Use this method in method marked with [ClassCleanup] attribute.
        /// </summary>
        private void CleanUpSharedDatabase()
        {
            ReleaseDatabases(mFixtureDatabase, false);
            mFixtureDatabase = null;
        }

        #endregion


        #region "Extra databases support"

        /// <summary>
        /// Provides extra databases for the test
        /// </summary>
        /// <param name="instanceDatabaseName">Instance database name. Used as a base name for extra databases</param>
        /// <param name="empty">If true, the database should be empty rather than regular installation</param>
        private void EnsureExtraDatabases(string instanceDatabaseName, bool empty = false)
        {
            foreach (var extraDbName in ExtraDatabaseNames)
            {
                CreateExtraDatabase(mMasterDatabase, instanceDatabaseName, extraDbName, empty);
            }
        }


        /// <summary>
        /// Sets connection strings for all extra databases
        /// </summary>
        private void SetExtraDatabasesConnectionStrings()
        {
            for (int i = 0; i < ExtraDatabaseNames.Count; i++)
            {
                var dbName = ExtraDatabaseNames[i];
                var dbProperties = mExtraDatabases[i];

                SetExtraDatabaseConnectionString(dbName, dbProperties);
            }
        }


        /// <summary>
        /// Creates a specified extra database and adds it to the list of databases
        /// </summary>
        /// <param name="masterDatabase">Master database</param>
        /// <param name="instanceDatabaseName">Instance database name</param>
        /// <param name="extraDbName">Extra database name</param>
        /// <param name="empty">If true, the database should be empty rather than regular installation</param>
        private DatabaseProperties CreateExtraDatabase(DatabaseProperties masterDatabase, string instanceDatabaseName, string extraDbName, bool empty = false)
        {
            DatabaseProperties extraDb;

            // Create extra database
            var extraName = instanceDatabaseName + "_" + extraDbName;
            if (empty)
            {
                extraDb = GetInstanceDatabaseProperties(extraName);

                CreateNewDatabase(extraDb);
            }
            else
            {
                extraDb = EnsureLocalDatabase(masterDatabase, extraName);
            }

            SetExtraDatabaseConnectionString(extraDbName, extraDb);

            mExtraDatabases = mExtraDatabases ?? new List<DatabaseProperties>();
            mExtraDatabases.Add(extraDb);

            return extraDb;
        }
        

        /// <summary>
        /// Sets connection string for an extra database
        /// </summary>
        /// <param name="extraDbName">Database name</param>
        /// <param name="extraDb">Database properties</param>
        private static void SetExtraDatabaseConnectionString(string extraDbName, DatabaseProperties extraDb)
        {
            var csName = GetExtraDatabaseConnectionStringName(extraDbName);

            SettingsHelper.ConnectionStrings[csName] = new ConnectionStringSettings(csName, GetConnectionString(extraDb));
        }


        /// <summary>
        /// Gets connection string name for given extra database
        /// </summary>
        /// <param name="extraDbName">Database name</param>
        protected static string GetExtraDatabaseConnectionStringName(string extraDbName)
        {
            return extraDbName + "CMSConnectionString";
        }


        /// <summary>
        /// Releases all allocated extra databases
        /// </summary>
        private void ReleaseExtraDatabases()
        {
            var extras = mExtraDatabases;
            if (extras != null)
            {
                mExtraDatabases = null;
                mExtraDatabaseNames = null;

                foreach (var extraDb in extras)
                {
                    ReleaseLocalDatabase(extraDb);
                }
            }
        }


        /// <summary>
        /// Executes the same given action in the context of all test databases. 
        /// Original database is handled first.
        /// </summary>
        /// <param name="action">Action to execute</param>
        public void ExecuteWithAllDatabases(Action<string> action)
        {
            action("");

            foreach (var dbName in ExtraDatabaseNames)
            {
                var name = dbName;
                ExecuteWithExtraDatabase(dbName, () => action(name));
            }
        }


        /// <summary>
        /// Ensures the specified extra database for the test
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="empty">If true, the database should be empty rather than regular installation</param>
        public string EnsureExtraDatabase(string dbName, bool empty = false)
        {
            if (!ExtraDatabaseExists(dbName))
            {
                var extraDb = CreateExtraDatabase(mMasterDatabase, GetCurrentTestDatabaseName(), dbName, empty);

                ExtraDatabaseNames.Add(dbName);

                return GetConnectionString(extraDb);
            }

            return GetConnectionString(mExtraDatabases.Single(db => db.Name.EndsWith("_" + dbName)));
        }


        /// <summary>
        /// Gets the local database name
        /// </summary>
        public string GetCurrentTestDatabaseName()
        {
            var localDb = mFixtureDatabase ?? mTestDatabase;
            var dbName = localDb.Name;

            return dbName;
        }


        /// <summary>
        /// Executes the given action in the context of the given extra database
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="action">Action to execute</param>
        public void ExecuteWithExtraDatabase(string dbName, Action action)
        {
            if (!ExtraDatabaseExists(dbName))
            {
                NUnit.Framework.Assert.Fail("Extra database '" + dbName + "' not found. You must add attribute [ExtraDatabase(\"" + dbName + "\")] to your test fixture or call EnsureExtraDatabase(\"" + dbName + "\") before accessing it.");
            }

            using (new CMSConnectionContext(dbName))
            {
                action();
            }
        }


        /// <summary>
        /// Returns true if the given extra database exists
        /// </summary>
        /// <param name="dbName">Database name</param>
        private bool ExtraDatabaseExists(string dbName)
        {
            return ExtraDatabaseNames.Contains(dbName);
        }


        /// <summary>
        /// Separate the database after creation. The database is stored as extra database.
        /// </summary>
        protected void SeparateDatabase()
        {
            // If already separated, do not separate
            if (SqlInstallationHelper.DatabaseIsSeparated())
            {
                return;
            }

            // Provide one extra database for target
            var mOMConnectionString = EnsureExtraDatabase("OM", true);

            var separation = new DatabaseSeparationHelper();

            separation.InstallationConnStringSeparate = mOMConnectionString;
            separation.InstallScriptsFolder = Path.Combine(GetDatabaseScriptPath(), "Objects");
            separation.ScriptsFolder = Path.Combine(GetDatabaseScriptPath(), "..\\..\\DBSeparation");

            separation.AllowCopyDataThroughApplication = true;
            separation.SqlServerCapabilities = new SqlServerCapabilities();

            separation.SeparateDatabase();
            separation.DeleteSourceTables(false, true);

            if (!string.IsNullOrEmpty(mOMConnectionString))
            {                
                SettingsHelper.SetConnectionString(DatabaseSeparationHelper.OM_CONNECTION_STRING, mOMConnectionString);                
            }
        }

        #endregion
    }
}