using System;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Handles special actions during the export process.
    /// </summary>
    internal static class ExportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetSelectionWhereCondition.Execute += ExportSelection_Execute;
        }


        private static void ExportSelection_Execute(object sender, ExportSelectionArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;

            // Don't preselect object which are not used/allowed by site
            string appendGlobalObjectsKeyName = null;
            switch (objectType.ToLowerCSafe())
            {
                case SKUInfo.OBJECT_TYPE_SKU:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalProducts";
                    break;

                case OptionCategoryInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalProductOptions";
                    break;

                case ManufacturerInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalManufacturers";
                    break;

                case SupplierInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalSuppliers";
                    break;

                case DepartmentInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalDepartments";
                    break;

                case ShippingOptionInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalShippingOptions";
                    break;

                case PaymentOptionInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreAllowGlobalPaymentMethods";
                    break;

                case TaxClassInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreUseGlobalTaxClasses";
                    break;

                case ExchangeTableInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreUseGlobalExchangeRates";
                    break;

                case OrderStatusInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreUseGlobalOrderStatus";
                    break;

                case PublicStatusInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreUseGlobalPublicStatus";
                    break;

                case InternalStatusInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSStoreUseGlobalInternalStatus";
                    break;
            }

            if (!string.IsNullOrEmpty(appendGlobalObjectsKeyName))
            {
                e.Select &= SettingsKeyInfoProvider.GetBoolValue(settings.SiteName + "." + appendGlobalObjectsKeyName);
            }

            if (e.Select)
            {
                // Add additional where condition for special cases
                switch (objectType.ToLowerCSafe())
                {
                    // Include customers assigned to exported site directly and customer having an order on exported site
                    case CustomerInfo.OBJECT_TYPE:
                        {
                            string customerWhere = String.Format("CustomerID IN (SELECT CustomerID FROM COM_Customer WHERE CustomerSiteID = {0} UNION SELECT OrderCustomerID FROM COM_Order WHERE OrderSiteID = {0})", settings.SiteId);
                            e.Where.Or().Where(customerWhere);
                        }
                        break;
                }

                // Add additional where condition due to depending objects
                switch (objectType.ToLowerCSafe())
                {
                    // Special cases
                    case CurrencyInfo.OBJECT_TYPE:
                    case CustomerInfo.OBJECT_TYPE:
                    case DepartmentInfo.OBJECT_TYPE:
                    case InternalStatusInfo.OBJECT_TYPE:
                    case ManufacturerInfo.OBJECT_TYPE:
                    case OrderStatusInfo.OBJECT_TYPE:
                    case PaymentOptionInfo.OBJECT_TYPE:
                    case PublicStatusInfo.OBJECT_TYPE:
                    case ShippingOptionInfo.OBJECT_TYPE:
                    case SupplierInfo.OBJECT_TYPE:
                    case TaxClassInfo.OBJECT_TYPE:
                        e.IncludeDependingObjects = false;
                        break;
                }
            }
        }

        #endregion
    }
}