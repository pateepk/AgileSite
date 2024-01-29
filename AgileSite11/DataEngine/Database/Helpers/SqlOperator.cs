using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines SQL operators
    /// </summary>
    public static class SqlOperator
    {
        /// <summary>
        /// Union operator
        /// </summary>
        public const string UNION = "UNION";


        /// <summary>
        /// Intersect operator
        /// </summary>
        public const string INTERSECT = "INTERSECT";


        /// <summary>
        /// Except operator
        /// </summary>
        public const string EXCEPT = "EXCEPT";


        /// <summary>
        /// Union all operator
        /// </summary>
        public const string UNION_ALL = "UNION ALL";
    }
}
