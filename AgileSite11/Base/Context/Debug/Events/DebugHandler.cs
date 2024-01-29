using System.Data;

using CMS.Base;

namespace CMS.Base
{
    /// <summary>
    /// Debug event handler.
    /// </summary>
    public class DebugHandler : SimpleHandler<DebugHandler, DebugEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="debugData">DataRow with debug information</param>
        public DebugEventArgs StartEvent(DataRow debugData)
        {
            var e = new DebugEventArgs()
                {
                    DebugData = debugData
                };

            return StartEvent(e);
        }
    }
}