using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page classs for the modal pages which should be available for global administrators only.
    /// </summary>
    public abstract class CMSModalGlobalAdminPage : GlobalAdminPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CMSModalGlobalAdminPage()
        {
            Load += new EventHandler(CMSModalGlobalAdminPage_Load);
            IsDialog = true;
        }


        /// <summary>
        /// Page load event
        /// </summary>
        protected void CMSModalGlobalAdminPage_Load(object sender, EventArgs e)
        {
            RegisterEscScript();
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterModalPageScripts();
        }
    }
}