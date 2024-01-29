using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce.Automation
{
    /// <summary>
    /// Provides automation support for common actions related to Salesforce.com integration.
    /// </summary>
    public class AutomationHelper : AbstractHelper<AutomationHelper>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the AutomationHelper class.
        /// </summary>
        public AutomationHelper()
        {

        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Replicates the specified contact to Salesforce leads, if applicable, and returns the result.
        /// </summary>
        /// <param name="contact">The contact to replicate.</param>
        public static ReplicateLeadCommandResultEnum ReplicateLead(ContactInfo contact)
        {
            return ReplicateLead(contact, false);
        }


        /// <summary>
        /// Replicates the specified contact to Salesforce leads, if applicable, and returns the result.
        /// </summary>
        /// <param name="contact">The contact to replicate.</param>
        /// <param name="deferredReplication">A value indicating whether the specified contact will not be replicated immediately, but during the next scheduled replication.</param>
        public static ReplicateLeadCommandResultEnum ReplicateLead(ContactInfo contact, bool deferredReplication)
        {
            return HelperObject.ReplicateLeadInternal(contact, deferredReplication);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Replicates the specified contact to Salesforce leads, if applicable, and returns the result.
        /// </summary>
        /// <param name="contact">The contact to replicate.</param>
        /// <param name="deferredReplication">A value indicating whether the specified contact will not be replicated immediately, but during the next scheduled replication.</param>
        protected virtual ReplicateLeadCommandResultEnum ReplicateLeadInternal(ContactInfo contact, bool deferredReplication)
        {
            ReplicateLeadCommand command = new ReplicateLeadCommand
            {
                DeferredReplication = deferredReplication
            };

            return command.Execute(contact);
        }

        #endregion

    }

}
