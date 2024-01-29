using System;

using CMS;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Synchronization;

[assembly: RegisterModule(typeof(ECommerceModule))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the E-commerce module.
    /// </summary>
    public class ECommerceModule : Module
    {
        #region "Variables & Constants"

        internal const string ECOMMERCE = "##ECOMMERCE##";
        private ECommerceActionContext mECommerceActionContext;


        /// <summary>
        /// Name of email template type for ecommerce.
        /// </summary>
        public const string ECOMMERCE_EMAIL_TEMPLATE_TYPE_NAME = "ecommerce";


        /// <summary>
        /// Name of email template type for ecommerce e-product expiration.
        /// </summary>
        public const string ECOMMERCE_EPRODUCT_EXPIRATION_EMAIL_TEMPLATE_TYPE_NAME = "ecommerceeproductexpiration";

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ECommerceModule()
            : base(new ECommerceModuleMetadata())
        {
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            InitImportExport();

            InitStaging();

            InitMacros();

            // Register handlers for Ecommerce-MacroEngine interaction
            MacroEngineHandlers.Init();

            // Register handlers for Ecommerce-Membership interaction
            MembershipHandlers.Init();

            // Handle actions for SKUs within document actions
            DocumentHandlers.Init();
            DocumentGenerator.RegisterDefaultFactory(new SKUTreeNodeFactory());

            // Register actions for Ecommerce-Conversions interaction
            ConversionHandlers.Init();

            // Register actions for Ecommerce-Contact interaction
            ContactHandlers.Init();

            LicenseCheckEvents.ObjectCountCheckEvent.Execute += ObjectCountCheckEventOnExecute;
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        ///  Fires after the staging processes task.
        /// </summary>
        private void SynchronizeTask_After(object sender, StagingTaskEventArgs e)
        {
            // After staging task is completed, ecommerce action context is not needed  
            mECommerceActionContext?.Dispose();
        }


        /// <summary>
        /// Fires before the staging processes task.
        /// </summary>
        private void ProcessTask_Before(object sender, StagingSynchronizationEventArgs e)
        {
            // Create and set ecommerce action context to avoid recalculating the parent product price from the cheapest product variant
            mECommerceActionContext = new ECommerceActionContext
                                      {
                                          SetLowestPriceToParent = false,
                                      };
        }


        /// <summary>
        /// Sets the SKU count for the current website to the <paramref name="eventArgs"/>. 
        /// </summary>
        private void ObjectCountCheckEventOnExecute(object sender, ObjectCountCheckEventArgs eventArgs)
        {
            if (eventArgs.Feature == FeatureEnum.Ecommerce)
            {
                eventArgs.ObjectCount = SKUInfoProvider.GetObjectCountForLicenseCheck();
            }
        }

        #endregion


        #region "Methods"

        private static void InitImportExport()
        {
            ImportSpecialActions.Init();
            ExportSpecialActions.Init();
            SKUImport.Init();
        }


        private void InitStaging()
        {
            StagingEvents.ProcessTask.Before += ProcessTask_Before;
            StagingEvents.SynchronizeTask.After += SynchronizeTask_After;
        }


        private static void InitMacros()
        {
            RegisterContext<ECommerceContext>("ECommerceContext");

            ExtendList<MacroResolverStorage, MacroResolver>.With("SKUResolver").WithLazyInitialization(() => EcommerceResolvers.SKUResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("CalculationResolver").WithLazyInitialization(() => EcommerceResolvers.CalculationResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("EcommerceResolver").WithLazyInitialization(() => EcommerceResolvers.EcommerceResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("EcommerceEproductExpirationResolver").WithLazyInitialization(() => EcommerceResolvers.EcommerceEproductExpirationResolver);
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action</param>
        [Obsolete("Use module specific method if available or change dependency for module you can't access directly.")]
        protected override bool CheckLicenseInternal(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            return SKUInfoProvider.LicenseVersionCheck(domain, feature, action);
        }

        #endregion
    }
}