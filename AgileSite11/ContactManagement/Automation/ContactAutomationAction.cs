using CMS.Automation;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Base class for contact action.
    /// </summary>
    abstract public class ContactAutomationAction : AutomationAction
    {

        /// <summary>
        /// Gets current Contact object.
        /// </summary>
        public ContactInfo Contact
        {
            get
            {
                return (ContactInfo)InfoObject;
            }
        }

    }
}
