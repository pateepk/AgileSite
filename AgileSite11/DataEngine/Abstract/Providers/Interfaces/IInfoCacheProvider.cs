namespace CMS.DataEngine
{
    /// <summary>
    /// Defines methods for type specific info cache operations.
    /// </summary>
    internal interface IInfoCacheProvider<in TInfo> : IInfoCacheProvider
        where TInfo : IInfo
    {
        /// <summary>
        /// Updates the object instance in the hashtables. Updates is different than register, because it logs task about changing object.
        /// </summary>
        void UpdateObjectInHashtables(TInfo info);


        /// <summary>
        /// Deletes the object instance from the hashtables.
        /// </summary>
        void DeleteObjectFromHashtables(TInfo info);
    }


    /// <summary>
    /// Defines methods for general info cache operations.
    /// </summary>
    internal interface IInfoCacheProvider
    {
        /// <summary>
        /// Clears the object's hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        void ClearHashtables(bool logTasks);
    }
}