using System;
using System.Collections.Concurrent;

namespace CMS.DataEngine
{
    /// <summary>
    /// Factory for <see cref="ITransactionScope"/>.
    /// </summary>
    internal class TransactionScopeFactory
    {
        private static readonly Lazy<TransactionScopeFactory> mInstance = new Lazy<TransactionScopeFactory>(() => new TransactionScopeFactory());

        private readonly ConcurrentDictionary<Type, Func<ITransactionScope>> transactionFactoryMethods = new ConcurrentDictionary<Type, Func<ITransactionScope>>();


        private static TransactionScopeFactory Instance
        {
            get
            {
                return mInstance.Value;
            }
        }


        private TransactionScopeFactory()
        {
            
        }


        /// <summary>
        /// Returns a new transaction scope. 
        /// </summary>
        /// <param name="providerType">Type of a provider in whose the transaction should occur.</param>
        public static ITransactionScope GetTransactionScope(Type providerType)
        {
            return Instance.GetTransactionScopeInternal(providerType);
        }


        /// <summary>
        /// Registers transaction scope implementation specific for given <paramref name="providerType"/>.
        /// </summary>
        /// <param name="providerType">Type of a provider for whom the implementation should be used.</param>
        /// <param name="factoryMethod">Method for transaction scope implementation creation.</param>
        public static void RegisterTransaction(Type providerType, Func<ITransactionScope> factoryMethod)
        {
            Instance.RegisterTransactionInternal(providerType, factoryMethod);
        }


        private ITransactionScope GetTransactionScopeInternal(Type providerType)
        {
            Func<ITransactionScope> factoryMethod;
            if (providerType != null && transactionFactoryMethods.TryGetValue(providerType, out factoryMethod))
            {
                return factoryMethod();
            }

            return new CMSTransactionScope();
        }


        private void RegisterTransactionInternal(Type providerType, Func<ITransactionScope> factoryMethod)
        {
            if (providerType == null)
            {
                throw new ArgumentNullException("providerType");
            }

            if (factoryMethod == null)
            {
                throw new ArgumentNullException("factoryMethod");
            }

            transactionFactoryMethods[providerType] = factoryMethod;
        }
    }
}
