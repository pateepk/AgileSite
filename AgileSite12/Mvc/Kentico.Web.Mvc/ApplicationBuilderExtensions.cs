using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables localization of ASP.NET model meta-data based on data annotation attributes.
        /// Display names or validation results declared with data annotation attributes can contain keys of Kentico resource strings that will be resolved automatically using Kentico localization API.
        /// The localization uses a custom model metadata provider based on data annotations and is therefore not compatible with other providers or their customizations.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void UseDataAnnotationsLocalization(this IApplicationBuilder builder)
        {
            ModelMetadataProviders.Current = new LocalizedDataAnnotationsModelMetadataProvider();

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RangeAttribute), typeof(LocalizedRangeAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RegularExpressionAttribute), typeof(LocalizedRegularExpressionAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredAttribute), typeof(LocalizedRequiredAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(StringLengthAttribute), typeof(LocalizedStringLengthAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MaxLengthAttribute), typeof(LocalizedMaxLengthAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MinLengthAttribute), typeof(LocalizedMinLengthAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(System.ComponentModel.DataAnnotations.CompareAttribute), typeof(LocalizedCompareAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(CreditCardAttribute), (metadata, context, attribute) => new LocalizedDataTypeAttributeAdapter(metadata, context, (DataTypeAttribute)attribute, "creditcard"));
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(EmailAddressAttribute), (metadata, context, attribute) => new LocalizedDataTypeAttributeAdapter(metadata, context, (DataTypeAttribute)attribute, "email"));
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(PhoneAttribute), (metadata, context, attribute) => new LocalizedDataTypeAttributeAdapter(metadata, context, (DataTypeAttribute)attribute, "phone"));
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(UrlAttribute), (metadata, context, attribute) => new LocalizedDataTypeAttributeAdapter(metadata, context, (DataTypeAttribute)attribute, "url"));
        }


        /// <summary>
        /// When request is sent from administration domain of the site, 
        /// adds header Access-Control-Allow-Origin into response to enable cross origin resource sharing (CORS) with the administration.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void UseResourceSharingWithAdministration(this IApplicationBuilder builder)
        {
            KenticoCorsModule.Initialize(new CorsConfiguration
            {
                AllowMethods = new[] { "GET", "POST" },
                AllowHeaders = new[] { "Content-Type" },
            });
        }
    }
}
