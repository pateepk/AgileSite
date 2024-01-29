using System;
using System.Collections.Generic;
using System.Linq;

using SystemIO = System.IO;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Parses and validates input arguments.  
    /// </summary>
    internal class ArgumentsProcessor
    {
        private const int MIN_ARGUMENTS_COUNT = 2;
        private const int MAX_ARGUMENTS_COUNT = 3;


        private const string HELP = @"
Syntax:  CIConsistencyChecker.exe ConnectionString RepositoryPath TeamCityOutput
Examples: 
  CIConsistencyChecker.exe ""Data Source=(localdb)\.\Kentico;(...)"" .\App_Data\OrigReposiotry
  CIConsistencyChecker.exe ""Data Source=(localdb)\.\Kentico;(...)"" .\App_Data\OrigReposiotry TeamCityOutput

Options:
  ConnectionString [REQUIRED] Connection string

  RepositoryPath   [REQUIRED] Path to the original CI repository files from VCS

  TeamCityOutput   [OPTIONAL] Formats error messages for TeamCity";


        private readonly IReadOnlyList<string> mArguments;


        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentsProcessor"/>.
        /// </summary>
        /// <param name="arguments">Arguments passed to the application.</param>
        public ArgumentsProcessor(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            mArguments = arguments.ToList().AsReadOnly();
        }


        /// <summary>
        /// Parses and validates arguments. 
        /// </summary>
        /// <returns>Validated arguments.</returns> 
        public Arguments Process()
        {
            if (IsHelpArgumentPassed() || !IsArgumentCountValid())
            {
                PrintHelpAndExit();
            }

            return ReadArguments();
        }


        private bool IsArgumentCountValid()
        {
            return ((mArguments.Count >= MIN_ARGUMENTS_COUNT) && (mArguments.Count <= MAX_ARGUMENTS_COUNT));
        }


        private bool IsHelpArgumentPassed()
        {
            var helpArguments = new[] { "-?", "/?", "-h", "--help", "help" };
            return (mArguments.Count == 1) && helpArguments.Any(arg => arg.Equals(mArguments[0], StringComparison.OrdinalIgnoreCase));
        }


        private void PrintHelpAndExit()
        {
            Console.WriteLine(HELP);
            Environment.Exit(0);            
        }


        /// <summary>
        /// Reads arguments.
        /// </summary>
        private Arguments ReadArguments()
        {
            var processedArguments = new Arguments();

            processedArguments.ConnectionString = mArguments[0];
            processedArguments.NewRepositoryFullPath = GetFullPath(@".\App_Data\CIRepository");
            processedArguments.OriginalRepositoryFullPath = GetFullPath(mArguments[1]);
            processedArguments.TeamCityOutput = IsTeamCityOutputRequested();

            return processedArguments;
        }


        /// <summary>
        /// If given <paramref name="path"/> is relative, returns full path based in application directory.
        /// Otherwise returns the <paramref name="path"/> as it was passed in.
        /// </summary>
        /// <param name="path">Path.</param>
        private string GetFullPath(string path)
        {
            if (SystemIO.Path.IsPathRooted(path))
            {
                return path;
            }

            return SystemIO.Path.Combine(Program.AppDirectory, path);
        }


        private bool IsTeamCityOutputRequested()
        {
            return mArguments.Count == 3 && mArguments[2].Equals("TeamCityOutput", StringComparison.OrdinalIgnoreCase);
        }
    }
}
