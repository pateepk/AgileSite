namespace CMS.Base
{
    /// <summary>
    /// Interface to mark the objects to be cloned for new thread items
    /// </summary>
    public interface ICloneThreadItem
    {
        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        object CloneForNewThread();
    }
}
