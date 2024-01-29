using System;

using CMS.Base.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal context menu base class.
    /// </summary>
    public abstract class CMSAbstractPortalUserControl : AbstractUserControl
    {
        #region "Variables"

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder = null;

        /// <summary>
        /// Parent page manager.
        /// </summary>
        protected IPageManager mPageManager = null;

        /// <summary>
        /// Portal manager.
        /// </summary>
        protected CMSPortalManager mPortalManager = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Parent page manager.
        /// </summary>
        public virtual IPageManager PageManager
        {
            get
            {
                if (mPageManager == null)
                {
                    if (mPagePlaceholder != null)
                    {
                        mPageManager = mPagePlaceholder;
                    }
                    else
                    {
                        // Try to find placeholder first
                        mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                        if (mPagePlaceholder != null)
                        {
                            mPageManager = mPagePlaceholder;
                        }
                        else
                        {
                            // Try to find PageManager within the page
                            mPageManager = PortalHelper.FindPageManager(Page);
                        }
                    }
                    // If no manager found, throw exception
                    if (mPageManager == null)
                    {
                        throw new Exception("[CMSAbstractWebPart.PageManager]: Parent PageManager not found.");
                    }
                }
                return mPageManager;
            }
        }


        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        public virtual CMSPagePlaceholder PagePlaceholder
        {
            get
            {
                if (mPagePlaceholder == null)
                {
                    mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    if (mPagePlaceholder == null)
                    {
                        throw new Exception("[CMSAbstractWebPart.PagePlaceholder]: Parent CMSPagePlaceholder not found.");
                    }
                }
                return mPagePlaceholder;
            }
            set
            {
                mPagePlaceholder = value;
            }
        }


        /// <summary>
        /// Portal manager for the page.
        /// </summary>
        public virtual CMSPortalManager PortalManager
        {
            get
            {
                if (mPortalManager == null)
                {
                    mPortalManager = PagePlaceholder.PortalManager;
                }
                return mPortalManager;
            }
            set
            {
                mPortalManager = value;
            }
        }

        #endregion
    }
}