namespace CMS.Scheduler
{
    /// <summary>
    /// Defines a common interface for scheduled tasks.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Executes the task given in a task info.
        /// </summary>
        /// <param name="task">Container with task information</param>
        /// <returns>Textual description of task run's failure if any.</returns>
        string Execute(TaskInfo task);
    }
}