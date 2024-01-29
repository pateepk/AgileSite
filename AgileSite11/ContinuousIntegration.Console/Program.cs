using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.Search;

namespace CMS.ContinuousIntegration.ConsoleApp
{
    /// <summary>
    /// Console application for continuous integration in Kentico.
    /// Return codes:
    /// 0 - success, commands successfully finished without errors
    /// 1 - error occurred while commands
    /// 2 - unrecognized commands or help is displayed either by help arguments or another way 
    /// </summary>
    internal static class Program
    {
        #region Static Fields

        /// <summary>
        /// Delegate for console event
        /// Keeps it from getting garbage collected!
        /// </summary>
        private static ConsoleEventDelegate Handler;

        /// <summary>
        /// Cancellation token
        /// Has to be used for cancelling the CI restore while in progress
        /// </summary>
        private static CancellationTokenSource CancellationTokenSource;

        #endregion


        #region "Constants"

        private const int SUCCESS_EXIT_CODE = 0;
        private const int ERROR_EXIT_CODE = 1;
        private const int UNRECOGNIZED_EXIT_CODE = 2;

        private const string DESERIALIZATION_START = "Restoring objects...";
        private const string SEARCH_REBUILD_START = "Creating search tasks for rebuilding changed or new indexes...";
        private const string ERROR_OCCURRED = "Errors occurred while restoring objects from the repository:";
        private const string WARNING_OCCURRED = "Warnings were logged while restoring objects from the repository:";
        private const string RESTORE_PARAM = "-r";
        private const string SEARCH_REBUILD_PARAM = "-s";
        private const string HELP = @"
Syntax: ContinuousIntegration.exe [options]

Options:

  -r   Restores objects from the repository to the database
       of the current Kentico instance

  -s   [OPTIONAL] Creates 'Rebuild' tasks for all changed 
       or new search indexes after the end of the restore action
    
Exit codes:

   0   Success, all operations were finished without errors
   1   Errors occurred while performing operations
   2   Unrecognized parameters or help";

        private const string SPLASHSCREEN = @"
Continuous Integration Console
Kentico Software";

        private const string RESTORE_WAS_CANCELED = @"
The restore operation was manually canceled.
The partial restore may leave the database in an inconsistent state. To resolve potential problems, you need to run the restore operation again.";

        private static bool createRebuildTasks;

        #endregion


        #region "Methods"

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        internal static void Main(string[] args)
        {
            Console.WriteLine(SPLASHSCREEN.TrimStart());

            try
            {
                bool parametersAreValid = ValidCommandArguments(args);

                // When arguments are not recognized or it doesn't fulfill the syntax, show help and exit
                if (parametersAreValid)
                {
                    string pathToExe = Assembly.GetExecutingAssembly().Location;

#pragma warning disable BH1014 // Do not use System.IO
                    string webAppPhysicalPath = Directory.GetParent(pathToExe).Parent.FullName;
#pragma warning restore BH1014 // Do not use System.IO

                    InitKenticoApplication(webAppPhysicalPath);
                    
                    var exitCode = Deserialize();

                    if ((exitCode != ERROR_EXIT_CODE) && createRebuildTasks)
                    {
                        CreateRebuildSearchTasks();
                    }

                    Environment.Exit(exitCode);
                }
                else
                {
                    DisplayHelp();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Environment.Exit(ERROR_EXIT_CODE);
            }
        }

        
        /// <summary>
        /// Validates command arguments
        /// </summary>
        /// <param name="args">Arguments</param>
        private static bool ValidCommandArguments(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            }

            bool restoreParam = args.First().Equals(RESTORE_PARAM, StringComparison.InvariantCultureIgnoreCase);

            switch (args.Length)
            {
                case 1:
                    return restoreParam;

                case 2:
                    createRebuildTasks = args.Last().Equals(SEARCH_REBUILD_PARAM, StringComparison.InvariantCultureIgnoreCase);
                    return restoreParam && createRebuildTasks;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Display help and return exit code.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine(HELP);
            Environment.Exit(UNRECOGNIZED_EXIT_CODE);
        }


        /// <summary>
        /// Deserializes files from the CI repository 
        /// </summary>
        private static int Deserialize()
        {
            Console.WriteLine(DESERIALIZATION_START);

            CancellationTokenSource = new CancellationTokenSource();
            Handler = ConsoleEventCallback;
            SetConsoleCtrlHandler(Handler, true);

            Action<LogItem> messageHandler = delegate(LogItem logItem)
            {
                // Error messages should not be shown when manual cancellation was requested
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine(logItem.Message);
                }
            };

            RepositoryActionResult restoreAllResult;

            // Turn off smart search indexer, smart search tasks are still created but processed after restore is finished
            using (new CMSActionContext { EnableSmartSearchIndexer = false })
            {
                restoreAllResult = FileSystemRepositoryManager.RestoreAll(messageHandler, CancellationTokenSource.Token);
            }

            return ProcessRestoreResult(restoreAllResult, CancellationTokenSource);
        }


