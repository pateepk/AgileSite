using System.Collections.Specialized;

namespace CMS.Base
{
    /// <summary>
    /// Defines implementation providing access to session-state values and session-level settings.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Gets or sets the time, in minutes, that can elapse between requests before the session-state provider ends the session.
        /// </summary>
        int Timeout { get; }


        /// <summary>
        /// Gets a collection of the keys for all values that are stored in the session-state collection.
        /// </summary>
        NameObjectCollectionBase.KeysCollection Keys { get; }


        /// <summary>
        /// Gets the unique identifier for the session.
        /// </summary>
        string SessionID { get; }


        /// <summary>
        /// Indicates whether the session is read-only or not.
        /// </summary>
        bool IsReadOnly { get; }


        /// <summary>
        /// Cancels the current session.
        /// </summary>
        void Abandon();


        /// <summary>
        /// Removes all keys and values from the session-state collection.
        /// </summary>
        void Clear();


        /// <summary>
        /// Deletes an item from the session-state collection.
        /// </summary>
        void Remove(string name);


        /// <summary>
        /// Gets or sets a session value by using the specified name.
        /// </summary>
        /// <param name="name">The key name of the session value.</param>
        object this[string name] { get; set; }
    }
}
