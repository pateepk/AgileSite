using System;
using System.Security.Principal;
using System.Web;

using CMS.Base;
using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Uses custom principal implementation for virtual context links requests to bypass authorization.
    /// </summary>
    internal class VirtualContextPrincipalAssigner
    {
        private static Lazy<VirtualContextPrincipalAssigner> instance = new Lazy<VirtualContextPrincipalAssigner>(() => new VirtualContextPrincipalAssigner());


        /// <summary>
        /// Event fires after assigning <see cref="IPrincipal"/> to the <see cref="HttpContext"/>.
        /// </summary>
        public readonly SimpleHandler UserAssigned = new SimpleHandler
        {
            Name = "VirtualContextPrincipalAssigner.UserAssigned"
        };


        /// <summary>
        /// Returns instance of the <see cref="VirtualContextPrincipalAssigner"/>.
        /// </summary>
        public static VirtualContextPrincipalAssigner Instance => instance.Value;


        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualContextPrincipalAssigner"/>. 
        /// </summary>
        protected VirtualContextPrincipalAssigner()
        {
        }


        /// <summary>
        /// Registers user's virtual context processing to the request pipeline.
        /// </summary>
        /// <remarks>
        /// This method has to be called only once within the application life-cycle. Thread-safety must be ensured by caller.
        /// </remarks>
        public static void Initialize()
        {
            RequestEvents.PostAuthenticate.Execute += (object sender, EventArgs eventArgs) =>
            {
                if (!VirtualContext.IsInitialized)
                {
                    return;
                }

                HttpContext.Current.User = new VirtualContextPrincipal();
                Instance.UserAssigned.StartEvent(new EventArgs());
            };
        }
    }
}
