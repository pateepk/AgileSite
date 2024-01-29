using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the types
    /// </summary>
    public enum TypeEnum
    {
        /// <summary>
        /// SQL server type, e.g. "nvarchar(max)"
        /// </summary>
        SQL = 1,

        /// <summary>
        /// Field type, e.g. "longtext"
        /// </summary>
        Field = 2,

        /// <summary>
        /// Schema type, e.g. "xs:string"
        /// </summary>
        Schema = 3
    }
}
