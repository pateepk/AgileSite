using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Interface for control that is able to explicitly ensure its child controls
    /// </summary>
    public interface IEnsureControls
    {
        /// <summary>
        /// Ensures the child controls collection
        /// </summary>
        void EnsureControls();
    }
}
