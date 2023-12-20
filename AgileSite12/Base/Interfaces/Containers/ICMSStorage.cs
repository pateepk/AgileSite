namespace CMS.Base
{
    /// <summary>
    /// Interface to access the CMS objects and collections as storages
    /// </summary>
    public interface ICMSStorage
    {
        /// <summary>
        /// Returns true if the storage is disconnected
        /// </summary>
        bool IsDisconnected
        {
            get;
        }


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        bool IsCachedObject
        {
            get;
            set;
        }
    }
}