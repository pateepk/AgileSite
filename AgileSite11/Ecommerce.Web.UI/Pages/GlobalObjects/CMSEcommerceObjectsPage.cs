using System;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for E-commerce pages working with site specific records with option to include global records. 
    /// </summary>
    public class CMSEcommerceObjectsPage : CMSEcommercePage
    {
        private int mConfiguredSiteId = -1;


        /// <summary>
        /// Settings key name allowing usage of global objects.
        /// </summary>
        protected string GlobalObjectsKeyName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates global objects are allowed on current site besides site-specific ones. Type of the objects depends on settings key name 
        /// set in GlobalObjectsKeyName property.
        /// </summary>
        public bool AllowGlobalObjects
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
        /// Returns id of the configured site. For admin allows to configure global objects on demand (siteId in querystring == 0).
        /// In other cases returns 0 (when global objects allowed and asked for global objects) or current site ID (when using site specific configuration only).
        /// </summary>
        protected int ConfiguredSiteID
        {
            get
            {
                if (mConfiguredSiteId < 0)
                {
                    // Configure global records when admin explicitly asks for
                    if (IsAdmin && (SiteID <= 0))
                    {
                        mConfiguredSiteId = 0;
                    }
                    else
                    {
                        if (AllowGlobalObjects && (SiteID <= 0))
                        {
                            mConfiguredSiteId = 0;
                        }
                        else
                        {
                            mConfiguredSiteId = SiteContext.CurrentSiteID;
                        }
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
                return AllowGlobalObjects;
            }
        }


        /// <summary>
        /// Checks if site id of edited object corresponds to configured site ID and site settings. If it does not, 
        /// user is redirected to 'Object doesn't exist' page.
        /// </summary>
        /// <param name="editedObjSiteId">ID of the site which edited object belongs to</param>
        protected override void CheckEditedObjectSiteID(int editedObjSiteId)
        {
            if (AllowGlobalObjects && (editedObjSiteId == 0))
            {
                return;
            }

            // An attempt to configure site-specific record which does not belong to configured or current site
            if ((editedObjSiteId != ConfiguredSiteID) && (editedObjSiteId != SiteContext.CurrentSiteID))
            {
                EditedObject = null;
            }
        }


        /// <summary>
        /// Appends site mark to given object name according to current ui culture. 
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="siteId">Site id of named object.</param>
        public static string FormatBreadcrumbObjectName(string name, int siteId)
        {
            // Encode name
            name = HTMLHelper.HTMLEncode(name ?? "");

            // No change for site-specific name
            if (siteId != 0)
            {
                return name;
            }

            // Format according to culture
            string format = CultureHelper.IsUICultureRTL() ? "{1} {0}" : "{0} {1}";

            return string.Format(format, name, GetString("general.global"));
        }

        /// <summary>
        /// Creates where condition for UniGrid according object site separability settings.
        /// </summary>
        /// <param name="siteIDColumnName">Name of siteID column</param>
        /// <returns></returns>
        protected WhereCondition InitSiteWhereCondition(string siteIDColumnName)
        {
            var where = new WhereCondition();

            where.WhereEquals(siteIDColumnName, SiteContext.CurrentSiteID);

            if (AllowGlobalObjects)
            {
                where.Or().WhereNull(siteIDColumnName);
            }

            return where;
        }


        /// <summary>
        /// Checks the price has correct maximum decimal places defined in currency configuration.
        /// </summary>
        /// <param name="currency">Currency object with configuration.</param>
        /// <param name="price">Price to be checked.</param>
        /// <remarks>Returns also true if the currency is null.</remarks>
        protected bool CheckCurrencyRounding(CurrencyInfo currency, decimal price)
        {
            if (currency == null)
            {
                // Decimal places will be validated against system settings for decimal data type
                return true;
            }

            var decimals = currency.CurrencyRoundTo;
            var roundedPrice = Math.Round(price, decimals);

            return price == roundedPrice;
        }


        /// <summary>
        /// Validates product price.
        /// </summary>
        /// <returns>Null or validation error.</returns>
        protected virtual string ValidatePrice(decimal price, CurrencyInfo currency, SKUInfo product)
        {
            if (!CheckCurrencyRounding(currency, price))
            {
                return string.Format(GetString("com.productedit.priceinvalid"), currency.CurrencyCode, currency.CurrencyRoundTo);
            }

            var priceField = FormHelper.GetFormInfo(product.TypeInfo.ObjectClassName, false)
                                       .GetFormField("SKUPrice");

            return new DataTypeIntegrity(price, priceField).ValidateDataType();
        }
    }
}