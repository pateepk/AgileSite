using System;

namespace CMS.Base
{
    /// <summary>
    /// Simple System handler
    /// </summary>
    public class SimpleSystemHandler : SimpleHandler<SimpleSystemHandler, SystemEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleSystemHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public SimpleSystemHandler(SimpleSystemHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="ex">Raised exception</param>
        /// <param name="logException">If true, the exception is logged to the event log</param>
        public SystemEventArgs StartEvent(Exception ex, ref bool logException)
        {
            var e = new SystemEventArgs()
                {
                    Exception = ex,
                    LogException = logException
                };

            StartEvent(e);

            logException = e.LogException;

            return e;
        }
    }
}