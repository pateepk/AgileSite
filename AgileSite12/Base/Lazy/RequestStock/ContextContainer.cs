using System;

using CMS.Core;
using CMS.Helpers;

namespace CMS.Base
{
    /// <summary>
    /// General container for request data
    /// </summary>
    public class ContextContainer<TParent> : AbstractHierarchicalObject<TParent>, IContextContainer
        where TParent : ContextContainer<TParent>, new()
    {
        #region "Variables"

        /// <summary>
        /// Unique container key
        /// </summary>
        internal static readonly string mKey = "Current" + typeof(TParent).Name;

        /// <summary>
        /// If true (default), the object is added to the thread containers so that it is cloned for a new thread
        /// </summary>
        protected static bool AddToContainers = true;

        /// <summary>
        /// Current context for the given thread.
        /// </summary>
        [ThreadStatic]
        private static TParent mThreadCurrent;


        #endregion


        #region "Properties"

        /// <summary>
        /// Unique container key
        /// </summary>
        public string Key
        {
            get
            {
                return mKey;
            }
        }


        /// <summary>
        /// Returns true if the context is the default request context
        /// </summary>
        public bool IsDefault
        {
            get;
            protected set;
        }


        /// <summary>
        /// Current request data
        /// </summary>
        protected internal static TParent Current
        {
            get
            {
                return EnsureContextContainer();
            }
            set
            {
                var ctx = CMSHttpContext.Current;
                if (ctx != null)
                {
                    ctx.Items[mKey] = value;
                }
                else
                {
                    mThreadCurrent = value;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures current <see cref="ContextContainer{TParent}"/>.
        /// </summary>
        /// <remarks>
        /// If not available create and register the container of the specified type.
        /// </remarks>
        internal static TParent EnsureContextContainer()
        {
            TParent result;
            var ctx = CMSHttpContext.CurrentInternal;
            if (ctx != null)
            {
                result = (TParent)ctx.Items[mKey];
                if (result == null)
                {
                    result = CreateNew();
                    ctx.Items[mKey] = result;
                }
            }
            else
            {
                result = mThreadCurrent;
                if (result == null)
                {
                    result = CreateNew();
                    mThreadCurrent = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Static constructor
        /// </summary>
        static ContextContainer()
        {
            TypeManager.RegisterGenericType(typeof(ContextContainer<TParent>));
        }


        /// <summary>
        /// Creates a new object for the current thread
        /// </summary>
        private static TParent CreateNew()
        {
            // Create the container
            var result = new TParent
            {
                IsDefault = true,
            };


            // Add to context containers so that a new thread can easily clone the context
            if (AddToContainers)
            {
                ThreadContext.CurrentContainers[mKey] = result;
            }

            return result;
        }


        /// <summary>
        /// Clears the current context value
        /// </summary>
        public static void Clear()
        {
            mThreadCurrent = null;

            // Clear HTTP context value
            var httpContext = CMSHttpContext.Current;
            if (httpContext != null)
            {
                httpContext.Items[mKey] = null;
            }
        }


        /// <summary>
        /// Clears the current context value
        /// </summary>
        public void ClearCurrent()
        {
            Clear();
        }


        /// <summary>
        /// Ensures the value
        /// </summary>
        /// <param name="value">Value to ensure</param>
        /// <param name="initializer">Value initializer</param>
        protected static TValue EnsureValue<TValue>(ref TValue value, Func<TValue> initializer)
            where TValue : class
        {
            return value ?? (value = initializer());
        }


        /// <summary>
        /// Sets the current instance as the current thread item
        /// </summary>
        public virtual void SetAsCurrent()
        {
            Current = (TParent)this;
        }


        /// <summary>
        /// Gets the current container from the provided thread
        /// </summary>
        internal static TParent FromThread(CMSThread thr)
        {
            if ((thr == null) || (thr.Context == null))
            {
                return null;
            }

            return (TParent)thr.Context.Containers[mKey];
        }


        /// <summary>
        /// Creates the internal <see cref="ContextContainer{TParent}"/> storage if it does not exist yet.
        /// </summary>
        public void EnsureCurrent()
        {
            // Forces current context to be copied into current thread context, so it will be cloned when a new thread is started.
            EnsureContextContainer();
        }
        
        #endregion
    }
}
