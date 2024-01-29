using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Enumeration of the thread modes
    /// </summary>
    public enum ThreadModeEnum
    {
        /// <summary>
        /// Asynchronous execution within thread newly created from CMSThread or request. Ensures the new thread proper context.
        /// </summary>
        Async = 1,

        /// <summary>
        /// Synchronous execution within existing CMSThread or request. Keeps the execution context of current thread.
        /// </summary>
        Sync = 2,

        /// <summary>
        /// Synchronous execution within existing anonymous thread. Ensures the anonymous thread proper context as it was a CMSThread.
        /// </summary>
        Wrapper = 3
    }
}
