using System;
using System.Linq;
using System.Text;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Indicates how coupled data columns are included in the multiple documents query.
    /// </summary>
    public enum IncludeCoupledDataEnum
    {
        /// <summary>
        /// Coupled data columns are included in the result as well as in the inner queries to be able to use them as part of the where conditions.
        /// </summary>
        Complete = 0,


        /// <summary>
        /// Coupled data columns are included only in the inner queries to be able to use them as part of the where conditions.
        /// </summary>
        InnerQueryOnly = 1,


        /// <summary>
        /// Coupled data columns are not included in the query at all.
        /// </summary>
        None = 2
    }
}
