using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Contains helper methods extending email confirmation functionality (newsletters, user registration) with A/B testing features.
    /// </summary>
    internal static class ABTestConfirmationHelper
    {
        /// <summary>
        /// Defines character which is used to separate variant identifiers in a string.
        /// </summary>
        internal const string VARIANTS_SEPARATOR = "|";


        /// <summary>
        /// Name of query string parameter which is appended to confirmation URL within newsletter double opt-in email.
        /// </summary>
        internal const string VARIANT_DOUBLEOPTIN_PARAMETER = "abvariant";


        /// <summary>
        /// Name used as a key under which variant identifiers are stored in user's registration data.
        /// </summary>
        internal const string VARIANTS_REGISTRATIONDATA_KEY = "KenticoABTestVariants";


        /// <summary>
        /// Returns query parameters which are used to extend double opt-in URL.
        /// </summary>
        public static NameValueCollection GetQueryParameters()
        {
            var parameters = new NameValueCollection();

            var variants = String.Join(VARIANTS_SEPARATOR, ABTestHelper.GetValidVariants());
            if (!String.IsNullOrEmpty(variants))
            {
                parameters.Add(VARIANT_DOUBLEOPTIN_PARAMETER, variants);
            }

            return parameters;
        }


        /// <summary>
        /// Assigns user A/B test variants, obtained from <paramref name="queryString"/>. Used on newsletter double opt-in confirmation.
        /// </summary>
        public static void HandleNewsletterDoubleOptInConfirmation(NameValueCollection queryString)
        {
            if (queryString == null || !queryString.HasKeys())
            {
                return;
            }

            var variants = queryString[VARIANT_DOUBLEOPTIN_PARAMETER];
            if (variants == null)
            {
                return;
            }

            AssignUserVariants(variants);
        }


        /// <summary>
        /// Stores A/B test variants to user registration data. Used on before registration confirmation email is sent.
        /// </summary>
        public static void StoreUserABVariants(int userId)
        {
            var user = UserInfoProvider.GetUserInfo(userId);
            if (user == null)
            {
                return;
            }

            var variants = String.Join(VARIANTS_SEPARATOR, ABTestHelper.GetValidVariants());
            if (!String.IsNullOrEmpty(variants))
            {
                user.UserSettings.UserRegistrationInfo.SetValue(VARIANTS_REGISTRATIONDATA_KEY, variants);
                UserInfoProvider.SetUserInfo(user);
            }
        }


        /// <summary>
        /// Assigns user A/B test variants obtained from registration data of user defined by <paramref name="userId"/>. Used on registration email confirmation.
        /// </summary>
        public static void RestoreUserABVariants(int userId)
        {
            var user = UserInfoProvider.GetUserInfo(userId);
            if (user == null)
            {
                return;
            }

            if (user.UserSettings.UserRegistrationInfo.TryGetValue(VARIANTS_REGISTRATIONDATA_KEY, out object variants) && (variants != null))
            {
                // Assign variants to current user
                AssignUserVariants((string)variants);

                // Clear stored variant data
                user.UserSettings.UserRegistrationInfo.Remove(VARIANTS_REGISTRATIONDATA_KEY);
                UserInfoProvider.SetUserInfo(user);
            }
        }


        /// <summary>
        /// Returns collection of variant GUIDs parsed from given string where the variants are separated with <see cref="VARIANTS_SEPARATOR"/>.
        /// </summary>
        private static IEnumerable<Guid> GetVariantGuids(string variants)
        {
            return variants.Split(new[] { VARIANTS_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).Select(s => ValidationHelper.GetGuid(s, Guid.Empty));
        }


        /// <summary>
        /// Assigns user A/B test variants from given <paramref name="variants"/>.
        /// </summary>
        private static void AssignUserVariants(string variants)
        {
            var variantGuids = GetVariantGuids(variants);

            foreach (var variant in variantGuids.Where(v => v != Guid.Empty))
            {
                var abTestId = ABVariantDataInfoProvider.GetVariantsData().Columns("ABVariantTestID").WhereEquals("ABVariantGUID", variant).GetScalarResult(0);
                var abTest = ABTestInfoProvider.GetABTestInfo(abTestId);
                if (abTest == null)
                {
                    continue;
                }

                var abUserStateManager = Service.Resolve<IABUserStateManagerFactory>().Create<Guid>(abTest.ABTestName);
                abUserStateManager.AssignVariant(variant);
            }
        }
    }
}
