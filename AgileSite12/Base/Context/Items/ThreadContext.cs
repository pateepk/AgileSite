using System;

using CMS.Helpers;

namespace CMS.Base
{
    /// <summary>
    /// Thread context
    /// </summary>
    internal class ThreadContext : AbstractContext<ThreadContext>, INotCopyThreadItem
    {
        #region "Variables"

        // Parent thread ID
        internal int ParentThreadID;

        // Parent thread type
        internal Type ParentThreadType;

        // If true, the items will be used by multiple threads, therefore cannot be used by a new thread directly.
        internal bool MultipleThreads;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current thread items
        /// </summary>
        public static ThreadItemsDictionaryInternal CurrentItems
        {
            get
            {
                return Current.Items;
            }
            set
            {
                Current.Items = value;
            }
        }


        /// <summary>
        /// Holds a collection of containers used within current thread.
        /// </summary>
        internal static ContextContainersDictionary CurrentContainers
        {
            get
            {
                var c = Current;

                return c.Containers ?? (c.Containers = new ContextContainersDictionary());
            }
            set
            {
                Current.Containers = value;
            }
        }


        /// <summary>
        /// Thread items.
        /// </summary>
        public ThreadItemsDictionaryInternal Items { get; set; }


        /// <summary>
        /// Holds a collection of containers used within current thread
        /// </summary>
        internal ContextContainersDictionary Containers { get; private set; }

        #endregion


        #region "Methods"

        /// <summary>
        ///  Ensures that current context is not added to the collection of current containers to avoid infinite loop, this one is handled separately by thread start.
        /// </summary>
        static ThreadContext()
        {
            AddToContainers = false;
        }


        /// <summary>
        /// Clears all containers in the current context.
        /// </summary>
        internal static void Reset()
        {
            var containers = CurrentContainers;

            foreach (var c in containers.TypedValues)
            {
                c.ClearCurrent();
            }

            Current.ClearCurrent();
        }


        /// <summary>
        /// Copies current context values to the <see cref="ThreadContext"/> to be available for new <see cref="CMSThread"/>.
        /// </summary>
        /// <returns><see cref="ThreadContext" /> instance for a new <see cref="CMSThread"/>.</returns>
        public ThreadContext CopyForNewThread()
        {
            var result = new ThreadContext();

            // Copy the thread items - prefer items from current HttpContext, if available
            if (CMSHttpContext.Current != null)
            {
                result.Items = ThreadItemsDictionaryInternal.Copy(CMSHttpContext.Current.Items);
            }
            else if (Items != null)
            {
                result.Items = Items.Copy();
            }

            ThreadRequiredContexts.EnsureContextsForThread();

            // Copy the containers
            if (Containers != null)
            {
                result.Containers = Containers.Copy();
            }

            return result;
        }


        /// <summary>
        /// Sets the current instance as the current thread item.
        /// </summary>
        public override void SetAsCurrent()
        {
            base.SetAsCurrent();

            if (Containers != null)
            {
                // Apply containers from the incoming context
                foreach (var container in Containers.TypedValues)
                {
                    container.SetAsCurrent();
                }
            }
        }


        /// <summary>
        /// Clones the object for new thread.
        /// </summary>
        public override object CloneForNewThread()
        {
            return CopyForNewThread();
        }


        /// <summary>
        /// Gets the context for a new thread
        /// </summary>
        /// <param name="multipleThreads">If true, the wrapped method may be used by multiple threads. Use this parameter to ensure that all the threads receive the original context values.</param>
        public static ThreadContext GetContextForNewThread(bool multipleThreads = false)
        {
            // Clone the context
            var result = (ThreadContext)Current.CloneForNewThread();

            result.MultipleThreads = multipleThreads;

            return result;
        }


        /// <summary>
        /// Sets the context parent thread.
        /// </summary>
        /// <param name="parentThreadId">Thread ID</param>
        /// <param name="parentThreadType">Thread type</param>
        public void SetParentThread(int parentThreadId, Type parentThreadType)
        {
            ParentThreadID = parentThreadId;
            ParentThreadType = parentThreadType;
        }

        #endregion
    }
}
