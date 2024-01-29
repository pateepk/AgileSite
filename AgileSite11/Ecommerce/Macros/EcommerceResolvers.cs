using System.Data;
using System.Linq;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class EcommerceResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mSKUResolver;
        private static MacroResolver mCartResolver;
        private static MacroResolver mEcommerceResolver;
        private static MacroResolver mEcommerceEproductExpirationResolver;

        #endregion

        /// <summary>
        /// Returns SKU resolver.
        /// </summary>
        public static MacroResolver SKUResolver
        {
            get
            {
                if (mSKUResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();
                    resolver.SetNamedSourceData("SKU", ModuleManager.GetReadOnlyObject(PredefinedObjectType.SKU));
                    mSKUResolver = resolver;
                }

                return mSKUResolver;
            }
        }


        /// <summary>
        /// Ecommerce calculation resolver.
        /// </summary>
        public static MacroResolver CalculationResolver
        {
            get
            {
                if (mCartResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();
                    CalculatorData data = new CalculatorData(new CalculationRequest(), new CalculationResult());

                    // Calculation data
                    resolver.SetNamedSourceData("Data", data);
                    resolver.SetNamedSourceData(nameof(data.Request.BillingAddress), ModuleManager.GetReadOnlyObject(OrderAddressInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData(nameof(data.Request.ShippingAddress), ModuleManager.GetReadOnlyObject(OrderAddressInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData(nameof(data.Request.PaymentOption), ModuleManager.GetReadOnlyObject(PaymentOptionInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData(nameof(data.Request.ShippingOption), ModuleManager.GetReadOnlyObject(ShippingOptionInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData(nameof(data.Request.Currency), ModuleManager.GetReadOnlyObject(CurrencyInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData(nameof(data.Request.Customer), ModuleManager.GetReadOnlyObject(CustomerInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData(nameof(data.Request.CalculationDate), data.Request.CalculationDate);
                    resolver.SetNamedSourceData(nameof(data.Request.TotalItemsWeight), data.Request.TotalItemsWeight);
                    resolver.SetNamedSourceData(nameof(data.Result.GrandTotal), data.Result.GrandTotal);
                    resolver.SetNamedSourceData(nameof(data.Result.OrderDiscount), data.Result.OrderDiscount);
                    resolver.SetNamedSourceData(nameof(data.Result.Shipping), data.Result.Shipping);
                    resolver.SetNamedSourceData(nameof(data.Result.Subtotal), data.Result.Subtotal);
                    resolver.SetNamedSourceData(nameof(data.Result.Tax), data.Result.Tax);
                    resolver.SetNamedSourceData(nameof(data.Result.Total), data.Result.Total);
                    resolver.SetNamedSourceData(nameof(data.Result.OrderDiscountSummary), data.Result.OrderDiscountSummary);
                    resolver.SetNamedSourceData(nameof(data.Result.TaxSummary), data.Result.TaxSummary);

                    mCartResolver = resolver;
                }

                return mCartResolver;
            }
        }


        /// <summary>
        /// E-commerce e-mail template macro resolver.
        /// </summary>
        public static MacroResolver EcommerceResolver
        {
            get
            {
                if (mEcommerceResolver == null)
                {
                    // Copy all the data from ShoppingCart resolver
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Data
                    resolver.SetNamedSourceData("ShoppingCart", new ShoppingCartInfo());
                    resolver.SetNamedSourceData("Order", ModuleManager.GetReadOnlyObject(OrderInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("OrderStatus", ModuleManager.GetReadOnlyObject(OrderStatusInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("BillingAddress", ModuleManager.GetReadOnlyObject(OrderAddressInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("ShippingAddress", ModuleManager.GetReadOnlyObject(OrderAddressInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("ShippingOption", ModuleManager.GetReadOnlyObject(ShippingOptionInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("CompanyAddress", ModuleManager.GetReadOnlyObject(OrderAddressInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Customer", ModuleManager.GetReadOnlyObject(CustomerInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("PaymentOption", ModuleManager.GetReadOnlyObject(PaymentOptionInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Currency", ModuleManager.GetReadOnlyObject(CurrencyInfo.OBJECT_TYPE));

                    // Content
                    resolver.SetNamedSourceData("ContentTable", Enumerable.Empty<ShoppingCartLine>());
                    resolver.SetNamedSourceData("EproductsTable", (new DataTable()).Rows);

                    // Totals
                    resolver.SetNamedSourceData("TotalPrice", 0m);
                    resolver.SetNamedSourceData("TotalShipping", 0m);
                    resolver.SetNamedSourceData("GrandTotal", 0m);

                    mEcommerceResolver = resolver;
                }

                return mEcommerceResolver;
            }
        }


        /// <summary>
        /// E-commerce expiring e-product e-mail template macro resolver.
        /// </summary>
        public static MacroResolver EcommerceEproductExpirationResolver
        {
            get
            {
                if (mEcommerceEproductExpirationResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Expiring e-products
                    resolver.SetNamedSourceData("EproductsTable", (new DataTable()).Rows);

                    mEcommerceEproductExpirationResolver = resolver;
                }

                return mEcommerceEproductExpirationResolver;
            }
        }
    }
}