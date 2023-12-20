using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CMS.ContinuousIntegration;
using CMS.DataEngine;

using SystemIO = System.IO;

namespace CIConsistencyChecker
{
    internal class Program
    {
        private IEnumerable<Issue> Issues { get; set; }


        private Arguments Arguments { get; set; }


        private IOutput Output { get; set; }


        public static string AppDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }


        private static void Main(string[] args)
        {
            new Program()
                .ProccessArguments(args)
                .InitializeCmsApplication()
                .SerializeDatabase()
                .CompareRepositories()
                .CopyFilesWithIssuesToArtifactsFolder()
                .DisplayIssues()
                .GenerateTestResults();
        }

        /// <summary>
        /// Parses <paramref name="args"/>.
        /// </summary>
        private Program ProccessArguments(string[] args)
        {
            Arguments = new ArgumentsProcessor(args).Process();

            if (Arguments.TeamCityOutput)
            {
                Output = new TeamCityOutput(Console.WriteLine);
            }
            else
            {
                Output = new ConsoleOutput();
            }

            return this;
        }


        /// <summary>
        /// Initializes CMS application to allow calling its API.
        /// </summary>
        private Program InitializeCmsApplication()
        {
            Output.LogInfo("Initializing CMS App.");

            ConnectionHelper.ConnectionString = Arguments.ConnectionString;
            CMSApplication.Init();

            return this;
        }


        /// <summary>
        /// Serializes database to the CIrepository files.
        /// </summary>
        private Program SerializeDatabase()
        {
            Output.LogInfo("Serializing database.");

            FileSystemRepositoryManager.StoreAll();
            
            return this;
        }


        /// <summary>
        /// Checks whether serialized databases are same.
        /// </summary>
        private Program CompareRepositories()
        {
            Output.LogInfo("Comparing repositories.");

            var comparator = new DirectoryContentComparator(Arguments.OriginalRepositoryFullPath, Arguments.NewRepositoryFullPath);
            Issues = comparator.Compare();

            return this;
        }


        /// <summary>
        /// Goes through all issues found and copies files (from both repositories) which have issues into Artifacts folder.
        /// </summary>
        private Program CopyFilesWithIssuesToArtifactsFolder()
        {
            var artifactsDirectoryFullPath = SystemIO.Path.Combine(AppDirectory, @".\Artifacts\");
            var originalRepositoryArtifactsFullPath = SystemIO.Path.Combine(artifactsDirectoryFullPath, "CIRepository_TFS");
            var newRepositoryArtifactsFullPath = SystemIO.Path.Combine(artifactsDirectoryFullPath, "CIRepository_Serialized");

            var originalFileCopier = new FileCopier(Arguments.OriginalRepositoryFullPath, originalRepositoryArtifactsFullPath);
            var newFileCopier = new FileCopier(Arguments.NewRepositoryFullPath, newRepositoryArtifactsFullPath);

            foreach (var issue in Issues)
            {
                originalFileCopier.TryCopy(issue.FileName);
                newFileCopier.TryCopy(issue.FileName);
            }

            return this;
        }


        /// <summary>
        /// Displays all differences between repositories.
        /// </summary>
        private Program DisplayIssues()
        {
            if (Issues.Any())
            {
                Output.LogErrors(Issues);
            }
            else
            {
                Output.LogInfo("No issues detected.");
            }

            return this;
        }


        /// <summary>
        /// Generates TEST-result.xml file as report for Azure DevOps services
        /// </summary>
        private Program GenerateTestResults()
        {
            if(Issues.Any())
            {
                var testResult = new XElement("test-run",
                    new XAttribute("id", "2"),
                    new XAttribute("start-time", $"{DateTime.Now:u}"),
                    new XAttribute("end-time", $"{DateTime.Now:u}"),
                    new XElement("test-suite",
                        new XAttribute("id", "1-1"),
                        new XAttribute("type", "Assembly"),
                        new XAttribute("name", "Consistency tests"),
                        Issues.Select((issue, index) =>
                            new XElement("test-case",
                                new XAttribute("id", $"1-{index}"),
                                new XAttribute("result", "Failed"),
                                new XAttribute("name", $"File: {issue.FileName}"),
                                new XAttribute("fullname", $"File: {issue.FileName}"),
                                new XElement("failure",
                                    new XElement("message", new XCData("Files do not match. See the message below and files in Artifacts of this build for more details.")),
                                    new XElement("stack-trace", new XCData(issue.ErrorMessage))
                                )
                            )
                        )
                    )
                );

                Output.LogInfo(testResult.ToString());
                testResult.Save("./TEST-result.xml");
            }

            return this;
        }


    }
}