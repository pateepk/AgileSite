using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the Designer pages to apply global settings to the pages.
    /// </summary>
    public abstract class DesignerPage : CMSModalPage
    {
        /// <summary>
        /// PageLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            RequireSite = false;

            base.OnLoad(e);
            Response.Cache.SetNoStore();
        }
    }
}