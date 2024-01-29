using System;
using System.Linq;
using System.Text;

namespace CMS.Core
{
    /// <summary>
    /// Determines kind of optimization which can be used for reduction number of generated web farm tasks of particular type.
    /// </summary>
    public enum WebFarmTaskOptimizeActionEnum
    {
        /// <summary>
        /// No optimization available.
        /// </summary>
        None = 0,

        /// <summary>
        /// Group data of same task type
        /// </summary>
        GroupData = 1
    }
}
