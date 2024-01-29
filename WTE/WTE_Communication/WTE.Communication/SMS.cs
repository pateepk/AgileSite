using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WTE.Communication
{
    public class SMS
    {
        private string m_toPhoneNumber = string.Empty;
        private string m_message = string.Empty;
        private ConnectionInfo m_connectionInfo = null;
        private AsynchronousResponseHandler m_asyncResponseHandler = null;

        /// <summary>
        /// The connection to use
        /// </summary>
        public ConnectionInfo ConnectionInfo
        {
            get { return m_connectionInfo; }
            set { m_connectionInfo = value; }
        }

        /// <summary>
        /// Optional AsyncResponseHandler which can be called after email has been sent to communication server
        /// </summary>
        public AsynchronousResponseHandler AsyncResponseHandler
        {
            get { return m_asyncResponseHandler; }
            set { m_asyncResponseHandler = value; }
        }

        public string ToPhoneNumber
        {
            get { return m_toPhoneNumber; }
            set { m_toPhoneNumber = value; }
        }

        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        public SMS()
        {
        }

        public SMS(ConnectionInfo p_connectionInfo,
            string p_ToPhoneNumber, string p_Message)
            : this(p_connectionInfo,
             p_ToPhoneNumber, p_Message,
             null)
        {
        }

        public SMS(ConnectionInfo p_connectionInfo,
            string p_ToPhoneNumber, string p_Message,
            AsynchronousResponseHandler p_asynchronousResponseHandler)
            : this()
        {
            ConnectionInfo = p_connectionInfo;
            ToPhoneNumber = p_ToPhoneNumber;
            Message = p_Message;
            AsyncResponseHandler = p_asynchronousResponseHandler;
        }
    }
}
