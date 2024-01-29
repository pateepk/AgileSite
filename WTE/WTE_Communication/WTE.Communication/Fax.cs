namespace WTE.Communication
{
    public class Fax
    {
        private string m_faxNumber = string.Empty;
        private string m_faxFrom = string.Empty;
        private string m_message = string.Empty;
        private string m_format = string.Empty;
        private ConnectionInfo m_connectionInfo = null;
        private AsynchronousResponseHandler m_asyncResponseHandler = null;

        public string FaxNumber
        {
            get { return m_faxNumber; }
            set { m_faxNumber = value; }
        }

        public string FaxFrom
        {
            get { return m_faxFrom; }
            set { m_faxFrom = value; }
        }

        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        public string Format
        {
            get { return m_format; }
            set { m_format = value; }
        }

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

        public Fax()
        {
        }

        public Fax(ConnectionInfo p_connectionInfo,
            string p_FaxNumber,
            string p_FaxFrom, string p_Message, string p_Format)
            : this(p_connectionInfo,
             p_FaxNumber,
             p_FaxFrom, p_Message, p_Format, null)
        {
        }

        public Fax(ConnectionInfo p_connectionInfo,
            string p_FaxNumber,
            string p_FaxFrom, string p_Message, string p_Format,
            AsynchronousResponseHandler p_asynchronousResponseHandler)
            : this()
        {
            ConnectionInfo = p_connectionInfo;
            FaxNumber = p_FaxNumber;
            FaxFrom = p_FaxFrom;
            Message = p_Message;
            Format = p_Format;
            AsyncResponseHandler = p_asynchronousResponseHandler;
        }
    }
}