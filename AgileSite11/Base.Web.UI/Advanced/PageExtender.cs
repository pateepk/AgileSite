using System;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base class for page extenders
    /// </summary>
    public abstract class PageExtender
    {
        /// <summary>
        /// Extender page
        /// </summary>
        public Page Page
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes the extender using the specified page.
        /// </summary>
        /// <param name="page">Page to be extended</param>
        public virtual void Init(Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            Page = page;

            OnInit();
        }

        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public abstract void OnInit();
    }


    /// <summary>
    /// Generic page extender 
    /// </summary>
    /// <typeparam name="TPage">Type of the page to be extended</typeparam>
    public abstract class PageExtender<TPage> : PageExtender where TPage : Page
    {
        private bool initialized = false;


        /// <summary>
        /// Gets the extended control.
        /// </summary>
        public new TPage Page
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes the extender using the specified control.
        /// </summary>
        /// <param name="page">Control to be extended</param>
        public override void Init(Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            if (initialized)
            {
                return;
            }

            if (page is TPage)
            {
                // Cast the page to the required type
                Page = (TPage)page;
            }

            // Init was not successfull, throw exception
            if (Page == null)
            {
                throw GetInitFailedException(page);
            }

            OnInit();
            initialized = true;
        }


        private Exception GetInitFailedException(Page page)
        {
            var message = string.Format("[{0}.Init]: Unable to init page extender of type {1}. Please review the extender settings.", GetType().FullName, page.GetType().FullName);
            return new Exception(message);
        }
    }
}