        private static void CreateRebuildSearchTasks()
        {
            Console.WriteLine(SEARCH_REBUILD_START);

            var q = SearchIndexInfoProvider.GetSearchIndexes().Columns("IndexName", "IndexID", "IndexGUID", "IndexIsOutdated", "IndexStatus");

            q.ForEachObject(index =>
            {
                if (index.IndexIsOutdated || (SearchIndexInfoProvider.GetIndexStatus(index) == IndexStatusEnum.NEW))
                {
                    var parameters = new SearchTaskCreationParameters
                    {
                        TaskType = SearchTaskTypeEnum.Rebuild,
                        TaskValue = index.IndexName,
                        RelatedObjectID = index.IndexID
                    };

                    SearchTaskInfoProvider.CreateTask(parameters, false);
                }
            });
        }


        /// <summary>
        /// Initializes Kentico application in which is console located.
        /// Initialization is needed for Kentico API calls.
        /// </summary>
        /// <param name="webAppPhysicalPath">Physical path to Kentico instance that will be initialized</param>
        private static void InitKenticoApplication(string webAppPhysicalPath)
        {
            SystemContext.WebApplicationPhysicalPath = webAppPhysicalPath;
            SystemContext.UseWebApplicationConfiguration = true;
            CMSApplication.Init();
        }


        private static void LogResult(RepositoryActionResult restoreResult)
        {
            if (restoreResult.Errors.Any())
            {
                Console.WriteLine(ERROR_OCCURRED);
                foreach (var error in restoreResult.Errors)
                {
                    Console.Error.WriteLine(error);
                }
            }

            if (restoreResult.Warnings.Any())
            {
                Console.WriteLine(WARNING_OCCURRED);
                foreach (var warning in restoreResult.Warnings)
                {
                    Console.Error.WriteLine(warning);
                }
            }
        }


        /// <summary>
        /// Processes given restoration result to Console, when operation was not canceled
        /// </summary>
        /// <returns>exit code according to specification</returns>
        private static int ProcessRestoreResult(RepositoryActionResult restoreResult, CancellationTokenSource cancellationTokenSource)
        {
            LogResult(restoreResult);

            if (!restoreResult.Success && !cancellationTokenSource.IsCancellationRequested)
            {
                return ERROR_EXIT_CODE;
            }

            return SUCCESS_EXIT_CODE;
        }


        /// <summary>
        /// Callback for console events
        /// Assigned to static property to prevent garbage collection!
        /// </summary>
        /// <param name="eventType">event type as integer number</param>
        private static bool ConsoleEventCallback(int eventType)
        {
            var supportedEventTypes = Enum.GetValues(typeof(ConsoleEventType)).OfType<ConsoleEventType>().Select(i => (int)i);
            if (!supportedEventTypes.Contains(eventType))
            {
                return true;
            }

            CancellationTokenSource.Cancel(false);

            // Manual cancelation shows specific message, not the general one from job
            Console.WriteLine(RESTORE_WAS_CANCELED);
            Console.ReadLine();

            return false;
        }

        #endregion

        /// <summary>
        /// Represents native console event types
        /// More information here: https://msdn.microsoft.com/en-us/library/windows/desktop/ms686016%28v=vs.85%29.aspx
        /// </summary>
        private enum ConsoleEventType
        {
            // Ctrl + C was pressed
            CTRL_C_EVENT = 0,

            // Ctrl + Break was pressed
            CTRL_BREAK_EVENT = 1,

            // Console was closed
            CTRL_CLOSE_EVENT = 2,

            // User has logged off
            CTRL_LOGOFF_EVENT = 5,

            // System shutdown
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool ConsoleEventDelegate(int eventType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate consoleEventDelegate, bool add);
    }
}
