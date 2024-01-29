namespace CMS
{
    /// <summary>
    /// Defines a common interface for simple worker tasks
    /// </summary>
    public interface IWorkerTask
    {
        /// <summary>
        /// Executes the task
        /// </summary>
        /// <returns>Textual description of task run's failure if any.</returns>
        string Execute();
    }
}