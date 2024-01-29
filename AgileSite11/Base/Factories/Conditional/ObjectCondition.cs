using System;

namespace CMS.Base
{
    /// <summary>
    /// Generic object condition
    /// </summary>
    public class ObjectCondition<TInput> : IObjectCondition
    {
        /// <summary>
        /// Condition function which evaluates whether the object of the given type can be created based on the input data
        /// </summary>
        public Func<TInput, bool> Condition
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="condition">Condition for the factory</param>
        public ObjectCondition(Func<TInput, bool> condition)
        {
            Condition = condition;
        }


        /// <summary>
        /// Returns true if the condition over the given object matches
        /// </summary>
        /// <param name="value">Value to match</param>
        public bool Matches(object value)
        {
            if (Condition == null)
            {
                return true;
            }

            if (value is TInput)
            {
                // Evaluate the condition
                return Condition((TInput)value);
            }

            return false;   
        }
    }
}
