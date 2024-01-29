using System;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Base;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Placeholder for the layout content targeted to a different placeholder.
    /// </summary>
    [ToolboxData("<{0}:CMSContent runat=server></{0}:CMSContent>")]
    // ****
    // IMPORTANT: DO NOT remove the class in spite of the fact that it's obsolete. Removing it completely would introduce a breaking change and we have no work-around to offer.
    // ****
    [Obsolete("This control is not fully tested and documented, therefore it is no longer recommended to use.")]
    public class CMSContent : CMSPlaceHolder
    {
        #region "Variables"

        /// <summary>
        /// True if the content was relocated to the placeholder (to prevent double relocation and clearing the content)
        /// </summary>
        protected bool mContentRelocated = false;

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the content is displayed in case the target placeholder is found.
        /// </summary>
        public bool ShowWhenTargetNotFound
        {
            get;
            set;
        }


        /// <summary>
        /// Target page placeholder ID.
        /// </summary>
        public virtual string PagePlaceholderID
        {
            get;
            set;
        }


        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        public virtual CMSPagePlaceholder PagePlaceholder
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mPagePlaceholder;
                }

                if (mPagePlaceholder == null)
                {
                    mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    if (mPagePlaceholder == null)
                    {
                        throw new Exception("[CMSContent.PagePlaceholder]: Parent CMSPagePlaceholder not found.");
                    }
                }
                return mPagePlaceholder;
            }
            set
            {
                mPagePlaceholder = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Relocates the content to the appropriate placeholder.
        /// </summary>
        public void RelocateContent()
        {
            // Relocate if not yet in the target placeholder
            if (!mContentRelocated && !String.IsNullOrEmpty(PagePlaceholderID) && !PagePlaceholderID.EqualsCSafe(PagePlaceholder.PagePlaceholderID, true))
            {
                CMSPagePlaceholder target = (CMSPagePlaceholder)PagePlaceholder.SiblingPagePlaceholders[PagePlaceholderID.ToLowerCSafe()];
                if (target != null)
                {
                    // Load empty layout
                    CMSAbstractLayout layout = target.LoadLayout(null);
                    if (layout != null)
                    {
                        // Move the controls to the target layout
                        ControlsHelper.MoveControls(this, layout, null);

                        // Load the content to the layout
                        target.PageInfo = PagePlaceholder.PageInfo;
                        target.AllowDesignModeActions = false;
                        target.HasExternalContent = true;

                        mContentRelocated = true;
                    }
                }
                else if ((PagePlaceholder.ViewMode != ViewModeEnum.Design) && !ShowWhenTargetNotFound)
                {
                    // Clear the controls in case target was not found
                    Controls.Clear();
                }
            }
        }

        #endregion
    }
}