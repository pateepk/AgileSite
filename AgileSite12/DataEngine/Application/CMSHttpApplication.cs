using System;
using System.Reflection;
using System.Web;

using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Envelope class with request mappings for HttpApplication class
    /// </summary>
    public class CMSHttpApplication : HttpApplication
    {
        #region "Application events"

        /// <summary>
        /// Application error event handler.
        /// </summary>
        public void Application_Error(object sender, EventArgs e)
        {
            CMSApplication.ApplicationError();
        }


        /// <summary>
        /// Application end event handler.
        /// </summary>
        public void Application_End(object sender, EventArgs e)
        {
            CMSApplication.ApplicationEnd();
        }
        
        #endregion


        #region "Session events"

        /// <summary>
        /// Session start event handler.
        /// </summary>
        public void Session_Start(object sender, EventArgs e)
        {
            SessionStart();
        }


        /// <summary>
        /// Session end event handler.
        /// </summary>
        public void Session_End(object sender, EventArgs e)
        {
            SessionEnd();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Custom cache parameters processing.
        /// </summary>
        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            // Get base value
            var result = base.GetVaryByCustomString(context, custom);

            var e = new GetVaryByCustomStringEventArgs
                {
                    Context = context,
                    Custom = custom,
                    Result = result
                };

            return CMSApplication.GetVaryByCustomString(e);
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Initialize CMS application from a web-based environment.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not intended to be used in custom code.
        /// </para>
        /// <para>
        /// Application start ensures that the <see cref="CMSApplication.PreInit()"/> and <see cref="CMSApplication.Init"/> methods are called.
        /// </para>
        /// </remarks>
        /// <param name="webProjectAssembly">Web project assembly.</param>
        protected static void InitApplication(Assembly webProjectAssembly)
        {
            AssemblyDiscoveryHelper.RegisterAdditionalAssemblies(webProjectAssembly);

            // Set the flag that indicates whether the current hosting environment is CMS application
            SystemContext.IsCMSRunningAsMainApplication = true;

            if (CMSApplication.PreInit())
            {
                // Ensure application initialization
                RequestEvents.Prepare.Execute += (sender, e) => CMSApplication.Init();
            }
        }


        /// <summary>
        /// Fires the session start event
        /// </summary>
        private static void SessionStart()
        {
            ApplicationEvents.SessionStart.StartEvent(new EventArgs());
        }


        /// <summary>
        /// Fires the session end event
        /// </summary>
        public static void SessionEnd()
        {
            ApplicationEvents.SessionEnd.StartEvent(new EventArgs());
        }

        #endregion
    }
}