using System;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface providing access to priority of object.
    /// </summary>
    public interface IPrioritizable
    {
        /// <summary>
        /// Object priority.
        /// </summary>
        double Priority
        {
            get;
        }


        /// <summary>
        /// Updates object priority and returns update result.
        /// </summary>
        /// <param name="newPriority">New priority</param>
        /// <returns>Update result should contain error message or null if update succeeded.</returns>
        string TryUpdatePriority(double newPriority);
    }
}
