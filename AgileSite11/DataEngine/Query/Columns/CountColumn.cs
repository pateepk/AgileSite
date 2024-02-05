using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Count column
    /// </summary>
    public class CountColumn : AggregatedColumn
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression to count. Leave empty for COUNT(*)</param>
        public CountColumn(string expression = null) 
            : base(AggregationType.Count, expression)
        {
        }
    }
}
