using System.Collections.Generic;
using System.Linq;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Represents list of issues.
    /// </summary>
    public class Issues : List<Issue>
    {
        /// <summary>
        /// Adds new issue with the <paramref name="errorMessage"/> for given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="errorMessage">Error message.</param>
        public void Add(string fileName, string errorMessage)
        {
            var issue = new Issue(fileName, errorMessage);
            Add(issue);
        }


        /// <summary>
        /// Adds issues with the <paramref name="errorMessage"/> for all given <paramref name="fileNames"/>.
        /// </summary>
        /// <param name="fileNames">File names.</param>
        /// <param name="errorMessage">Error message same for all files.</param>
        public void AddRange(IEnumerable<string> fileNames, string errorMessage)
        {
            var redundantFilesIssues = fileNames.Select(fileName => new Issue(fileName, errorMessage));
            AddRange(redundantFilesIssues);
        }
    }
}
