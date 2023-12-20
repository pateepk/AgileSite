using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Summary description for Functions.
    /// </summary>
    public class EcommerceTransformationFunctions
    {
        #region "Links"

        /// <summary>
        /// Returns link to "add to shopping cart".
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="enabled">Indicates whether product is enabled or not</param>
        public static string GetAddToShoppingCartLink(object productId, object enabled)
        {
            return GetAddToShoppingCartLink(productId, enabled, null);
        }


        /// <summary>
        /// Returns link to "add to shopping cart".
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="enabled">Indicates whether product is enabled or not</param>
        /// <param name="imageUrl">Image URL</param>
        public static string GetAddToShoppingCartLink(object productId, object enabled, string imageUrl)
        {
            if (ValidationHelper.GetBoolean(enabled, false) && (ValidationHelper.GetInteger(productId, 0) != 0))
            {
                // Get default image URL
                imageUrl = imageUrl ?? "CMSModules/CMS_Ecommerce/addorder.png";
                return "<img src=\"" + UIHelper.GetImageUrl(null, imageUrl) + "\" alt=\"Add to cart\" />" +
                    "<a href=\"" + ShoppingCartURL(SiteContext.CurrentSiteName) + "?productId=" + Convert.ToString(productId) + "&amp;quantity=1\">" + 
                        ResHelper.GetString("EcommerceTransformationFunctions.AddToShoppingCart") + 
                    "</a>";
            }

            return "";
        }


        /// <summary>
        /// Returns link to "add to shopping cart".
        /// </summary>
        /// <param name="productId">Product ID</param>
        public static string GetAddToShoppingCartLink(object productId)
        {
            return GetAddToShoppingCartLink(productId, true);
        }


        /// <summary>
        /// Returns link to add specified product to the user's wish list.
        /// </summary>
        /// <param name="productId">Product ID</param>
        public static string GetAddToWishListLink(object productId)
        {
            return GetAddToWishListLink(productId, null);
        }


        /// <summary>
        /// Returns link to add specified product to the user's wish list.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="imageUrl">Image URL</param>
        public static string GetAddToWishListLink(object productId, string imageUrl)
        {
            if (ValidationHelper.GetInteger(productId, 0) != 0)
            {
                // Get default image URL
                imageUrl = imageUrl ?? "CMSModules/CMS_Ecommerce/addtowishlist.png";
                return "<img src=\"" + UIHelper.GetImageUrl(null, imageUrl) + "\" alt=\"Add to wishlist\" />" + 
                    "<a href=\"" + WishlistURL(SiteContext.CurrentSiteName) + "?productId=" + Convert.ToString(productId) + "\">" + 
                        ResHelper.GetString("EcommerceTransformationFunctions.AddToWishlist") + 
                    "</a>";
            }

            return "";
        }


        /// <summary>
        /// Returns link to remove specified product from the user's wish list.
        /// </summary>
        /// <param name="productId">Product ID</param>
        public static string GetRemoveFromWishListLink(object productId)
        {
            if ((productId != DBNull.Value) && (!MembershipContext.AuthenticatedUser.IsPublic()))
            {
                return "<a href=\"javascript:onclick=RemoveFromWishlist(" + Convert.ToString(productId) + ")\" class=\"RemoveFromWishlist\">" + ResHelper.GetString("Wishlist.RemoveFromWishlist") + "</a>";
            }

            return "";
        }

        #endregion


        #region "URLs"

        /// <summary>
        /// Returns URL to the shopping cart.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string ShoppingCartURL(string siteName)
        {
            return HTMLHelper.EncodeForHtmlAttribute(UrlResolver.ResolveUrl(ECommerceSettings.ShoppingCartURL(siteName)));
        }


        /// <summary>
        /// Returns URL to the wish list.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string WishlistURL(string siteName)
        {
            return HTMLHelper.EncodeForHtmlAttribute(UrlResolver.ResolveUrl(ECommerceSettings.WishListURL(siteName)));
        }


        /// <summary>
        /// Returns product URL.
        /// </summary>
        /// <param name="SKUID">SKU ID</param>
        public static string GetProductUrl(object SKUID)
        {
            return UrlResolver.ResolveUrl("~/CMSPages/Ecommerce/GetProduct.aspx?productId=" + Convert.ToString(SKUID));
        }


        /// <summary>
        /// Returns user friendly URL of the specified SKU and site name. Does not uses hash tables to create URL.
        /// </summary>
        /// <param name="skuGuid">SKU Guid</param>
        /// <param name="skuName">SKU Name</param>
        /// <param name="siteNameObj">Site Name</param>
        public static string GetProductUrl(object skuGuid, object skuName, object siteNameObj = null)
        {
            Guid guid = ValidationHelper.GetGuid(skuGuid, Guid.Empty);
            string name = Convert.ToString(skuName);
            string siteName = ValidationHelper.GetString(siteNameObj, null);

            return UrlResolver.ResolveUrl(SKUInfoProvider.GetSKUUrl(guid, name, siteName));
        }


        /// <summary>
        /// Returns user friendly URL of the specified SKU and site name. Uses hash tables to create URL.
        /// </summary>
        /// <param name="skuID">SKU ID</param>
        /// <param name="skuName">SKU Name</param>
        /// <param name="siteNameObj">Site Name</param>
        public static string GetProductUrlByID(object skuID, object skuName, object siteNameObj = null)
        {
            int id = ValidationHelper.GetInteger(skuID, 0);
            string name = Convert.ToString(skuName);
            string siteName = ValidationHelper.GetString(siteNameObj, null);

            return UrlResolver.ResolveUrl(SKUInfoProvider.GetSKUUrl(id, name, siteName));
        }


        /// <summary>
        /// Returns e-product download URL.
        /// </summary>
        /// <param name="fileToken">File unique download URL</param>
        /// <param name="fileName">File name</param>
        /// <param name="siteId">Site ID of the order in which the e-product is included</param>
        public static string GetEproductUrl(object fileToken, object fileName, object siteId)
        {
            Guid fToken = ValidationHelper.GetGuid(fileToken, Guid.Empty);
            string fName = ValidationHelper.GetString(fileName, String.Empty);
            int sId = ValidationHelper.GetInteger(siteId, 0);

            return OrderItemSKUFileInfoProvider.GetOrderItemSKUFileUrl(fToken, fName, sId);
        }


        /// <summary>
        /// Returns URL of the specified product with feed parameter.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="skuGUID">SKU GUID</param>
        /// <param name="skuName">SKU name</param>
        /// <param name="siteName">Site name</param>
        public static string GetProductUrlForFeed(string feedName, object skuGUID, object skuName, object siteName)
        {
            string url = GetProductUrl(skuGUID, skuName, siteName);
            if (!String.IsNullOrEmpty(feedName))
            {
                url = URLHelper.AddParameterToUrl(url, "feed", feedName);
            }
            return url;
        }


        /// <summary>
        /// Returns SKU image URL including dimension's modifiers (width, height or maxsidesize) and site name parameter if product is from different site than current. If image URL is not specified, SKU default image URL is used.
        /// </summary>
        /// <param name="imageUrl">SKU image URL</param>
        /// <param name="width">Image requested width, has no effect if maxsidesize is specified</param>
        /// <param name="height">Image requested height, has no effect if maxsidesize is specified</param>
        /// <param name="maxsidesize">Image requested maximum side size</param>
        /// <param name="siteId">SKU site ID. If empty, current site ID is used.</param>    
        public static string GetSKUImageUrl(object imageUrl, object width, object height, object maxsidesize, object siteId)
        {
            int iSiteId = ValidationHelper.GetInteger(siteId, 0);
            bool notCurrentSite = ((iSiteId > 0) && (iSiteId != SiteContext.CurrentSiteID));

            // Get site name        
            var siteName = notCurrentSite ? SiteInfoProvider.GetSiteName(iSiteId) : SiteContext.CurrentSiteName;

            // Get product image URL  
            string url = ValidationHelper.GetString(imageUrl, null);

            if (String.IsNullOrEmpty(url))
            {
                // Get default product image URL                      
                url = ECommerceSettings.DefaultProductImageURL(siteName);
            }

            if (!String.IsNullOrEmpty(url))
            {
                // Resolve URL
                url = UrlResolver.ResolveUrl(url);

                int slashIndex = url.LastIndexOf("/", StringComparison.Ordinal);
                if (slashIndex >= 0)
                {
                    string urlStartPart = url.Substring(0, slashIndex);
                    string urlEndPart = url.Substring(slashIndex);

                    url = urlStartPart + HttpUtility.UrlPathEncode(urlEndPart);

                    // Add site name if not current
                    if (notCurrentSite)
                    {
                        url = URLHelper.AddParameterToUrl(url, "siteName", siteName);
                    }

                    // Add max side size
                    int iMaxSideSize = ValidationHelper.GetInteger(maxsidesize, 0);
                    if (iMaxSideSize > 0)
                    {
                        url = URLHelper.AddParameterToUrl(url, "maxsidesize", iMaxSideSize.ToString());
                    }
                    else
                    {
                        // Add width
                        int iWidth = ValidationHelper.GetInteger(width, 0);
                        if (iWidth > 0)
                        {
                            url = URLHelper.AddParameterToUrl(url, "width", iWidth.ToString());
                        }

                        // Add height
                        int iHeight = ValidationHelper.GetInteger(height, 0);
                        if (iHeight > 0)
                        {
                            url = URLHelper.AddParameterToUrl(url, "height", iHeight.ToString());
                        }
                    }

                    // Encode URL
                    url = HTMLHelper.HTMLEncode(url);
                }
            }

            return url;
        }

        #endregion


        #region "SKU related objects' properties"

        /// <summary>
        /// Gets object from the specified column of the manufacturer with specific ID.
        /// </summary>
        /// <param name="Id">Manufacturer ID</param>
        /// <param name="column">Column name</param>
        public static object GetManufacturer(object Id, string column)
        {
            int id = ValidationHelper.GetInteger(Id, 0);
            if ((id > 0) && !String.IsNullOrEmpty(column))
            {
                // Get manufacturer
                ManufacturerInfo mi = ManufacturerInfoProvider.GetManufacturerInfo(id);

                return mi?.GetValue(column);
            }

            return "";
        }


        /// <summary>
        /// Gets object from the specified column of the department with specific ID.
        /// </summary>
        /// <param name="Id">Department ID</param>
        /// <param name="column">Column name</param>
        public static object GetDepartment(object Id, string column)
        {
            int id = ValidationHelper.GetInteger(Id, 0);

            if (id > 0 && !String.IsNullOrEmpty(column))
            {
                // Get department
                DepartmentInfo di = DepartmentInfoProvider.GetDepartmentInfo(id);

                return di?.GetValue(column);
            }

            return "";
        }


        /// <summary>
        /// Gets object from the specified column of the supplier with specific ID.
        /// </summary>
        /// <param name="Id">Supplier ID</param>
        /// <param name="column">Column name</param>
        public static object GetSupplier(object Id, string column)
        {
            int id = ValidationHelper.GetInteger(Id, 0);
            if ((id > 0) && !String.IsNullOrEmpty(column))
            {
                // Get supplier
                SupplierInfo si = SupplierInfoProvider.GetSupplierInfo(id);

                return si?.GetValue(column);
            }

            return "";
        }


        /// <summary>
        /// Gets object from the specified column of the internal status with specific ID.
        /// </summary>
        /// <param name="Id">Internal status ID</param>
        /// <param name="column">Column name</param>
        public static object GetInternalStatus(object Id, string column)
        {
            int id = ValidationHelper.GetInteger(Id, 0);

            if ((id > 0) && !String.IsNullOrEmpty(column))
            {
                // Get internal status
                InternalStatusInfo status = InternalStatusInfoProvider.GetInternalStatusInfo(id);

                return status?.GetValue(column);
            }

            return "";
        }


        /// <summary>
        /// Returns formatted weight, based on mass unit format configuration.
        /// </summary>
        /// <param name="weight">"Weight to be formatted."</param>
        public static object GetFormattedWeight(double weight)
        {
            return string.Format(ECommerceSettings.WeightFormattingString(new SiteInfoIdentifier(SiteContext.CurrentSiteName)), weight);
        }


        /// <summary>
        /// Gets object from the specified column of the public status with specific ID.
        /// </summary>
        /// <param name="Id">Public status ID</param>
        /// <param name="column">Column name</param>
        public static object GetPublicStatus(object Id, string column)
        {
            int id = ValidationHelper.GetInteger(Id, 0);

            if ((id > 0) && !String.IsNullOrEmpty(column))
            {
                // Get public status
                PublicStatusInfo status = PublicStatusInfoProvider.GetPublicStatusInfo(id);

                return status?.GetValue(column);
            }

            return "";
        }


        /// <summary>
        /// Gets document name of specified node id.
        /// </summary>
        /// <param name="nodeIdObj">ID of the node to get name for.</param>
        public static string GetDocumentName(object nodeIdObj)
        {
            int nodeId = ValidationHelper.GetInteger(nodeIdObj, 0);
            if (nodeId != 0)
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                TreeNode node = tree.SelectSingleNode(nodeId, LocalizationContext.PreferredCultureCode);
                if (node != null)
                {
                    return node.GetDocumentName();
                }
            }
            return String.Empty;
        }

        #endregion


        #region "SKU properties"

        /// <summary>
        /// Returns value of the specified product public status column.
        /// If the product is evaluated as a new product in the store, public status set by 'CMSStoreNewProductStatus' setting is used, otherwise product public status is used.
        /// </summary>
        /// <param name="sku">SKU data</param>
        /// <param name="column">Name of the product public status column the value should be retrieved from</param>
        public static object GetSKUIndicatorProperty(SKUInfo sku, string column)
        {
            // Do not process
            if (sku == null)
            {
                return null;
            }

            PublicStatusInfo status = null;
            string siteName = SiteInfoProvider.GetSiteName(sku.SKUSiteID);
            string statusName = ECommerceSettings.NewProductStatus(siteName);

            if (!string.IsNullOrEmpty(statusName) && SKUInfoProvider.IsSKUNew(sku))
            {
                // Get 'new product' status            
                status = PublicStatusInfoProvider.GetPublicStatusInfo(statusName, siteName);
            }
            else
            {
                // Get product public status
                if (sku.SKUPublicStatusID > 0)
                {
                    status = PublicStatusInfoProvider.GetPublicStatusInfo(sku.SKUPublicStatusID);
                }
            }

            // Get specified column value
            return status?.GetValue(column);
        }


        /// <summary>
        /// Returns an SKU catalog price based on the <paramref name="sku"/> data and the data of the current shopping cart. 
        /// Taxes are included based on the site settings.
        /// </summary>
        /// <param name="sku">An SKU object</param>
        public static decimal GetSKUPrice(SKUInfo sku)
        {
            return CalculatePrices(sku)?.Price ?? 0m;
        }


        /// <summary>
        /// Returns an SKU list price based on the SKU data and the data of the current shopping cart.
        /// </summary>
        /// <param name="sku">An SKU object</param>
        public static decimal GetSKUListPrice(SKUInfo sku)
        {
            var cart = ECommerceContext.CurrentShoppingCart;
            if (cart == null)
            {
                return 0m;
            }

            return Service.Resolve<ISKUPriceSourceFactory>()
                .GetSKUListPriceSource(cart.ShoppingCartSiteID)
                .GetPrice(sku, cart.Currency);
        }


        /// <summary>
        /// Returns amount of saved money based on the difference between product seller price and product retail price or price before discount.
        /// </summary> 
        /// <param name="sku">SKU to calculate saving for</param>
        /// <param name="percentage">True - result is percentage, False - result is in the current currency</param>
        public static decimal GetSKUPriceSaving(SKUInfo sku, bool percentage)
        {
            var prices = CalculatePrices(sku);
            if (prices == null)
            {
                return 0m;
            }

            // Actual product price
            var productPrice = prices.Price;

            // Get product price before catalog discount is applied
            var beforeDiscounts = prices.StandardPrice;

            // Retail price
            var retailPrice = GetSKUListPrice(sku);

            // Get higher saving
            if (beforeDiscounts > retailPrice)
            {
                retailPrice = beforeDiscounts;
            }

            // Saved amount
            var savedAmount = retailPrice - productPrice;

            // When seller price is greater than retail price
            if (((productPrice > 0) && (savedAmount < 0)) || ((productPrice < 0) && (savedAmount > 0)))
            {
                // Zero saved amount
                savedAmount = 0;
            }
            else if (percentage)
            {
                // Percentage saved amount
                savedAmount = ((retailPrice == 0) ? 0 : Math.Round(100m * savedAmount / retailPrice));
            }

            return savedAmount;
        }


        /// <summary>
        /// Returns SKURetailPrice or price before discounts if saving is bigger.
        /// Returns zero if price saving is zero.
        /// </summary>
        /// <param name="sku">Product to calculate original price for.</param>
        public static decimal GetSKUOriginalPrice(SKUInfo sku)
        {
            var prices = CalculatePrices(sku);
            if (prices == null)
            {
                return 0m;
            }

            var listPrice = GetSKUListPrice(sku);

            if (listPrice > prices.StandardPrice)
            {
                return listPrice;
            }

            return (prices.StandardPrice > prices.Price) ? prices.StandardPrice : 0;
        }


        /// <summary>
        /// Returns tax for given product.
        /// </summary>
        public static decimal GetSKUTax(SKUInfo product)
        {
            return CalculatePrices(product)?.Tax ?? 0m;
        }


        /// <summary>
        /// Gets the SKU node alias. If there are multiple nodes for this SKU the first occurrence is returned.
        /// If there is not a single one node for this SKU, empty string is returned.
        /// </summary>
        /// <param name="sku">The sku.</param>        
        public static string GetSKUNodeAlias(SKUInfo sku)
        {
            return GetSKUNode(sku).NodeAlias;
        }


        /// <summary>
        /// Gets the SKU node. If there are multiple nodes for this SKU the first occurrence is returned.
        /// </summary>
        /// <param name="sku">The SKU.</param>
        /// <param name="orderBy">OrderBy columns. Default is NodeAlias column.</param>
        /// <returns></returns>
        public static TreeNode GetSKUNode(SKUInfo sku, string orderBy = "NodeAlias")
        {
            // Use ParentSKUID when SKU is a variant
            int skuid = sku.SKUParentSKUID == 0 ? sku.SKUID : sku.SKUParentSKUID;

            var whereCondition = $"NodeSKUID = {skuid}";

            // Ignore linked nodes
            whereCondition = SqlHelper.AddWhereCondition(whereCondition, "NodeLinkedNodeID is null");

            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            var nodes = tree.SelectNodes(SiteContext.CurrentSiteName, string.Empty, LocalizationContext.PreferredCultureCode, false, null, whereCondition, orderBy, -1, true, 1);
            return nodes.FirstOrDefault();
        }


        private static ProductCatalogPrices CalculatePrices(SKUInfo product)
        {
            var cart = ECommerceContext.CurrentShoppingCart;
            if (product == null || cart == null)
            {
                return null;
            }

            return Service.Resolve<ICatalogPriceCalculatorFactory>()
                .GetCalculator(cart.ShoppingCartSiteID)
                .GetPrices(product, null, cart);
        }

        #endregion


        #region "UI methods"

        /// <summary>
        /// Sets different css styles to enabled and disabled dropdownlist items.
        /// </summary>
        /// <param name="drpTemp">Dropdownlist control</param>
        /// <param name="valueFieldName">Field name with ID value</param>
        /// <param name="statusFieldName">Field name with status value</param>
        /// <param name="itemEnabledStyle">Enabled item style</param>
        /// <param name="itemDisabledStyle">Disabled item style</param>
        public static void MarkEnabledAndDisabledItems(DropDownList drpTemp, string valueFieldName, string statusFieldName, string itemEnabledStyle = null, string itemDisabledStyle = null)
        {
            var table = DataHelper.GetDataTable(drpTemp.DataSource);
            if (table == null)
            {
                return;
            }

            itemEnabledStyle = String.IsNullOrEmpty(itemEnabledStyle) ? "DropDownItemEnabled" : itemEnabledStyle;
            itemDisabledStyle = String.IsNullOrEmpty(itemDisabledStyle) ? "DropDownItemDisabled" : itemDisabledStyle;

            foreach (DataRow row in table.Rows)
            {
                ListItem li = drpTemp.Items.FindByValue(Convert.ToString(row[valueFieldName]));
                if ((li != null) && (li.Value != "0"))
                {
                    var itemClass = ValidationHelper.GetBoolean(row[statusFieldName], false) ? itemEnabledStyle : itemDisabledStyle;

                    li.Attributes.Add("class", itemClass);
                }
            }
        }


        /// <summary>
        /// Returns button usable for removing shopping cart coupon code.
        /// </summary>
        /// <param name="couponCode">Discount coupon code value</param>
        /// <param name="cssClass">CSS class; "btn btn-default" class is used by default</param>
        /// <param name="buttonText">Text of the button; resource string with key "general.remove" is used by default</param>
        public static string GetDiscountCouponCodeRemoveButton(string couponCode, string cssClass = null, string buttonText = null)
        {
            var text = buttonText ?? ResHelper.GetString("general.remove");
            var css = cssClass ?? "btn btn-default";

            return $"<input type=\"submit\" value=\"{text}\" class=\"{css}\" id=\"btn_{couponCode}\" onclick=\"{{ __doPostBack('', 'couponCode:{couponCode}'); }} return false;\" />";
        }

        #endregion


        #region "Google Tag Manager"

        /// <summary>
        /// Generates Google Tag Manager product object in JSON format for given <paramref name="sku"/>.
        /// </summary>
        /// <param name="sku"><see cref="SKUInfo"/> for which  Google Tag Manager product object JSON is to be generated.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with <paramref name="sku"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <seealso cref="GtmProductHelper.MapSKU(SKUInfo, object, string)"/>
        /// <seealso cref="GtmDataHelper.SerializeToJson(GtmData, string)"/>
        public static string GetGtmProductJson(SKUInfo sku, object additionalData = null, string purpose = null)
        {
            var gtmData = GtmProductHelper.MapSKU(sku, additionalData, purpose);
            return GtmDataHelper.SerializeToJson(gtmData, purpose);
        }


        /// <summary>
        /// Generates Google Tag Manager product object in JSON format for <see cref="SKUInfo"/> identified by <paramref name="skuId"/>.
        /// </summary>
        /// <param name="skuId">ID of <see cref="SKUInfo"/> for which  Google Tag Manager product object JSON is to be generated.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with properties of <see cref="SKUInfo"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <seealso cref="GtmProductHelper.MapSKU(SKUInfo, object, string)"/>
        /// <seealso cref="GtmDataHelper.SerializeToJson(GtmData, string)"/>
        public static string GetGtmProductJson(int skuId, object additionalData = null, string purpose = null)
        {
            var sku = SKUInfoProvider.GetSKUInfo(skuId);
            return GetGtmProductJson(sku, additionalData, purpose);
        }

        #endregion
    }
}