using System;

namespace CMS.Helpers.Internal
{
    /// <summary>
    /// Virtual context user data.
    /// </summary>
    public class VirtualContextUser
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; internal set; }


        /// <summary>
        /// User GUID.
        /// </summary>
        public Guid UserGuid { get; internal set; }
    }
}
