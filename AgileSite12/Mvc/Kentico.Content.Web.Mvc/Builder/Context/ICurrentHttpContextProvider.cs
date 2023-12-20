using System.Web;

using CMS;
using CMS.Core;

using Kentico.Builder.Web.Mvc;

[assembly: RegisterImplementation(typeof(ICurrentHttpContextProvider), typeof(CurrentHttpContextProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Defines a method for retrieving current HTTP context.
    /// </summary>
    internal interface ICurrentHttpContextProvider
    {
        /// <summary>
        /// Gets an instance of current HTTP context.
        /// </summary>
        /// <returns>Returns an instance of HTTP context.</returns>
        HttpContextBase Get();
    }
}
