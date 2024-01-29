namespace CMS.DataEngine
{
    /// <summary>
    /// Defines info provider which holds count of related object.
    /// </summary>
    public interface IRelatedObjectCountProvider
    {
        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        void RefreshObjectsCounts();
    }
}