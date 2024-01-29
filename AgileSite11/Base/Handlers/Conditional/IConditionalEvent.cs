using System;

namespace CMS.Base
{
    /// <summary>
    /// Interface for conditional events
    /// </summary>
    public interface IConditionalEvent<TEvent>
    {
        /// <summary>
        /// Adds the condition to the conditional event
        /// </summary>
        TEvent When(Func<bool> condition);
    }
}