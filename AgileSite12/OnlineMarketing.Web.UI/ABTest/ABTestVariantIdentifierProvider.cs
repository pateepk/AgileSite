using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.OnlineMarketing.Web.UI;

[assembly: RegisterImplementation(typeof(IABTestVariantIdentifierProvider), typeof(ABTestVariantIdentifierProvider), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Provides A/B test variant identifier.
    /// </summary>
    internal class ABTestVariantIdentifierProvider : IABTestVariantIdentifierProvider
    {
        /// <summary>
        /// Defines the name of the Form's field persisting an identifier of the A/B variant to be saved.
        /// </summary>
        public const string VARIANT_IDENTIFIER_FORM_FIELD_NAME = "ABTestVariantProvider_VariantIdentifierFormFieldName";


        private readonly IHttpContextAccessor httpContextAccessor;


        /// <summary>
        /// Initializes a new instance of the <see cref="ABTestVariantIdentifierProvider" />.
        /// </summary>
        /// <param name="httpContextAccessor">Encapsulates the access to <see cref="IHttpContext"/> instance.</param>
        public ABTestVariantIdentifierProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        /// <summary>
        /// Returns a variant identifer for which the Page builder's configuration is to be saved.
        /// </summary>
        public Guid? GetVariantIdentifier()
        {
            var value = httpContextAccessor.HttpContext.Request.Form.Get(VARIANT_IDENTIFIER_FORM_FIELD_NAME);

            return Guid.TryParse(value, out Guid result) ? result : null as Guid?;
        }
    }
}
