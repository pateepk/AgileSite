namespace CMS.Base
{
    /// <summary>
    /// Request container interface
    /// </summary>
    internal interface IContextContainer
    {
        /// <summary>
        /// Unique container key.
        /// </summary>
        string Key
        {
            get;
        }


        /// <summary>
        /// Clears the current container.
        /// </summary>
        void ClearCurrent();


        /// <summary>
        /// Sets the current instance as the current thread item.
        /// </summary>
        void SetAsCurrent();


        /// <summary>
        /// Creates the internal <see cref="ContextContainer{TParent}"/> storage if it does not exist yet.
        /// </summary>
        void EnsureCurrent();
    }
}