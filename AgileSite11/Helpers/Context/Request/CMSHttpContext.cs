using System;
using System.Web;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides an instance of the HttpContextBase class associated with the current request.
    /// </summary>
    /// <remarks>
    /// The instance could me set programmatically to support testing scenarios.
    /// </remarks>
    public class CMSHttpContext : AbstractContext<CMSHttpContext>, INotCopyThreadItem
    {
        #region "Variables"

        private HttpContextBase mCurrent;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets an instance of the HttpContextBase class associated with the current request.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe, however, multiple instances of the HttpContextBase class could be created simultaneously.
        /// </remarks>
        public static HttpContextBase CurrentOrThrow
        {
            get
            {
                // Standard system http context
                var current = Current;
                if (current != null)
                {
                    return current;
                }

                throw new NullReferenceException("[CMSHttpContext.Current]: Http context is null (code is not running in context of web application).");
            }
        }


        /// <summary>
        /// Gets or sets an instance of the HttpContextBase class associated with the current request.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe, however, multiple instances of the HttpContextBase class could be created simultaneously.
        /// </remarks>
        public static new HttpContextBase Current
        {
            get
            {
                var c = CurrentContext;

                if (c.mCurrent == null)
                {
                    // Create new context from current context
                    var http = HttpContext.Current;
                    if (http != null)
                    {
                        c.mCurrent = new HttpContextWrapper(http);
                    }

                    // Throw exception if necessary
                    if (c.mCurrent == null)
                    {
                        if (Default != null)
                        {
                            // Use default context
                            c.mCurrent = Default;
                        }
                        else if (ThrowExceptionWhenNull)
                        {
                            // Throw exception if none found
                            throw new NullReferenceException("[CMSHttpContext.Current]: Http context is null (code is not running in context of web application).");
                        }
                    }
                }

                // Return value which could be set to this property (support of automatic testing).
                return c.mCurrent;
            }
            set
            {
                CurrentContext.mCurrent = value;
            }
        }


        /// <summary>
        /// Default Http context which is used in case request Http context is not available
        /// </summary>
        public static HttpContextBase Default
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether exception should be thrown if CMSHttpContext.Current is null.
        /// </summary>
        public static bool ThrowExceptionWhenNull
        {
            get;
            set;
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Check whether current context is available (always no matter ThrowExceptionWhenNull) and then optionally whether request and response are available.
        /// </summary>
        /// <param name="checkRequest">Whether check Request property.</param>
        /// <param name="checkResponse">Whether check Response property.</param>        
        public static HttpContextBase GetCurrent(bool checkRequest, bool checkResponse)
        {
            if (Current == null)
            {
                throw new NullReferenceException("[CMSHttpContext.GetCurrent]: Http context is null (code is not running in context of web application).");
            }

            if (checkRequest && (Current.Request == null))
            {
                throw new NullReferenceException("[CMSHttpContext.GetCurrent]: HttpRequest is not available in current HttpContext.");
            }


            if (checkResponse && (Current.Response == null))
            {
                throw new NullReferenceException("[CMSHttpContext.GetCurrent]: HttpResponse is not available in current HttpContext.");
            }

            return Current;
        }
        
        #endregion
    }
}