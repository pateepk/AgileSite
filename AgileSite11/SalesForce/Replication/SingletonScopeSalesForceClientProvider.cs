namespace CMS.SalesForce
{

    /// <summary>
    /// Provides a unique instance of the SalesForce client that is created on the first request.
    /// </summary>
    public sealed class SingletonScopeSalesForceClientProvider : ISalesForceClientProvider
    {

        #region "Private members"

        private ISessionProvider mSessionProvider;
        private SalesForceClient mClient;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the provider using the specified session provider.
        /// </summary>
        /// <param name="sessionProvider">The provider of SalesForce communication sessions.</param>
        public SingletonScopeSalesForceClientProvider(ISessionProvider sessionProvider)
        {
            mSessionProvider = sessionProvider;
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Returns a unique instance of the SalesForce client.
        /// </summary>
        /// <returns>A unique instance of the SalesForce client.</returns>
        public SalesForceClient CreateClient()
        {
            if (mClient == null)
            {
                Session session = mSessionProvider.CreateSession();
                mClient = new SalesForceClient(session);
                mClient.Options.AttributeTruncationEnabled = true;
            }

            return mClient;
        }

        #endregion

    }

}