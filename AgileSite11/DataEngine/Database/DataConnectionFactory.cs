using System;
using System.Threading;

using CMS.Base;
using CMS.DataProviderSQL;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides DataConnection object for specified data provider according to configuration settings.
    /// </summary>
    public static class DataConnectionFactory
    {
        private static IDataProvider mProviderObject;
        private static ISqlGenerator mGeneratorObject;
        private static ITableManager mTableManagerObject;
        private static string mProviderAssemblyName;

        /// <summary>
        /// Gets connection method delegate.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public delegate IDataConnection GetConnectionEventHandler(string connectionString);


        /// <summary>
        /// Event raised when the connection is required. Returns the connection.
        /// </summary>
        public static event GetConnectionEventHandler OnGetConnection;


        /// <summary>
        /// Custom Provider library assembly.
        /// </summary>
        private static string ProviderAssemblyName
        {
            get
            {
                return mProviderAssemblyName ?? (mProviderAssemblyName = SettingsHelper.AppSettings["CMSDataProviderAssembly"]);
            }
        }


        /// <summary>
        /// Returns the provider object.
        /// </summary>
        public static IDataProvider ProviderObject
        {
            get
            {
                if (mProviderObject == null)
                {
                    Interlocked.CompareExchange(ref mProviderObject, CreateInstanceOf<IDataProvider>("DataProvider", () => new DataProvider()), null);
                }
                return mProviderObject;
            }
        }


        /// <summary>
        /// Returns the SQL generator object.
        /// </summary>
        public static ISqlGenerator GeneratorObject
        {
            get
            {
                if (mGeneratorObject == null)
                {
                    Interlocked.CompareExchange(ref mGeneratorObject, CreateInstanceOf<ISqlGenerator>("SqlGenerator", () => new DataProviderSQL.SqlGenerator()), null);
                }
                return mGeneratorObject;
            }
        }


        /// <summary>
        /// Returns the Table manager object.
        /// </summary>
        public static ITableManager TableManagerObject
        {
            get
            {
                if (mTableManagerObject == null)
                {
                    Interlocked.CompareExchange(ref mTableManagerObject, NewTableManagerObject(null), null);
                }
                return mTableManagerObject;
            }
        }

        
        /// <summary>
        /// Returns the connection string.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return ProviderObject.ConnectionString;
            }
            set
            {
                ProviderObject.ConnectionString = value;
            }
        }


        /// <summary>
        /// Current DB connection to use within current connection scope.
        /// </summary>
        public static IDataConnection CurrentConnection
        {
            get
            {
                return ProviderObject.CurrentConnection;
            }
            set
            {
                ProviderObject.CurrentConnection = value;
            }
        }


        /// <summary>
        /// Creates a new table manager object with the given connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static ITableManager NewTableManagerObject(string connectionString)
        {
            var tableManager = CreateInstanceOf<ITableManager>("TableManager", () => new DataProviderSQL.TableManager());

            tableManager.ConnectionString = connectionString;

            return tableManager;
        }


        /// <summary>
        /// Returns a new database connection.
        /// </summary>
        public static IDataConnection GetConnection()
        {
            return GetConnection(null);
        }


        /// <summary>
        /// Returns a new database connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static IDataConnection GetConnection(string connectionString)
        {
            IDataConnection result = null;

            // Get by custom handler if required
            if (OnGetConnection != null)
            {
                result = OnGetConnection(connectionString);
            }

            return result ?? GetNativeConnection(connectionString);
        }


        /// <summary>
        /// Returns a new database connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="newConnection">If true, a new connection instance is created</param>
        public static IDataConnection GetNativeConnection(string connectionString, bool newConnection = false)
        {
            // If no connection string given and existing connection is accepted, try to get the existing connection
            if (!newConnection && String.IsNullOrEmpty(connectionString))
            {
                var result = CurrentConnection;
                if (result != null)
                {
                    return result;
                }
            }

            return ProviderObject.GetNewConnection(connectionString);
        }


        /// <summary>
        /// Sends the specific command with arguments to the provider.
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="commandArguments">Command arguments (parameters)</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static object ProcessCommand(string commandName, object[] commandArguments)
        {
            return ProviderObject.ProcessCommand(commandName, commandArguments);
        }


        private static T CreateInstanceOf<T>(string typeName, Func<T> defaultImpl)
        {
            if (!String.IsNullOrEmpty(ProviderAssemblyName))
            {
                string fullName = $"{ProviderAssemblyName}.{typeName}";
                var implementation = ClassHelper.GetClass<T>(ProviderAssemblyName, fullName);

                if (implementation == null)
                {
                    throw new ApplicationException($"The class {fullName} couldn't be loaded.");
                }

                return implementation;
            }

            return defaultImpl();
        }
    }
}