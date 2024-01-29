namespace CMS.SalesForce
{

    /// <summary>
    /// Provides instances of SalesForce client.
    /// </summary>
    public interface ISalesForceClientProvider
    {

        #region "Methods"

        /// <summary>
        /// Creates a new instance of the SalesForce client, and returns it.
        /// </summary>
        /// <returns>A new instance of the SalesForce client.</returns>
        SalesForceClient CreateClient();

        #endregion

    }

}