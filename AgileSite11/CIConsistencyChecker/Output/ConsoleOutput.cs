using System;
using System.Collections.Generic;

namespace CIConsistencyChecker
{
    internal class ConsoleOutput : IOutput
    {
        public void LogErrors(IEnumerable<Issue> issues)
        {
            Console.WriteLine("Issues:");

            foreach (var issue in issues)
            {
                Console.WriteLine(issue.FileName);
                Console.Write(issue.ErrorMessage);
            }
        }


        public void LogInfo(string message)
        {
            Console.WriteLine("{0} | {1}", DateTime.Now, message);
        }
    }
}
