using System;
using System.Linq;
using System.Text;

namespace CMS.Newsletters
{
    /// <summary>
    /// Validates subscription hash.
    /// </summary>
    public interface ISubscriptionHashValidator
    {
        /// <summary>
        /// Validates the subscription and unsubscription hash.
        /// </summary>
        /// <param name="requestHash">Hash to validate</param>
        /// <param name="siteName">Site name</param>
        /// <param name="datetime">Date time</param>
        HashValidationResult Validate(string requestHash, string siteName, DateTime datetime);
    }
}
