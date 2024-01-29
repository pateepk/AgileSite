using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class that manages the transaction scope, but doesn't start it until BeginTransaction is called.
    /// </summary>
    public class CMSLateBoundTransaction : IDisposable
    {
        /// <summary>
        /// Inner transaction scope
        /// </summary>
        private ITransactionScope mTransactionScope;

        private readonly Type mProviderType;


        /// <summary>
        /// Creates a new instance of <see cref="CMSLateBoundTransaction"/>, optionally in a context of given provider type.
        /// </summary>
        /// <param name="providerType">Type of the provider within whose context should the transaction start.</param>
        public CMSLateBoundTransaction(Type providerType = null)
        {
            mProviderType = providerType;
        }
        

        /// <summary>
        /// Begins the transaction
        /// </summary>
        public void BeginTransaction()
        {
            if (mTransactionScope == null)
            {
                mTransactionScope = TransactionScopeFactory.GetTransactionScope(mProviderType);
            }
        }


        /// <summary>
        /// Commits the transaction if it was activated
        /// </summary>
        public void Commit()
        {
            if (mTransactionScope != null)
            {
                mTransactionScope.Commit();
            }
        }


        /// <summary>
        /// Disposes the transaction
        /// </summary>
        public void Dispose()
        {
            if (mTransactionScope != null)
            {
                mTransactionScope.Dispose();
            }
        }
    }
}
