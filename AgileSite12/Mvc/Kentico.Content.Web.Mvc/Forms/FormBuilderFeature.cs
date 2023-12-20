using System.Threading;
using System.Globalization;
using System.Web.Optimization;

using CMS.Membership;
using CMS.Helpers;

using Kentico.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Form builder feature.
    /// </summary>
    internal static class FormBuilderFeature
    {
        /// <summary>
        /// Initializes the Forms module by registering bundles and processing of the virtual context.
        /// </summary>
        public static void Initialize()
        {
            VirtualContextUrlProcessorsRegister.Instance.Add(FormBuilderUrlProcessor.Instance);

            VirtualContextPrincipalAssigner.Instance.UserAssigned.Execute += (s, e) => SetThreadCulture(Thread.CurrentThread);

            StylesBundleCollectionUtils.RegisterFormComponentsStyleBundles(BundleTable.Bundles);

            ScriptsBundleCollectionUtils.RegisterJQueryScriptBundle(BundleTable.Bundles);
            ScriptsBundleCollectionUtils.RegisterJQueryUnobtrusiveAjaxScriptBundle(BundleTable.Bundles);
            ScriptsBundleCollectionUtils.RegisterFormsScriptBundle(BundleTable.Bundles);

            RouteRegistration.Instance.Add(routes => routes.Kentico().MapFormBuilderRoutes());
        }


        /// <summary>
        /// Sets <see cref="Thread.CurrentUICulture"/> as well as <see cref="Thread.CurrentCulture"/> based on current user if in scope of the Form builder.
        /// </summary>
        /// <param name="thread">Thread to which culture should be set.</param>
        public static void SetThreadCulture(Thread thread)
        {
            if (VirtualContext.IsFormBuilderLinkInitialized)
            {
                var cultureName = MembershipContext.AuthenticatedUser.PreferredUICultureCode;
                var culture = CultureInfo.GetCultureInfo(cultureName);

                thread.CurrentUICulture = culture;
                thread.CurrentCulture = culture;
            }
        }
    }
}
