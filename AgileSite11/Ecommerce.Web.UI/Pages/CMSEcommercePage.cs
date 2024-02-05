using System;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.DataEngine;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce pages to apply global settings to the pages.
    /// </summary>
    public class CMSEcommercePage : CMSDeskPage
    {
        #region "Variables"

        private bool? mIsGlobalStoreAdmin;
        private bool? mIsAdmin;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if current user is allowed to access global (multistore) configuration.
        /// </summary>
        protected bool IsGlobalStoreAdmin
        {
            get
            {
                if (!mIsGlobalStoreAdmin.HasValue)
                {
                    mIsGlobalStoreAdmin = MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin);
                }

                return mIsGlobalStoreAdmin.Value;
            }
        }


        /// <summary>
        /// Indicates if current user has 'Admin' privilege level.
        /// </summary>
        protected bool IsAdmin
        {
            get
            {
                if (!mIsAdmin.HasValue)
                {
                    mIsAdmin = MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
                }

                return mIsAdmin.Value;
            }
        }


        /// <summary>
        /// Id of the site. Value taken from query string parameter "siteId", or from SiteContext.CurrentSiteID when no SiteId 
        /// supplied or not an admin. Special values (non-positive) are passed without change.
        /// </summary>
        public int SiteID
        {
            get
            {
                int id = QueryHelper.GetInteger("siteId", SiteContext.CurrentSiteID);

                // Allow only current site or special value for non-administrator
                if (!IsAdmin && (id > 0))
                {
                    id = SiteContext.CurrentSiteID;
                }

                return id;
            }
        }


        /// <summary>
        /// Identifies if the page is used for products UI
        /// </summary>
        protected override bool IsProductsUI
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Indicates if information that object belongs to specific site or is global will be shown in listings.
        /// </summary>
        protected virtual bool ShowSiteInGrids
        {
            get
            {
                return true;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Page PreInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnPreInit(EventArgs e)
        {
            // Turn off check document permissions
            CheckDocPermissions = false;
            base.OnPreInit(e);
        }

        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if ((SiteID > 0) && !ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.ECOMMERCE, SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite(ModuleName.ECOMMERCE);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks if the site specified by the site ID has a main currency defined.
        /// Returns true if the main currency is defined, otherwise returns false and shows warning on the page.
        /// </summary>
        /// <param name="siteId">ID of site to check</param>
        protected bool CheckMainCurrency(int siteId)
        {
            string currencyWarning = EcommerceUIHelper.CheckMainCurrency(siteId);
            if (!string.IsNullOrEmpty(currencyWarning))
            {
                ShowWarning(currencyWarning);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks permissions to modify given product and redirect to access denied page when not passed.
        /// Returns true if current user is authorized to modify given product;
        /// </summary>
        /// <param name="product">Product to be checked.</param>
        protected bool CheckProductModifyAndRedirect(SKUInfo product)
        {
            // Check module permissions
            if (!ECommerceContext.IsUserAuthorizedToModifySKU(product))
            {
                if (product.IsGlobal)
                {
                    RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.ECOMMERCE_MODIFYGLOBAL);
                }
                else
                {
                    RedirectToAccessDenied(ModuleName.ECOMMERCE, "EcommerceModify OR ModifyProducts");
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks whether supplied siteId corresponds to current site ID and sets EditedObject to null if not.
        /// Check applies to non-admin users only.
        /// </summary>
        /// <param name="objectSiteId">SiteID of the object to be checked.</param>
        protected virtual void CheckEditedObjectSiteID(int objectSiteId)
        {
            // When not an admin, check if object belongs to current site
            if (!IsAdmin && (objectSiteId != SiteContext.CurrentSiteID))
            {
                EditedObject = null;
            }
        }


        /// <summary>
        /// Shows/hides column with SiteID in given unigrid according allow global XXX setting accessible via AllowGlobalObjects property.
        /// </summary>
        /// <param name="grid">Grid to hide/show site id column in.</param>
        protected void HandleGridsSiteIDColumn(UniGrid grid)
        {
            var obj = grid.InfoObject;
            if ((obj == null) || (grid.Page == null) || (obj.TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                return;
            }

            var siteColumn = obj.TypeInfo.SiteIDColumn;

            grid.Page.PreRender += (sender, args) =>
            {
                if (grid.NamedColumns.ContainsKey(siteColumn))
                {
                    grid.NamedColumns[siteColumn].Visible = ShowSiteInGrids;
                }
            };
        }

        #endregion
    }
}