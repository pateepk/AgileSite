using System;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Represents an event invoker that can raise specified event for each item in collection.
    /// </summary>
    public sealed class AdvancedHandlerInvoker<TItem, THandler> : IDisposable
        where THandler : AbstractAdvancedHandler
    {
        private readonly List<THandler> mHandlers = new List<THandler>();
        
        
        /// <summary>
        /// Raise specified event for each item in collection.
        /// </summary>
        /// <param name="items">Items over which the handler iterates.</param>
        /// <param name="startFunc">Function which starts individual handler iterations.</param>
        public void StartEvents(IEnumerable<TItem> items, Func<TItem, THandler> startFunc)
        {
            foreach (var item in items)
            {
                var h = startFunc(item);

                mHandlers.Add(h);
            }
        }


        /// <summary>
        /// Executes <see cref="HandlersExtensions.FinishEvent"/> method for all handlers created within the item iteration.
        /// </summary>
        public void FinishEvents()
        {
            mHandlers.ForEach(h => h.FinishEvent());
        }


        /// <summary>
        /// Executes <see cref="AbstractHandler.Dispose"/> method for all handlers created within the item iteration.
        /// </summary>
        public void Dispose()
        {
            mHandlers.ForEach(h => h.Dispose());
        }
    }
}