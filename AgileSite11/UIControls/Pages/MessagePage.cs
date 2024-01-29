using System;

using CMS.Helpers;
using CMS.DataEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for the message pages.
    /// </summary>
    public abstract class MessagePage : CMSPage
    {

        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public MessagePage()
        {
            // Ignore action requiring database if database is not available. Message page can be displayed for application without database (error within web install)
            if (ConnectionHelper.ConnectionAvailable)
            {
                SetCulture();
            }

            Load += BasePage_Load;
            Init += MessagePage_Init;
        }


        private void MessagePage_Init(object sender, EventArgs e)
        {
            // Prevent breadcrumbs refreshing in all message paging to keep current breadcrumbs
            RequestContext.ClientApplication.Add("breadcrumbsRefresh", false);
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            // Ignore action requiring database if database is not available. Message page can be displayed for application without database (error within web install)
            if (ConnectionHelper.ConnectionAvailable)
            {
                SetRTL();
            }

            SetBrowserClass(false);
            AddNoCacheTag();

            // Register modal page scripts
            RegisterModalPageScripts();
        }

        #endregion
    }
}