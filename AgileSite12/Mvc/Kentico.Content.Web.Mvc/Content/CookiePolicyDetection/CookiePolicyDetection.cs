using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Registers an end-point which enables the administration to detect 3rd party cookie policy.
    /// </summary>
    internal static class CookiePolicyDetection
    {
        /// <summary>
        /// Registers the end-point's route.
        /// </summary>
        public static void Initialize()
        {
            RouteRegistration.Instance.Add(routes => routes.Kentico().MapCookiePolicyDetectionRoute());
        }
    }
}
