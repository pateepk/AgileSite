using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Event arguments for portal engine 
    /// </summary>
    public class PortalEngineEventArgs : CMSEventArgs
    {
        /// <summary>
        /// State of request
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Zone instance
        /// </summary>
        public WebPartZoneInstance Zone
        {
            get;
            set;
        }
    }
}
