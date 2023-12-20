using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Provides helper methods for current page events and settings
    /// </summary>
    public class PageContext : AbstractContext<PageContext>, INotCopyThreadItem
    {
        #region "Const"

        private bool mPreRenderRaised;
        private bool mInitCompleteRaised;
        private bool mBeforeInitCompleteRaised;
        private bool mPageHandlersInitialized;

        private Page mCurrentPage;
        
        private List<EventHandler> mBeforeInitCompleteHandlers;
        private List<EventHandler> mPreRenderHandlers;
        private List<EventHandler> mInitCompleteHandlers;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether PreRender raised
        /// </summary>
        private static bool PreRenderRaised
        {
            get
            {
                return Current.mPreRenderRaised;
            }
            set
            {
                Current.mPreRenderRaised = value;
            }
        }


        /// <summary>
        /// Indicates whether InitComplete event was raised
        /// </summary>
        private static bool InitCompleteRaised
        {
            get
            {
                return Current.mInitCompleteRaised;
            }
            set
            {
                Current.mInitCompleteRaised = value;
            }
        }


        /// <summary>
        /// Indicates whether BeforeInitComplete event was raised
        /// </summary>
        private static bool BeforeInitCompleteRaised
        {
            get
            {
                return Current.mBeforeInitCompleteRaised;
            }
            set
            {
                Current.mBeforeInitCompleteRaised = value;
            }
        }


        /// <summary>
        /// Indicates whether page handlers are initialized
        /// </summary>
        private static bool PageHandlersInitialized
        {
            get
            {
                return Current.mPageHandlersInitialized;
            }
            set
            {
                Current.mPageHandlersInitialized = value;
            }
        }


        /// <summary>
        /// Current System.Web.UI.Page instance
        /// </summary>
        public static Page CurrentPage
        {
            get
            {
                var c = Current;
                var page = c.mCurrentPage;
                
                if (page == null)
                {
                    // Try get current page from current handler
                    var context = CMSHttpContext.Current;
                    if (context != null)
                    {
                        page = context.CurrentHandler as Page;
                        if (page != null)
                        {
                            c.mCurrentPage = page;
                            EnsureCurrentPageEvents(page);
                        }
                    }
                }

                return page;
            }
            set
            {
                Current.mCurrentPage = value;
                EnsureCurrentPageEvents(value);
            }
        }


        /// <summary>
        /// Ensures current page event handlers
        /// </summary>
        private static void EnsureCurrentPageEvents(Page page)
        {
            if ((page != null) && !PageHandlersInitialized)
            {
                page.InitComplete += CurrentPage_InitComplete;
                page.PreRender += CurrentPage_PreRender;

                PageHandlersInitialized = true;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// List of before Init complete event handlers
        /// </summary>
        private static List<EventHandler> BeforeInitCompleteHandlers
        {
            get
            {
                var c = Current;

                return c.mBeforeInitCompleteHandlers ?? (c.mBeforeInitCompleteHandlers = new List<EventHandler>());
            }
        }


        /// <summary>
        /// List of PreRender event handlers
        /// </summary>
        private static List<EventHandler> PreRenderHandlers
        {
            get
            {
                var c = Current;

                return c.mPreRenderHandlers ?? (c.mPreRenderHandlers = new List<EventHandler>());
            }
        }


        /// <summary>
        /// List of Init complete event handlers
        /// </summary>
        private static List<EventHandler> InitCompleteHandlers
        {
            get
            {
                var c = Current;

                return c.mInitCompleteHandlers ?? (c.mInitCompleteHandlers = new List<EventHandler>());
            }
        }


        /// <summary>
        /// Occurs when page initialization is complete (prioritized events). Precedes <see cref="PageContext.InitComplete"/>.
        /// </summary>
        public static event EventHandler BeforeInitComplete
        {
            add
            {
                if (!BeforeInitCompleteRaised)
                {
                    BeforeInitCompleteHandlers.Add(value);
                    EnsureCurrentPageEvents(CurrentPage);
                }
                else
                {
                    value.Invoke(CurrentPage, null);
                }
            }
            remove
            {
                BeforeInitCompleteHandlers.Remove(value);
            }
        }


        /// <summary>
        /// Occurs when page initialization is complete.
        /// </summary>
        public static event EventHandler InitComplete
        {
            add
            {
                if (!InitCompleteRaised)
                {
                    InitCompleteHandlers.Add(value);
                    EnsureCurrentPageEvents(CurrentPage);
                }
                else
                {
                    value.Invoke(CurrentPage, null);
                }
            }
            remove
            {
                InitCompleteHandlers.Remove(value);
            }
        }


        /// <summary>
        ///  Occurs in page pre-render phase.
        /// </summary>
        public static event EventHandler PreRender
        {
            add
            {
                if (!PreRenderRaised)
                {
                    PreRenderHandlers.Add(value);
                    EnsureCurrentPageEvents(CurrentPage);
                }
                else
                {
                    value.Invoke(CurrentPage, null);
                }
            }
            remove
            {
                PreRenderHandlers.Remove(value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raised on original PreRender event
        /// </summary>
        private static void CurrentPage_PreRender(object sender, EventArgs e)
        {
            // Remove default handler
            CurrentPage.PreRender -= CurrentPage_PreRender;

            // Flag that PreRender raised
            PreRenderRaised = true;

            var handlers = Current.mPreRenderHandlers;
            if (handlers != null)
            {
                foreach (EventHandler eh in handlers)
                {
                    eh.Invoke(CurrentPage, null);
                }

                handlers.Clear();
            }
        }


        /// <summary>
        /// Raised on original init complete event
        /// </summary>
        private static void CurrentPage_InitComplete(object sender, EventArgs e)
        {
            // Remove default handler
            CurrentPage.InitComplete -= CurrentPage_InitComplete;

            // Flag that before init raised
            BeforeInitCompleteRaised = true;

            var c = Current;

            // Before complete handlers
            var handlers = c.mBeforeInitCompleteHandlers;
            if (handlers != null)
            {
                // Invoke BeforeInitComplete 
                foreach (EventHandler eh in handlers)
                {
                    eh.Invoke(CurrentPage, null);
                }

                handlers.Clear();
            }

            // Flag that init raised
            InitCompleteRaised = true;

            // INIT complete handlers
            handlers = c.mInitCompleteHandlers;

            if (handlers != null)
            {
                // Invoke InitComplete 
                foreach (EventHandler eh in handlers)
                {
                    eh.Invoke(CurrentPage, null);
                }

                handlers.Clear();
            }
        }


        /// <summary>
        /// Resets the PageContext to default state.
        /// </summary>
        public static void Reset()
        {
            BeforeInitCompleteRaised = false;
            InitCompleteRaised = false;
            PreRenderRaised = false;

            var c = Current;
            var handlers = c.mInitCompleteHandlers;
            if (handlers != null)
            {
                handlers.Clear();
            }

            handlers = c.mPreRenderHandlers;
            if (handlers != null)
            {
                handlers.Clear();
            }

            handlers = c.mBeforeInitCompleteHandlers;
            if (handlers != null)
            {
                handlers.Clear();
            }

            c.mCurrentPage = null;
            c.mPageHandlersInitialized = false;
        }

        #endregion
    }
}
