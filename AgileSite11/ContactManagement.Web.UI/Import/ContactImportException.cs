using System;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactImportException : Exception
    {
        private string mUIMessage;


        /// <summary>
        /// Message displayed in the UI to the user
        /// </summary>
        public string UIMessage
        {
            get
            {
                return mUIMessage ?? Message;
            }
            set
            {
                mUIMessage = value;
            }
        }


        public ContactImportException(string message, string uiMessage) 
            : base(message)
        {
            UIMessage = uiMessage;
        }


        public ContactImportException(string message, string uiMessage, Exception innerException) 
            : base(message, innerException)
        {
            UIMessage = uiMessage;
        }
    }
}