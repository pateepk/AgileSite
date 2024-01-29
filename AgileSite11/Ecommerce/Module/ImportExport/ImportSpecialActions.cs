using System;
using System.Data;
using System.Linq;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ProcessObjectData.After += ProcessObjectData_After;
            ImportExportEvents.ImportObjectType.Before += ImportObjectTypeBefore;
            ImportExportEvents.ImportObject.Before += ImportObject_Before;
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObjectOnBefore;
            SpecialActionsEvents.PrepareDataStructure.Execute += PrepareDataStructureOnExecute;
        }


        private static void PrepareDataStructureOnExecute(object sender, ImportGetDataEventArgs e)
        {
            if (e.Settings.IsLowerVersion("11.0"))
            {
                // Ensure removed binding table to load data to restore assigned taxes
                switch (e.ObjectType)
                {
                    case ShippingOptionInfo.OBJECT_TYPE:
                        e.Data.Tables.Add(CreateTaxBindingTable("COM_ShippingOptionTaxClass", "ShippingOptionID"));

                        break;

                    case DepartmentInfo.OBJECT_TYPE:
                        e.Data.Tables.Add(CreateTaxBindingTable("COM_DepartmentTaxClass", "DepartmentID"));

                        break;

                    case SKUInfo.OBJECT_TYPE_SKU:
                    case OptionCategoryInfo.OBJECT_TYPE:
                        e.Data.Tables.Add(CreateTaxBindingTable("COM_SKUTaxClasses", "SKUID"));

                        break;

                    case MultiBuyDiscountInfo.OBJECT_TYPE:
                        var column = e.Data.Tables[0].Columns["MultiBuyDiscountIsProductCoupon"];
                        column.AllowDBNull = false;
                        column.DefaultValue = false;

                        break;
                }
            }
        }


        private static void ProcessMainObjectOnBefore(object sender, ImportEventArgs e)
        {
            var info = e.Object;

            if (e.Settings.IsLowerVersion("11.0"))
            {
                switch (info.TypeInfo.ObjectType)
                {
                    case ShippingOptionInfo.OBJECT_TYPE:

                        // Restore taxID for shipping option
                        SetTaxIDFromBinding(e, "COM_ShippingOptionTaxClass", "ShippingOptionTaxClassID");

                        break;

                    case DepartmentInfo.OBJECT_TYPE:
                        // Restore taxID for department
                        SetTaxIDFromBinding(e, "COM_DepartmentTaxClass", "DepartmentDefaultTaxClassID");
                        
                        break;

                    case SKUInfo.OBJECT_TYPE_SKU:
                    case SKUInfo.OBJECT_TYPE_OPTIONSKU:
                        // Restore taxID for SKUs and SKU options
                        SetTaxIDFromBinding(e, "COM_SKUTaxClasses", "SKUTaxClassID");

                        break;

                    case SettingsKeyInfo.OBJECT_TYPE:
                        // Following setting keys are not available in 11.0
                        if (info.Generalized.ObjectCodeName.Equals("CMSStoreDisplayPriceIncludingDiscounts", StringComparison.OrdinalIgnoreCase)
                            || info.Generalized.ObjectCodeName.Equals("CMSStoreDisplayPriceIncludingTaxes", StringComparison.OrdinalIgnoreCase)
                            || info.Generalized.ObjectCodeName.Equals("CMSStoreAllowGlobalDiscountCoupons", StringComparison.OrdinalIgnoreCase))
                        {
                            e.Cancel();
                        }

                        break;
                }
            }
        }


        private static void ImportObject_Before(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;

            if (e.Settings.IsLowerVersion("11.0"))
            {
                switch (infoObj.TypeInfo.ObjectType)
                {
                    case SKUInfo.OBJECT_TYPE_SKU:
                        if (infoObj.GetStringValue("SKUProductType", null) == "DONATION")
                        {
                            // Import donation as a standard product
                            infoObj.SetValue("SKUProductType", SKUProductTypeEnum.Product.ToStringRepresentation());
                        }

                        break;

                    case DiscountInfo.OBJECT_TYPE:
                        var discount = infoObj as DiscountInfo;
                        if ((discount != null) && discount.IsApplicableToOrders)
                        {
                            // Disable flat discounts and set them as percentage
                            if (discount.DiscountIsFlat)
                            {
                                discount.DiscountEnabled = false;
                                discount.DiscountIsFlat = false;
                                discount.DiscountValue = 0;
                            }
                        }

                        break;

                    case OrderInfo.OBJECT_TYPE:
                        var order = infoObj as OrderInfo;
                        if (order != null)
                        {
                            order.OrderGrandTotal = order.OrderTotalPrice;
                            order.OrderGrandTotalInMainCurrency = order.OrderTotalPriceInMainCurrency;
                        }

                        break;

                    case OrderItemInfo.OBJECT_TYPE:
                        var orderItem = infoObj as OrderItemInfo;
                        if (orderItem != null)
                        {
                            orderItem.OrderItemTotalPrice = orderItem.OrderItemUnitPrice * orderItem.OrderItemUnitCount;
                        }

                        break;
                }
            }
        }


        private static void ProcessObjectData_After(object sender, ImportEventArgs e)
        {
            var parameters = e.Parameters;
            var settings = e.Settings;
            var th = e.TranslationHelper;
            var infoObj = e.Object;
            var objectType = infoObj.TypeInfo.ObjectType;

            using (new ImportSpecialCaseContext(settings))
            {
                if ((objectType == OptionCategoryInfo.OBJECT_TYPE) && (parameters.ObjectProcessType == ProcessObjectEnum.All))
                {
                    // Translate default values list
                    th.TranslateListColumn(infoObj, "CategoryDefaultOptions", SKUInfo.OBJECT_TYPE_SKU, 0, ',');

                    infoObj.Generalized.SetObject();
                }
            }
        }


        private static void ImportObjectTypeBefore(object sender, ImportDataEventArgs e)
        {
            e.Using(new ECommerceActionContext
            {
                SetLowestPriceToParent = false,
            });
        }


        /// <summary>
        /// Restores tax ID from the binding table to the info object property.
        /// </summary>
        private static void SetTaxIDFromBinding(ImportEventArgs e, string bindingName, string taxColumnName)
        {
            var th = e.TranslationHelper;
            var info = e.Object;

            // Get the binding table 
            var taxTable = e.Parameters.Data.Tables[bindingName];

            // Get the original info ID
            var infoID = th.TranslationTable.AsEnumerable()
                .Where(r => r.Field<string>(TranslationHelper.RECORD_CODE_NAME_COLUMN) == info.Generalized.ObjectCodeName)
                .Select(r => r.Field<int>(TranslationHelper.RECORD_ID_COLUMN))
                .FirstOrDefault();

            // Search for some tax in the binding table
            var taxID = taxTable?.AsEnumerable()
                .Where(r => r.Field<int>(info.TypeInfo.IDColumn) == infoID)
                .Select(r => r.Field<int>("TaxClassID"))
                .DefaultIfEmpty()
                .Min();

            if (taxID > 0)
            {
                // Determine the new taxID 
                const string unknown = ObjectTypeInfo.COLUMN_NAME_UNKNOWN;
                var newTaxID = th.GetNewID(TaxClassInfo.OBJECT_TYPE, taxID.Value, unknown, 0, unknown, unknown, unknown);

                if (newTaxID > 0)
                {
                    // Set into the tax property
                    info.SetValue(taxColumnName, newTaxID);
                }
            }
        }


        private static DataTable CreateTaxBindingTable(string name, string fkColumn)
        {
            var dt = new DataTable(name);
            DataHelper.EnsureColumn(dt, fkColumn, typeof(int));
            DataHelper.EnsureColumn(dt, "TaxClassID", typeof(int));

            return dt;
        }
    }
}