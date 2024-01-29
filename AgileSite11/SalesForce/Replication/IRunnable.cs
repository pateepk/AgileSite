namespace CMS.SalesForce
{

    /// <summary>
    /// Represents an operation that could be executed.
    /// </summary>
    public interface IRunnable
    {

        #region "Methods"

        /// <summary>
        /// Executes the operation.
        /// </summary>
        void Run();

        #endregion

    }

}