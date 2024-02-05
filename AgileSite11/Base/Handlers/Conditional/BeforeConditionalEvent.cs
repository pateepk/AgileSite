using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Conditional event execute in before handler
    /// </summary>
    public class BeforeConditionalEvent<TArgs> : ConditionalEventBase<BeforeConditionalEvent<TArgs>, TArgs>
        where TArgs : EventArgs
    {
        /// <summary>
        /// If true, the event is canceled after calling the handler
        /// </summary>
        protected bool CancelEvent
        {
            get;
            set;
        }


        /// <summary>
        /// Ensures that the event gets cancelled after this handler is called
        /// </summary>
        public BeforeConditionalEvent<TArgs> ThenCancel()
        {
            CancelEvent = true;

            return TypedThis;
        }


        /// <summary>
        /// Ensures that the event continues after this handler is called. Also dismissed the ThenCancel call.
        /// </summary>
        public BeforeConditionalEvent<TArgs> ThenContinue()
        {
            CancelEvent = false;

            return TypedThis;
        }


        /// <summary>
        /// Executes the handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event arguments</param>
        protected override void ExecuteHandler(object sender, TArgs args)
        {
            base.ExecuteHandler(sender, args);

            if (CancelEvent)
            {
                var cmsArgs = args as CMSEventArgs;
                if (cmsArgs != null)
                {
                    cmsArgs.Cancel();
                }
                else
                {
                    throw new NotSupportedException("[BeforeConditionalEvent.ExecuteHandler]: Only events with arguments of type CMSEventArgs support cancelling.");
                }
            }
        }
    }
}
