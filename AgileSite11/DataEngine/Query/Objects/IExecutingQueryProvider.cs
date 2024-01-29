namespace CMS.DataEngine
{
    /// <summary>
    /// Allows possibility to change the execution chain of <see cref="ObjectQuery"/>
    /// </summary>
    internal interface IExecutingQueryProvider
    {
        /// <summary>
        /// Returns execution query as <see cref="IDataQuery"/>
        /// </summary>
        IDataQuery GetExecutionQuery();


        /// <summary>
        /// Refreshes the underlying data affecting the query retrieval
        /// </summary>
        /// <param name="typeInfo">Type info holding all necessary data</param>
        void Refresh(ObjectTypeInfo typeInfo);
    }
}