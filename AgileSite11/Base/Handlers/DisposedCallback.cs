using System;

namespace CMS.Base
{
    /// <summary>
    /// Represents a method wrapper for a method which is called when the wrapper is disposed
    /// </summary>
    public class OnDisposedCallback : IDisposable
    {
        /// <summary>
        /// Method called when this object is disposed
        /// </summary>
        public Action Callback 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="callback">Callback to execute when the object is disposes</param>
        public OnDisposedCallback(Action callback)
        {
            Callback = callback;
        }


        /// <summary>
        /// Fires when the object disposes
        /// </summary>
        public void Dispose()
        {
            Callback();
        }
    }
}
