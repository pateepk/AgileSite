using System;
using System.Web;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides an instance of the HttpContext associated with the current request.
    /// </summary>
    /// <remarks>
    /// The instance could be set programmatically to support testing scenarios.
    /// </remarks>
    public class CMSHttpContext
    {
#if NETFULLFRAMEWORK
        private const string HTTP_WRAPPER_KEY = "Kentico_CMS_HttpWrapper";


        [ThreadStatic]
        private static HttpContextBase mContext;


        /// <summary>
        /// Gets an instance of the <see cref="HttpContextBase"/> class associated with the current request.
        /// </summary>
        /// <remarks>
        /// Property should be used just in <see cref="ContextContainer{TParent}"/> implementation to avoid context switching in method <see cref="ContextContainer{TParent}.EnsureContextContainer"/> when the <see cref="Current"/> is mocked in tests.
        /// </remarks>
        internal static HttpContextBase CurrentInternal
        {
            get
            {
                var currentContext = HttpContext.Current;
                if (currentContext != null)
                {
                    var wrapper = currentContext.Items[HTTP_WRAPPER_KEY] as HttpContextBase;
                    if (wrapper == null)
                    {
                        wrapper = new HttpContextWrapper(currentContext);
                        currentContext.Items[HTTP_WRAPPER_KEY] = wrapper;
                    }

                    return wrapper;
                }

                return null;
            }
        }


        /// <summary>
        /// Gets or sets an instance of the <see cref="HttpContextBase"/> class associated with the current request.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe, however, multiple instances of the <see cref="HttpContextBase"/> class could be created simultaneously.
        /// </remarks>
        public static HttpContextBase Current
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return CurrentInternal;
                }

                return mContext;
            }
            internal set
            {
                if (HttpContext.Current != null)
                {
                    throw new InvalidOperationException("Property setter cannot be used within a web request.");
                }

                mContext = value;
            }
        }
#endif

        /// <summary>
        /// Provides an instance of the <see cref="IHttpContext"/> class associated with the current request.
        /// </summary>
        internal static IHttpContext CurrentStandard
        {
            get
            {
                if (!AppCore.PreInitialized && !Service.IsRegistered(typeof(IHttpContextAccessor)))
                {
                    return null;
                }

                return Service.Resolve<IHttpContextAccessor>().HttpContext;
            }
        }


#if NETSTANDARD

        /// <summary>
        /// Provides an instance of the <see cref="IHttpContext"/> class associated with the current request.
        /// </summary>
        public static IHttpContext Current
        {
            get
            {
                return CurrentStandard;
            }
        }

        
        /// <summary>
        /// Provides an instance of the <see cref="IHttpContext"/> class associated with the current request.
        /// </summary>
        internal static IHttpContext CurrentInternal
        {
            get
            {
               return CurrentStandard;
            }
        }

#endif
    }
}