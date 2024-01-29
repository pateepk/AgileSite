using System;

namespace CMS.Base
{
    /// <summary>
    /// Base class for advanced handler classes
    /// </summary>
    public abstract class AbstractAdvancedHandler : AbstractHandler
    {
        /// <summary>
        /// Specifies whether the context of execution should continue. If set to false, no additional events will be fired.
        /// </summary>
        protected internal abstract bool Continue
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the handler supports cancelling of the event. If set and handler is already cancelled, throws an exception.
        /// </summary>
        protected internal abstract bool SupportsCancel
        {
            get;
            set;
        }


        /// <summary>
        /// Finishes the event and raises the After event actions
        /// </summary>
        protected internal abstract void Finish();
    }
}
