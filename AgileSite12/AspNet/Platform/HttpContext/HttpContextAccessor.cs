using CMS;
using CMS.AspNet.Platform;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(IHttpContextAccessor), typeof(HttpContextAccessorImpl), Priority = RegistrationPriority.Fallback, Lifestyle = Lifestyle.Singleton)]

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Implementation providing <see cref="IHttpContext"/> implementation based on <see cref="CMSHttpContext.Current"/>.
    /// </summary>
    internal class HttpContextAccessorImpl : IHttpContextAccessor
    {
        const string KEY = "Kentico_HttpContextAccessorImpl";

        public IHttpContext HttpContext
        {
            get
            {
                if (CMSHttpContext.Current == null)
                {
                    return null;
                }

                var context = CMSHttpContext.Current.Items[KEY] as IHttpContext;
                if (context == null)
                {
                    context = new HttpContextImpl(CMSHttpContext.Current);
                    CMSHttpContext.Current.Items[KEY] = context;
                }

                return context;
            }
        }
    }
}
