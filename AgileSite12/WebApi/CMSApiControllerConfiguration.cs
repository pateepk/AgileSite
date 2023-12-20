using System;

namespace CMS.WebApi
{
    /// <summary>
    /// Stores configuration for CMS API controller
    /// </summary>
    internal class CMSApiControllerConfiguration
    {
        /// <summary>
        /// Gets or sets controller type.
        /// </summary>
        public Type ControllerType { get; set; }


        /// <summary>
        /// Gets or sets flag indicating whether session state is required.
        /// </summary>
        public bool RequiresSessionState { get; set; }
    }
}
