using System;
using System.Text;

using CMS.DataEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Provides functionality specific to BannerManagement pages in Tools where Banner Category is edited, so permission for specific site.
    /// </summary>
    public abstract class CMSBannerManagementEditPage : CMSBannerManagementPage
    {
        #region "Properties"

        /// <summary>
        /// Edited object site id.
        /// </summary>
        protected virtual int? EditedSiteID
        {
            get
            {
                int siteID = ((BaseInfo)EditedObject).Generalized.ObjectSiteID;

                return siteID > 0 ? (int?)siteID : null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks read permissions.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckReadPermission(EditedSiteID);
        }

        #endregion
    }
}
