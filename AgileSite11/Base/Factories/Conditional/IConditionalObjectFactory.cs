using System;

namespace CMS.Base
{
    /// <summary>
    /// Interface for the conditional object factory
    /// </summary>
    public interface IConditionalObjectFactory
    {
        /// <summary>
        /// Adds the object condition to the factory
        /// </summary>
        /// <param name="condition">Condition that must be matched</param>
        IConditionalObjectFactory WhenParameter<InputType>(Func<InputType, bool> condition);
    }
}
