namespace CMS.SalesForce
{

    /// <summary>
    /// Represents the interface for classes that provide SalesForce organization sessions.
    /// </summary>
    public interface ISessionProvider
    {

        #region "Methods"

        /// <summary>
        /// Creates a new SalesForce organization session, and returns it.
        /// </summary>
        /// <returns>A new SalesForce organization session.</returns>
        Session CreateSession();

        #endregion

    }

}