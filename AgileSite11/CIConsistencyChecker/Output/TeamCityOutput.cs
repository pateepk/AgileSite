using System;
using System.Collections.Generic;

using JetBrains.TeamCity.ServiceMessages.Write.Special;

namespace CIConsistencyChecker
{
    internal class TeamCityOutput : IOutput
    {
        private readonly ITeamCityWriter mTeamCityWriter;


        public TeamCityOutput(Action<string> writerDestination)
        {
            mTeamCityWriter = new TeamCityServiceMessages().CreateWriter(writerDestination);
        }


        public void LogErrors(IEnumerable<Issue> issues)
        {

            using (var testSuiteWriter = mTeamCityWriter.OpenTestSuite("Consistency tests"))
            {
                foreach (var issue in issues)
                {
                    using (var testWriter = testSuiteWriter.OpenTest($"File: {issue.FileName}"))
                    {
                        testWriter.WriteFailed("Files do not match. See the message below and files in Artifacts of this build for more details.", issue.ErrorMessage);
                    }
                }
            }
        }


        public void LogInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}
