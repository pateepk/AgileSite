using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a dynamic data parameter which value is resolved at the execution of the query, not when the parameter is defined
    /// </summary>
    public class DynamicDataParameter : DataParameter
    {
        /// <summary>
        /// Function to get parameter value
        /// </summary>
        private Func<object> GetValue
        {
            get;
            set;
        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="getValue">Function to get parameter value</param>
        public DynamicDataParameter(string name, Func<object> getValue)
            : base(name, getValue)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException("getValue");
            }

            GetValue = getValue;
        }


        /// <summary>
        /// Gets the value for the parameter comparison
        /// </summary>
        protected override object GetValueToCompare()
        {
            return GetValue;
        }


        /// <summary>
        /// Gets the current value for query execution
        /// </summary>
        protected override object GetCurrentValue()
        {
            return GetValue();
        }


        /// <summary>
        /// Clones the parameter
        /// </summary>
        public override DataParameter Clone()
        {
            var p = new DynamicDataParameter(Name, GetValue);
            p.Type = Type;

            return p;
        }
    }
}
