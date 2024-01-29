using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Defines a control with StopProcessing flag
    /// </summary>
    public interface IStopProcessing
    {
        /// <summary>
        /// Indicates if the control should stop processing
        /// </summary>
        bool StopProcessing
        {
            get;
            set;
        }
    }
}
