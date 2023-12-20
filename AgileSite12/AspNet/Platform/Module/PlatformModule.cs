using System.Web.Security;

using CMS;
using CMS.AspNet.Platform;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterModule(typeof(PlatformModule))]

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Represents the module providing basic full framework implementations.
    /// </summary>
    internal class PlatformModule : Module
    {
        /// <summary>
        /// Creates new instance of <see cref="PlatformModule"/>
        /// </summary>
        public PlatformModule()
            : base("CMS.AspNet.Platform")
        {
        }


        /// <summary>
        /// Handles the module pre-initialization.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            SystemEvents.RestartRequired.Execute += (s, e) => ApplicationRuntime.Restart(e);

            ObjectFactory<IHttpCookie>.SetObjectTypeTo<HttpCookieImpl>();

            CookieHelper.RegisterCookie(CookieName.ASPNETSessionID, CookieLevel.System, true);
            CookieHelper.RegisterCookie(FormsAuthentication.FormsCookieName, CookieLevel.Essential, true);
        }


        /// <summary>
        /// Handles the module initialization.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ApplicationEvents.Error.Execute += (s, e) => ApplicationErrorLogger.LogLastApplicationError();
        }
    }
}
