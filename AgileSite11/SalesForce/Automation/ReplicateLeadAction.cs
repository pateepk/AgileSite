using System;
using System.Text;

using CMS.ContactManagement;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.SalesForce.Automation
{

    /// <summary>
    /// Represents the "Replicate lead" workflow action.
    /// </summary>
    public class ReplicateLeadAction : ContactAutomationAction
    {

        #region "Constants"

        private const string EVENT_CODE = "Salesforce_ReplicateLead";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets a value indicating whether the contact will not be replicated immediately, but during the next scheduled replication.
        /// </summary>
        protected virtual bool DeferredReplication
        {
            get
            {
                return GetResolvedParameter<bool>("DeferredReplication", false);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Executes the action.
        /// </summary>
        public override void Execute()
        {
            if (Contact != null)
            {
                try
                {
                    ReplicateLeadCommandResultEnum result = AutomationHelper.ReplicateLead(Contact, DeferredReplication);
                    if (result != ReplicateLeadCommandResultEnum.Success)
                    {
                        string message = GetErrorMessage(result);
                        LogMessage(EventType.WARNING, EVENT_CODE, message, Contact);
                    }
                }
                catch (SalesForceReplicationException exception)
                {
                    string description = exception.Errors.Join(Environment.NewLine);
                    StringBuilder message = new StringBuilder();
                    message.AppendLine(exception.Message);
                    message.AppendLine();
                    message.AppendLine(description);
                    LogMessage(EventType.ERROR, EVENT_CODE, message.ToString(), Contact);
                }
                catch (SalesForceException exception)
                {
                    LogMessage(EventType.ERROR, EVENT_CODE, exception.Message, Contact);
                }
                catch (Exception exception)
                {
                    string format = ResHelper.GetAPIString("sf.automation.error", "There was an error processing your request: {0}");
                    string message = String.Format(format, !String.IsNullOrEmpty(exception.Message) ? exception.Message : exception.ToString());
                    LogMessage(EventType.ERROR, EVENT_CODE, message, Contact);
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates a localized error message for the specified command result, and returns it.
        /// </summary>
        /// <param name="result">The "Replicate lead" command result.</param>
        /// <returns>A localized error message for the specified command result.</returns>
        private string GetErrorMessage(ReplicateLeadCommandResultEnum result)
        {
            string name = Enum.GetName(result.GetType(), result);
            string resourceName = String.Format("sf.automation.errors.{0}", name);
            string defaultErrorMessage = String.Format("Your request could not be completed [{0}].", name);

            return ResHelper.GetAPIString(resourceName, defaultErrorMessage);
        }

        #endregion

    }

}