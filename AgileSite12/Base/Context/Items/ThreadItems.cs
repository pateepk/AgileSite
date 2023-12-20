using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Thread items collection.
    /// </summary>
    public class ThreadItems
    {
        /// <summary>
        /// If true, anonymous threads are allowed to work with their thread items
        /// </summary>
        internal static BoolAppSetting AllowAnonymousThreadItems = new BoolAppSetting("CMSAllowAnonymousThreadItems", true)
        {
            DefaultValueInitializer = () => !SystemContext.DevelopmentMode
        };


        /// <summary>
        /// Current items collection.
        /// </summary>
        public static IDictionary CurrentItems
        {
            get
            {
                return GetItems(true);
            }
        }


        /// <summary>
        /// Current items collection for read-only purposes, returns null if the items were not yet registered.
        /// </summary>
        public static IDictionary ReadOnlyCurrentItems
        {
            get
            {
                return GetItems(false);
            }
        }


        /// <summary>
        /// Ensures the items collection of the given thread.
        /// </summary>
        /// <param name="thread">Thread for which to ensure the items. Pass null for current thread.</param>
        public static void EnsureItems(CMSThread thread = null)
        {
            GetItems(true, true, thread);
        }


        /// <summary>
        /// Gets the items collection of a thread, optionally creates new ones
        /// </summary>
        /// <param name="createIfNotFound">If true, the items will be created if not found</param>
        /// <param name="allowAnonymous">If true, allows creation of the items in the anonymous thread</param>
        /// <param name="thread">Thread for which to get the items, or null for current thread.</param>
        private static ThreadItemsDictionaryInternal GetItems(bool createIfNotFound, bool allowAnonymous = false, CMSThread thread = null)
        {
            Thread systemThread;
            ThreadContext context;
            if (thread == null)
            {
                systemThread = Thread.CurrentThread;
                context = ThreadContext.Current;
            }
            else
            {
                systemThread = thread.InnerThread;
                context = thread.Context;
            }
            var items = context.Items;

            // Ensure the items collection
            if ((items == null) && createIfNotFound)
            {
                // Ensure thread items for the rest of the thread to make it functional (just without proper context)
                context.Items = items = new ThreadItemsDictionaryInternal();

                if (!allowAnonymous && !AllowAnonymousThreadItems)
                {
                    // Log the error
                    try
                    {
                        var message = new StringBuilder();

                        message.AppendLine("An anonymous thread (" + systemThread.ManagedThreadId + ") requested access to non-existing thread items, which is not allowed. Any operation using thread items should originate from CMSThread, use class CMSThread for any new thread created, or method CMSThread.Wrap to wrap an action to CMSThread if you pass it as an event handler. If you want to use the thread with empty context on purpose, call CMSThread.AllowEmptyContext(). If this error is thrown from the context of session serialization, you should ensure that this operation does not serialize other data than the data that is already loaded.");
                        message.AppendLine("Stack trace:");
                        message.Append(new StackTrace());

                        CoreServices.EventLog.LogEvent("E", "Thread", "ANONYMOUS", message.ToString());
                    }
                    catch
                    {
                        // Suppress exception to not kill the thread
                    }

                    return items;
                }
            }

            return items;
        }
    }
}