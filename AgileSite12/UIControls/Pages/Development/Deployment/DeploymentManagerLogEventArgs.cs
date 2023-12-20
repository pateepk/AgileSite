using System;
using System.Linq;
using System.Text;

namespace CMS.UIControls.Internal
{
    /// <summary>
    /// Deployment manager log EventArgs
    /// </summary>
    /// <remarks>This class is not indented to be used in custom code.</remarks>
    public class DeploymentManagerLogEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the log messages
        /// </summary>
        public string Message
        {
            get;
            internal set;
        }
    }
}
