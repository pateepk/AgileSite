using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce configuration pages.
    /// </summary>
    public class CMSEcommerceConfigurationPage : CMSEcommercePage
    {
        private int mConfiguredSiteId = -1;
        private string mSiteOrGlobalObjectType;
        private string mGlobalObjectsKeyName;


        /// <summary>
        /// Name of ecommerce object type configured on this page.
        /// </summary>
        protected string SiteOrGlobalObjectType
        {
            get
            {
                return mSiteOrGlobalObjectType;
            }
            set
            {
                mSiteOrGlobalObjectType = value;
                mGlobalObjectsKeyName = null;
            }
        }


        /// <summary>
        /// Settings key name configuring usage of global/site objects.
        /// </summary>
        protected string GlobalObjectsKeyName
        {
            get
            {
                if ((mGlobalObjectsKeyName == null) && (SiteOrGlobalObjectType != null))
                {
                    mGlobalObjectsKeyName = ECommerceSettings.GetGlobalKeyForType(SiteOrGlobalObjectType);
                }

                return mGlobalObjectsKeyName;
            }
            set
            {
                mGlobalObjectsKeyName = value;
            }
        }


        /// <summary>
        /// Indicates if current site uses global objects. Type of objects depends on settings key name 
        /// set in GlobalObjectsKeyName property.
        /// </summary>
        protected bool UseGlobalObjects
        {
            get
            {
                if (!string.IsNullOrEmpty(GlobalObjectsKeyName))
                {
                    return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + "." + GlobalObjectsKeyName);
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates if ecommerce object type specified by SiteOrGlobalObjectType property allows using global and site objects on the same site.
        /// </summary>
        protected bool AllowCombineSiteAndGlobal
        {
            get
            {
                if (!string.IsNullOrEmpty(SiteOrGlobalObjectType))
                {
                    return ECommerceSettings.AllowCombineSiteAndGlobal(SiteOrGlobalObjectType);
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates if this configuration page is opened under multistore configuration (i.e. Global Admin requesting global configuration).
        /// </summary>
        protected bool IsMultiStoreConfiguration
        {
            get
            {
                return IsGlobalStoreAdmin && (SiteID <= 0);
            }
        }


        /// <summary>
        /// Returns id of the configured site. For global admin with site manager enabled allows to configure global objects on demand (siteId in querystring == 0).
        /// In other cases returns 0 (when using global objects) or current site ID (when using site specific configuration).
        /// </summary>
        protected int ConfiguredSiteID
        {
            get
            {
                if (mConfiguredSiteId < 0)
                {
                    // Configure global records when global admin explicitly asks for
                    if (IsMultiStoreConfiguration)
                    {
                        mConfiguredSiteId = 0;
                    }
                    else
                    {
                        // Configuring global objects when used exclusively or when mixed and requested
                        var configuringGlobal = UseGlobalObjects && ((SiteID <= 0) || !AllowCombineSiteAndGlobal);

                        mConfiguredSiteId = configuringGlobal ? 0 : SiteContext.CurrentSiteID;
                    }
                }

                return mConfiguredSiteId;
            }
        }


        /// <summary>
        /// Indicates if information that object belongs to specific site or is global will be shown in listings.
        /// </summary>
        protected override bool ShowSiteInGrids
        {
            get
            {
                return UseGlobalObjects && !IsMultiStoreConfiguration;
            }
        }


        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Disable checking for multistore configuration
            if (IsGlobalStoreAdmin)
            {
                RequireSite = (SiteID > 0);
            }

            // Check 'ConfigurationRead' permission
            var user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_READ))
            {
                RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_READ);
            }
        }


        /// <summary>
        /// Checks ecommerce ConfigurationModify and ConfigurationGlobalModify permissions. Redirects to access denied page if check fails.
        /// </summary>
        protected void CheckConfigurationModification()
        {
            CheckConfigurationModification(ConfiguredSiteID);
        }


        /// <summary>
        /// Checks ecommerce ConfigurationModify and ConfigurationGlobalModify permissions. Redirects to access denied page if check fails.
        /// </summary>
        /// <param name="siteId">Site id of the configured object</param>
        protected void CheckConfigurationModification(int siteId)
        {
            // Check 'ConfigurationGlobalModify' permission only if configuring global objects
            if (siteId == 0)
            {
                // Check 'ConfigurationGlobalModify' permission
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFYGLOBAL))
                {
                    RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFYGLOBAL);
                }
            }
            else
            {
                // Check 'ConfigurationModify' permission
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFY))
                {
                    RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFY);
                }
            }
        }


        /// <summary>
        /// Checks if site id of edited object corresponds to configured site. If it does not, 
        /// user is redirected to 'Object doesn't exist' page. Check is skipped for global objects when allowed by setting 
        /// or when edited under multistore configuration.
        /// </summary>
        /// <param name="editedObjSiteId">ID of the site which edited object belongs to.</param>
        protected override void CheckEditedObjectSiteID(int editedObjSiteId)
        {
            if ((editedObjSiteId == 0) && (UseGlobalObjects || IsMultiStoreConfiguration))
            {
                return;
            }

            if (editedObjSiteId != ConfiguredSiteID)
            {
                EditedObject = null;
            }
        }


        /// <summary>
        /// Returns string param according <see cref="IsMultiStoreConfiguration"/> for Unigrid.
        /// Assign the returned value to <see cref="UniGrid.RememberStateByParam"/> to handle different filter states for Store and MultiStore configuration listings.
        /// </summary>
        protected string GetGridRememberStateParam()
        {
            return IsMultiStoreConfiguration ? "SiteID" : string.Empty;
        }


        /// <summary>
        /// Checks and displays info message stating that current site uses global objects of given type if used.
        /// </summary>
        /// <param name="objectType">Object type name to be used in info message.</param>
        protected void HandleGlobalObjectInformation(string objectType)
        {
            var siteId = SiteID;

            // Configuring global records from specific site
            if (!AllowCombineSiteAndGlobal && (ConfiguredSiteID == 0) && (siteId > 0))
            {
                var objectsName = TypeHelper.GetNiceObjectTypeName(objectType);
                objectsName = TypeHelper.GetPlural(objectsName);

                var site = SiteInfoProvider.GetSiteInfo(siteId);
                if (site != null)
                {
                    ShowInformation(string.Format(GetString("com.UsingGlobalSettings"), HTMLHelper.HTMLEncode(site.DisplayName), HTMLHelper.HTMLEncode(objectsName)));
                }
            }
        }


        /// <summary>
        /// Creates where condition for UniGrid according object site separability settings.
        /// </summary>
        /// <param name="siteIDColumnName">Name of siteID column</param>
        /// <returns></returns>
        protected WhereCondition InitSiteWhereCondition(string siteIDColumnName)
        {
            var where = new WhereCondition();

            if (IsMultiStoreConfiguration || UseGlobalObjects)
            {
                // Include global objects
                where.WhereNull(siteIDColumnName);
            }
            if (ConfiguredSiteID > 0)
            {
                // Include site objects
                where.Or().WhereEquals(siteIDColumnName, ConfiguredSiteID);
            }

            return where;
        }
    }
}