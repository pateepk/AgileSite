using System;
using System.Security.Principal;

using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Task for re-sending remaining newsletters in newsletter queue
    /// after main thread sender exit unexpectedly.
    /// </summary>
    public class NewsletterSender : ITask
    {
        /// <summary>
        ///  Sends all emails in newsletter queue.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            // Check license, do not send e-mails if license not valid
            if (!String.IsNullOrEmpty(RequestContext.CurrentDomain) && !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Newsletters))
            {
                return null;
            }

            var sender = new ThreadEmailSender
            {
                SendFailed = true, 
                SendNew = true
            };

            sender.RunAsync(WindowsIdentity.GetCurrent());

            return string.Empty;
        }
    }
}