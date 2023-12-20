using CMS.Core;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Web farm task used to cancel email queue sending.
    /// </summary>
    internal class CancelEmailQueueSendingWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Cancels email queue sending.
        /// </summary>
        public override void ExecuteTask()
        {
            EmailHelper.Queue.CancelSending(false);
        }
    }
}
