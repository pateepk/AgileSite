using System;
using System.Web.Caching;

namespace CMS.Helpers
{
    /// <summary>
    /// Wrapper class for a weakly referenced callback for the cache helper
    /// </summary>
    public class CacheDependencyCallback<TTarget> : ICacheDependencyCallback
        where TTarget : class
    {
        #region  "Variables"
        
        /// <summary>
        /// Target object as a weak reference
        /// </summary>
        protected WeakReference mTarget = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Cache callback key
        /// </summary>
        public string Key
        {
            get;
            protected set;
        }


        /// <summary>
        /// Event handler executed when the cache dependency is dropped
        /// </summary>
        public Action<TTarget, object> Handler
        {
            get;
            protected set;
        }


        /// <summary>
        /// Parameter passed to the handler
        /// </summary>
        public object Parameter 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Target object
        /// </summary>
        public TTarget Target
        {
            get
            {
                if (mTarget != null)
                {
                    return (TTarget)mTarget.Target;
                }

                return default(TTarget);
            }
            protected set
            {
                mTarget = (value != null) ? new WeakReference(value) : null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Cache callback key</param>
        /// <param name="target">Target object to pass as a parameter to the handler</param>
        /// <param name="handler">Handler to execute</param>
        /// <param name="parameter">Parameter passed to the handler</param>
        public CacheDependencyCallback(string key, TTarget target, Action<TTarget, object> handler, object parameter)
        {
            Key = key;
            Target = target;
            Handler = handler;
            Parameter = parameter;
        }
        

        /// <summary>
        /// Executes the callback to the target object
        /// </summary>
        public void PerformCallback()
        {
            var target = Target;
            if ((target != null) && (Handler != null))
            {
                Handler(target, Parameter);

                CacheDebug.LogCacheOperation("DoCallback", Key, null, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, false);
            }
        }

        #endregion
    }
}
