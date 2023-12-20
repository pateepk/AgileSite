using System;
using System.Data;
using System.Linq;

using CMS.CMSImportExport;
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

            // V12.0 specific
            SpecialActionsEvents.PrepareDataStructure.Execute += PrepareDataStructureOnExecute;
            ImportExportEvents.GetImportData.After += GetImportData_After;
        }

        private static void PrepareDataStructureOnExecute(object sender, ImportGetDataEventArgs e)
        {
            if (e.Settings.IsLowerVersion("12.0") && !e.SelectionOnly)
            {
                switch (e.ObjectType)
                {
                    case OrderInfo.OBJECT_TYPE:
                        // Ensure removed columns for address references
                        e.Data.Tables[0].Columns.Add("OrderBillingAddressID", typeof(int));
                        e.Data.Tables[0].Columns.Add("OrderShippingAddressID", typeof(int));
                        e.Data.Tables[0].Columns.Add("OrderCompanyAddressID", typeof(int));
                        break;
                }
            }
        }


        private static void GetImportData_After(object sender, ImportGetDataEventArgs e)
        {
            if (e.Settings.IsLowerVersion("12.0") && !e.SelectionOnly)
            {
                if (e.ObjectType.Equals(OrderInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
                {
                    // Load data for "ecommerce.orderaddress" object type
                    var addressData = ImportProvider.GetObjectsData(e.Settings, OrderAddressInfo.OBJECT_TYPE, false);

                    // Do nothing if COM_OrderAddress table has no values
                    if (addressData == null)
                    {
                        return;
                    }

                    var orderAddressTable = e.Data.Tables["COM_OrderAddress"];
                    var orderTable = e.Data.Tables["COM_Order"];

                    // Copy addresses related to imported orders to the table for order address import
                    foreach (DataRow row in addressData.Rows)
                    {
                        var addressId = DataHelper.GetIntValue(row, "AddressID");

                        // Select order record with old address reference
                        var order = orderTable.Select(String.Format("OrderBillingAddressID = {0} OR OrderShippingAddressID = {0} OR OrderCompanyAddressID = {0}", addressId)).FirstOrDefault();
                        if (order != null)
                        {
                            var orderId = DataHelper.GetIntValue(order, "OrderID");
                            var billingAddressId = DataHelper.GetIntValue(order, "OrderBillingAddressID");
                            var shippingAddressId = DataHelper.GetIntValue(order, "OrderShippingAddressID");

                            // Set address order ID and its type
                            row["AddressOrderID"] = orderId;
                            row["AddressType"] = (billingAddressId == addressId) ? (int)AddressType.Billing : (shippingAddressId == addressId) ? (int)AddressType.Shipping : (int)AddressType.Company;

                            // Copy address to the import data
                            orderAddressTable.ImportRow(row);
                        }
                    }
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
    }
}