using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.OnlineForms;
using CMS.OnlineMarketing;
using CMS.SiteProvider;

using Kentico.Membership;
using Kentico.Membership.Internal;
using Kentico.PageBuilder.Web.Mvc.Internal;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Contains event handlers of the A/B testing module.
    /// </summary>
    internal static class ABTestingHandlers
    {
        private static IHttpContextAccessor mHttpContextAccessor;
        private static IABTestLogger mABTestLogger;


        private static IHttpContextAccessor HttpContextAccessor => mHttpContextAccessor ?? (mHttpContextAccessor = Service.Resolve<IHttpContextAccessor>());


        private static IABTestLogger ABTestLogger => mABTestLogger ?? (mABTestLogger = Service.Resolve<IABTestLogger>());


        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            PageBuilderScriptConfigurationSource.Instance.GetConfiguration.Execute += AddABTestVariantParameter;
            BizFormItemEvents.Insert.After += LogFormSubmissionConversion;

            SubscriberNewsletterInfo.TYPEINFO.Events.Insert.After += LogNewsletterSubscriptionConversion;
            SubscriberNewsletterInfo.TYPEINFO.Events.Update.Before += LogNewsletterSubscriptionConversionUpdate;
            DoubleOptInExtensionDefinitionRegister.Instance.Register(new DoubleOptInExtensionDefinition(ABTestConfirmationHelper.GetQueryParameters, ABTestConfirmationHelper.HandleNewsletterDoubleOptInConfirmation));

            EcommerceEvents.NewOrderCreated.Execute += LogPurchaseConversion;
            EcommerceEvents.ProductAddedToShoppingCart.Execute += LogProductAddedToCartConversion;

            MembershipEvents.RegistrationCompleted.Execute += LogUserRegistrationConversion;
            EmailConfirmationExtensionDefinitionRegister.Instance.Register(new EmailConfirmationExtensionDefinition(ABTestConfirmationHelper.StoreUserABVariants, ABTestConfirmationHelper.RestoreUserABVariants));
        }


        /// <summary>
        /// Handles <see cref="PageBuilderScriptConfigurationSource.GetConfiguration"/> event by adding A/B test variant query string parameter.
        /// </summary>
        public static void AddABTestVariantParameter(object sender, GetConfigurationEventArgs e)
        {
            if (!CMS.OnlineMarketing.ABTestInfoProvider.ABTestingEnabled(SiteContext.CurrentSiteName))
            {
                return;
            }

            var configuration = e.Configuration;

            configuration.ConfigurationLoadEndpoint = ForwardVariantIdentifierParameter(ABTestConstants.AB_TEST_VARIANT_QUERY_STRING_PARAMETER_NAME, configuration.ConfigurationLoadEndpoint);
        }


        /// <summary>
        /// Forwards query string parameter from current request URL to given <paramref name="url"/>.
        /// </summary>
        private static string ForwardVariantIdentifierParameter(string parameterName, string url)
        {
            var parameterValue = HttpContextAccessor.HttpContext.Request.QueryString[parameterName];

            return String.IsNullOrEmpty(parameterValue) ? url : URLHelper.AddParameterToUrl(url, parameterName, parameterValue);
        }


        /// <summary>
        /// Logs form submission conversion.
        /// </summary>
        private static void LogFormSubmissionConversion(object sender, BizFormItemEventArgs e)
        {
            var form = e.Item.BizFormInfo;
            ABTestLogger.LogConversion(ABTestConversionNames.FORM_SUBMISSION, SiteInfoProvider.GetSiteName(form.FormSiteID), form.FormName);
        }


        /// <summary>
        /// Logs newsletter subscription conversion.
        /// </summary>
        private static void LogNewsletterSubscriptionConversion(object sender, ObjectEventArgs e)
        {
            var binding = (SubscriberNewsletterInfo)e.Object;
            if (!binding.SubscriptionApproved)
            {
                return;
            }

            LogNewsletterSubscriptionConversion(binding);
        }


        /// <summary>
        /// Logs newsletter subscription conversion when optIn approval is required.
        /// </summary>
        private static void LogNewsletterSubscriptionConversionUpdate(object sender, ObjectEventArgs e)
        {
            var binding = (SubscriberNewsletterInfo)e.Object;
            if (!binding.ChangedColumns().Contains("SubscriptionApproved") || !binding.SubscriptionApproved)
            {
                return;
            }

            e.CallWhenFinished(() => { LogNewsletterSubscriptionConversion(binding); });
        }


        private static void LogNewsletterSubscriptionConversion(SubscriberNewsletterInfo binding)
        {
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(binding.NewsletterID);

            ABTestLogger.LogConversion(ABTestConversionNames.NEWSLETTER_SUBSCRIPTION, SiteInfoProvider.GetSiteName(newsletter.NewsletterSiteID), newsletter.NewsletterName);
        }


        private static void LogPurchaseConversion(object sender, NewOrderCreatedEventArgs e)
        {
            var order = e.NewOrder;
            var items = OrderItemInfoProvider.GetOrderItems(order.OrderID);
            var siteName = SiteInfoProvider.GetSiteName(order.OrderSiteID);

            foreach (var item in items)
            {
                ABTestLogger.LogConversion(ABTestConversionNames.PRODUCT_PURCHASE, siteName, item.OrderItemSKUID.ToString(), item.OrderItemTotalPriceInMainCurrency, item.OrderItemUnitCount);
            }

            ABTestLogger.LogConversion(ABTestConversionNames.PURCHASE, siteName, defaultValue: order.OrderGrandTotalInMainCurrency);
        }


        private static void LogProductAddedToCartConversion(object sender, ProductAddedToCartEventArgs e)
        {
            var item = e.AddedShoppingCartItem;
            var siteName = item.ShoppingCart?.SiteName ?? SiteContext.CurrentSiteName;

            ABTestLogger.LogConversion(ABTestConversionNames.PRODUCT_ADDED_TO_CART, siteName, item.SKUID.ToString());
        }


        /// <summary>
        /// Logs user registration conversion.
        /// </summary>
        private static void LogUserRegistrationConversion(object sender, RegistrationCompletedEventArgs e)
        {
            ABTestLogger.LogConversion(ABTestConversionNames.USER_REGISTRATION, SiteContext.CurrentSiteName);
        }
    }
}
