using CMS.Base;

namespace CMS
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class HandlersExtensions
    {
        /// <summary>
        /// Finishes the event and raises the After event actions.
        /// </summary>
        /// <param name="handler">Handler to finish</param>
        public static void FinishEvent(this AbstractAdvancedHandler handler)
        {
            handler?.Finish();
        }


        /// <summary>
        /// Checks whether the context of execution should continue.
        /// </summary>
        /// <param name="handler">Handler to check</param>
        public static bool CanContinue(this AbstractAdvancedHandler handler)
        {
            if (handler == null)
            {
                return true;
            }
            
            return handler.Continue;
        }


        /// <summary>
        /// Disables the ability to cancel continuation of the execution context.
        /// </summary>
        /// <param name="handler">Handler to modify</param>
        public static void DontSupportCancel(this AbstractAdvancedHandler handler)
        {
            if (handler != null)
            {
                handler.SupportsCancel = false;
            }
        }
    }
}