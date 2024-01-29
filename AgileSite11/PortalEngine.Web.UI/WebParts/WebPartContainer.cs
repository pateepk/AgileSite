using System;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Base;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part container representation.
    /// </summary>
    public class WebPartContainer : Panel, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// Container info object.
        /// </summary>
        protected WebPartContainerInfo mContainer = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Container name.
        /// </summary>
        public string ContainerName
        {
            get;
            set;
        }


        /// <summary>
        /// Container title.
        /// </summary>
        public string ContainerTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Container CSS class.
        /// </summary>
        public string ContainerCSSClass
        {
            get;
            set;
        }


        /// <summary>
        /// Container custom content.
        /// </summary>
        public string ContainerCustomContent
        {
            get;
            set;
        }


        /// <summary>
        /// Web part container object.
        /// </summary>
        public WebPartContainerInfo Container
        {
            get
            {
                // Check if the correct container was loaded
                if ((mContainer == null) || (ContainerName.EqualsCSafe(mContainer.ContainerName, true)))
                {
                    // Get the container
                    mContainer = null;
                    if (ContainerName != "")
                    {
                        mContainer = WebPartContainerInfoProvider.GetWebPartContainerInfo(ContainerName);
                    }
                }
                return mContainer;
            }
            set
            {
                mContainer = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreRender handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register the container
            PortalContext.CurrentComponents.RegisterWebPartContainer(Container);
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            WebPartContainerInfo container = Container;
            if (Context != null)
            {
                // Text before
                if (container != null)
                {
                    // Resolve container macros
                    writer.Write(container.ContainerTextBefore.Replace("{%ContainerTitle%}", ContainerTitle).Replace("{%ContainerCSSClass%}", ContainerCSSClass).Replace("{%ContainerCustomContent%}", ContainerCustomContent));
                }
            }

            base.RenderChildren(writer);

            if (Context != null)
            {
                // Text after
                if (container != null)
                {
                    // Resolve container macros
                    writer.Write(container.ContainerTextAfter.Replace("{%ContainerTitle%}", ContainerTitle).Replace("{%ContainerCSSClass%}", ContainerCSSClass).Replace("{%ContainerCustomContent%}", ContainerCustomContent));
                }
            }
        }

        #endregion
    }
}