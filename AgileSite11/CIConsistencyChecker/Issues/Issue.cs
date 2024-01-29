namespace CIConsistencyChecker
{
    /// <summary>
    /// Represents issue in CI repository.
    /// </summary>
    public class Issue
    {

        /// <summary>
        /// File name.
        /// </summary>
        public string FileName { get; private set; }


        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage { get; private set; }


        /// <summary>
        /// Creates new instance of <see cref="Issue"/>.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="errorMessage">Error message.</param>
        public Issue(string fileName, string errorMessage)
        {
            FileName = fileName;
            ErrorMessage = errorMessage;
        }
    }
}
