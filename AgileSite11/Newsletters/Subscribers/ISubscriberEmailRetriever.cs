using System;
using System.Linq;
using System.Text;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for retrieving subscriber email address.
    /// </summary>
    public interface ISubscriberEmailRetriever
    {
        /// <summary>
        /// Returns email address for given subscriber.
        /// Does work with user and contact subscribers.
        /// Returns null for group subscriber -> role, contact group and persona.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        string GetSubscriberEmail(int subscriberID);


        /// <summary>
        /// Get email for group subscriber (Contact group or Persona).
        /// </summary>
        /// <param name="contactID">ID of contact that email is retrieved for</param>
        /// <returns>Null when contact was not found</returns>
        string GetEmailForContact(int contactID);
    }
}
