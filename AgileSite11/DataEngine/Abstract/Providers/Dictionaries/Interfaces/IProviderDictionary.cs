namespace CMS.DataEngine
{
    /// <summary>
    /// Exposes required members for access to non-generic implementation of provider dictionary.
    /// </summary>
    public interface IProviderDictionary
    {
        /// <summary>
        /// Items count.
        /// </summary>
        long Count
        {
            get;
        }


        /// <summary>
        /// Dictionary name.
        /// </summary>
        string Name
        {
            get;
        }


        /// <summary>
        /// Object type
        /// </summary>
        string ObjectType
        {
            get;
        }


        /// <summary>
        /// Column names
        /// </summary>
        string ColumnNames
        {
            get;
        }


        /// <summary>
        /// Returns true if the dictionary content is valid.
        /// </summary>
        bool DataIsValid
        {
            get;
        }


        /// <summary>
        /// Removes the specified object.
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="logTasks">If true, logs the web farm tasks</param>
        void Remove(object key, bool logTasks);


        /// <summary>
        /// Clears all the items.
        /// </summary>
        /// <param name="logTasks">If true, logs the web farm tasks</param>
        void Clear(bool logTasks);


        /// <summary>
        /// Invalidates the dictionary content.
        /// </summary>
        /// <param name="logTasks">If true, logs the web farm tasks</param>
        void Invalidate(bool logTasks);
    }
}