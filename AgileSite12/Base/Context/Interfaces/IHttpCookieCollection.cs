using System.Collections;

namespace CMS.Base
{
    /// <summary>
    /// Defines implementation capable of handling collection of <see cref="IHttpCookie"/> objects.
    /// </summary>
    public interface IHttpCookieCollection : ICollection
    {
        /// <summary>
        /// Adds the specified cookie to the cookie collection.
        /// </summary>
        void Add(IHttpCookie cookie);


        /// <summary>
        /// Gets the cookie with the specified name from the cookie collection.
        /// </summary>
        IHttpCookie this[string name] { get; }


        /// <summary>
        /// Gets a string array containing all the keys (cookie names) in the cookie collection.
        /// </summary>
        string[] AllKeys { get; }


        /// <summary>
        /// Clears all cookies from the cookie collection.
        /// </summary>
        void Clear();


        /// <summary>
        /// Removes the cookie with the specified name from the collection.
        /// </summary>
        void Remove(string name);
    }
}
