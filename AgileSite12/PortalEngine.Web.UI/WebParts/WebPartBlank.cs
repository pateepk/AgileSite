using System;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part to supplement the web part by empty web part.
    /// </summary>
    internal class WebPartBlank : CMSAbstractWebPart
    {
        /// <summary>
        /// Web part container object - Blank web part never shows the container.
        /// </summary>
        public override WebPartContainerInfo Container
        {
            get
            {
                return null;
            }
            set
            {
                base.Container = value;
            }
        }


        /// <summary>
        /// Content before
        /// </summary>
        public override string ContentBefore
        {
            get
            {
                return null;
            }
            set
            {
                base.ContentBefore = value;
            }
        }


        /// <summary>
        /// Content after
        /// </summary>
        public override string ContentAfter
        {
            get
            {
                return null;
            }
            set
            {
                base.ContentAfter = value;
            }
        }
    }
}