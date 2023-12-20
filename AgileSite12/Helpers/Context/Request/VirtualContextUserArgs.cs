using System;

namespace CMS.Helpers.Internal
{
    /// <summary>
    /// Virtual context user event data.
    /// </summary>
    public class VirtualContextUserArgs : EventArgs
    {
        /// <summary>
        /// UserName or UserGuid of the user to parse.
        /// </summary>
        public string UserNameOrGuid { get; set; }


        /// <summary>
        /// Parsed UserName.
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// Parsed UserGuid.
        /// </summary>
        public Guid UserGuid { get; set; }
    }
}
