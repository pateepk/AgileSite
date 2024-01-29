using System;
using System.Web.UI;

using CMS.Base.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Context menu icon.
    /// </summary>
    public class ContextMenuButton : ContextMenuContainer
    {
        /// <summary>
        /// Action image.
        /// </summary>
        protected CMSImageButton mImage = null;


        /// <summary>
        /// Action image.
        /// </summary>
        public CMSImageButton Image
        {
            get
            {
                if (mImage == null)
                {
                    mImage = new CMSImageButton();

                    // Set icon attribute
                    mImage.ImageUrl = UIHelper.GetImageUrl(Page, "/Design/Controls/ContextMenu/Menu.png");
                }

                return mImage;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ContextMenuButton()
        {
            MouseButton = MouseButtonEnum.Both;
            RenderAsTag = HtmlTextWriterTag.A;

            VerticalPosition = VerticalPositionEnum.Bottom;
            HorizontalPosition = HorizontalPositionEnum.Left;

            CssClass = "ContextMenuButton";
            ActiveItemCssClass = "ContextMenuButtonActive";
        }


        /// <summary>
        /// Creates the child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Add(Image);
        }
    }
}
