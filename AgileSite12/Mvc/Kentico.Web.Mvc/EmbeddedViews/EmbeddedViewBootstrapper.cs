using System.Web;
using System.Web.Mvc;

[assembly: PreApplicationStartMethod(typeof(Kentico.Web.Mvc.EmbeddedViewBootstrapper), "Register")]

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Ensures initialization of view engine used for embedded system views.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public class EmbeddedViewBootstrapper
    {
        /// <summary>
        /// Registers dynamic module with initialization routine.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public static void Register()
        {
            HttpApplication.RegisterModule(typeof(EmbeddedViewModuleInitializer));
        }


        private class EmbeddedViewModuleInitializer : IHttpModule
        {
            private static bool registered;
            private static readonly object locker = new object();


            public void Init(HttpApplication context)
            {
                if (registered)
                {
                    return;
                }

                lock (locker)
                {
                    if (!registered)
                    {
                        ViewEngines.Engines.Add(new EmbeddedViewEngine());

                        registered = true;
                    }
                }
            }


            public void Dispose()
            {
                // Nothing to dispose
            }
        }
    }
}
