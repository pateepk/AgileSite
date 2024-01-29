namespace CIConsistencyChecker
{
    /// <summary>
    /// Represents application arguments.
    /// </summary>
    internal class Arguments
    {
        /// <summary>
        /// Connection string to the database.
        /// </summary>
        public string ConnectionString { get; set; }


        /// <summary>
        /// Path to CI repository directory, where will be database serialized.
        /// </summary>
        public string NewRepositoryFullPath { get; set; }


        /// <summary>
        /// Path to CI repository directory from VCS.
        /// </summary>
        public string OriginalRepositoryFullPath { get; set; }


        /// <summary>
        /// Indicates whether process should write TeamCity-formatted output to standard output.
        /// </summary>
        public bool TeamCityOutput { get; set; } = false;
    }
}
