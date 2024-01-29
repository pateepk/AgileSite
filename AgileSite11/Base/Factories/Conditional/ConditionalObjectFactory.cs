using System;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Object factory which includes condition on the object creation
    /// </summary>
    public class ConditionalObjectFactory<ObjectType> : ObjectFactory<ObjectType>, IConditionalObjectFactory
        where ObjectType : class, new()
    {
        /// <summary>
        /// Condition function which evaluates whether the object of the given type can be created based on the input data
        /// </summary>
        protected List<IObjectCondition> Conditions
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the factory is able to create the object based on the given parameter
        /// </summary>
        /// <param name="parameter">Object parameter</param>
        public override bool CanCreateObject(object parameter)
        {
            // Check the condition if exists
            if (Conditions != null)
            {
                foreach (var c in Conditions)
                {
                    // If one condition matches
                    if (c.Matches(parameter))
                    {
                        return true;
                    }
                }

                return false;
            }

            return base.CanCreateObject(parameter);
        }


        /// <summary>
        /// Adds the object condition to the factory
        /// </summary>
        /// <param name="condition">Condition that must be matched</param>
        public IConditionalObjectFactory WhenParameter<InputType>(Func<InputType, bool> condition)
        {
            if (Conditions == null)
            {
                Conditions = new List<IObjectCondition>();
            }

            Conditions.Add(new ObjectCondition<InputType>(condition));

            return this;
        }
    }
}
