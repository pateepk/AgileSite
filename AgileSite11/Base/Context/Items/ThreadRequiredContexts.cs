using System;
using System.Collections.Generic;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Holds collection of required <see cref="IContextContainer"/>s and provides initialization for <see cref="CMSThread"/>.
    /// </summary>
    /// <threadsafety static="false" instance="false"/>
    internal sealed class ThreadRequiredContexts
    {
        private static List<IContextContainer> mThreadRequiredContextCollection;


        /// <summary>
        /// Collection of registered contexts.
        /// </summary>
        private static IList<IContextContainer> ThreadRequiredContextCollection
        {
            get
            {
                if (mThreadRequiredContextCollection == null)
                {
                    LazyInitializer.EnsureInitialized(ref mThreadRequiredContextCollection);
                }

                return mThreadRequiredContextCollection;
            }
        }


        /// <summary>
        /// Registers <see cref="IContextContainer"/> that needs to be ensured for every <see cref="CMSThread"/>.
        /// </summary>
        /// <param name="contextContainer">Context that needs to be initialized and copied into every new CMSThread</param>
        internal static void Register(IContextContainer contextContainer)
        {
            ThreadRequiredContextCollection.Add(contextContainer);
        }


        /// <summary>
        /// Ensures registered contexts into current <see cref="ThreadContext"/>, so they can be copied into new <see cref="CMSThread"/>.
        /// </summary>
        internal static void EnsureContextsForThread()
        {
            foreach (var contextContainer in ThreadRequiredContextCollection)
            {
                contextContainer.EnsureCurrent();
            }
        }
    }
}
