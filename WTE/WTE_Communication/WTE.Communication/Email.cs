using System;
using System.Collections.Generic;

namespace WTE.Communication
{
    /// <summary>
    /// represents an email to send
    /// </summary>
    public class Email
    {
        private ConnectionInfo m_connectionInfo = null;

        private string m_subject = string.Empty;
        private string m_body = string.Empty;
        private bool m_isHTMLEmail = true;
        private Attachment[] m_attachments = null;
        private AsynchronousResponseHandler m_asyncResponseHandler = null;
        private Addresses m_addresses = new Addresses();
        private Object m_additionalData = null;

        /// <summary>
        /// Addresses container
        /// </summary>
        public class Addresses
        {
            private Address m_fromAddress = null;
            private Address[] m_toAddresses = new Address[] { };
            private Address[] m_ccAddresses = new Address[] { };
            private Address[] m_bccAddresses = new Address[] { };
            private Address m_replyToAddress = null;

            public Address FromAddress
            {
                get { return m_fromAddress; }
                set { m_fromAddress = value; }
            }

            public Address ReplyToAddress
            {
                get { return m_replyToAddress; }
                set { m_replyToAddress = value; }
            }

            public Address[] ToAddresses
            {
                get { return m_toAddresses; }
                set { m_toAddresses = value; }
            }

            public Address[] CCAddresses
            {
                get { return m_ccAddresses; }
                set { m_ccAddresses = value; }
            }

            public Address[] BCCAddresses
            {
                get { return m_bccAddresses; }
                set { m_bccAddresses = value; }
            }

            public Addresses()
            {
            }

            public Addresses(Addresses orig)
            {
                this.FromAddress = new Address(orig.FromAddress);
                this.ReplyToAddress = new Address(orig.ReplyToAddress);
                this.ToAddresses = CloneAddressArray(orig.ToAddresses);
                this.CCAddresses = CloneAddressArray(orig.CCAddresses);
                this.BCCAddresses = CloneAddressArray(orig.BCCAddresses);
            }

            public Address[] CloneAddressArray(Address[] orig)
            {
                List<Address> outList = new List<Address>();
                foreach (Address addressOrig in orig)
                {
                    outList.Add(new Address(addressOrig));
                }

                return outList.ToArray();
            }
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
        /// Optional attachments array
        /// </summary>
        public Attachment[] Attachments
        {
            get
            {
                return m_attachments;
            }
            set { m_attachments = value; }
        }

        /// <summary>
        /// Access to email addresss associated with this email
        /// </summary>
        public Addresses EmailAddresses
        {
            get { return m_addresses; }
            set { m_addresses = value; }
        }

        /// <summary>
        /// Any additional data
        /// </summary>
        public Object AdditionalData
        {
            get
            {
                return m_additionalData;
            }
            set
            {
                m_additionalData = value;
            }
        }

        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject
        {
            get { return m_subject; }
            set { m_subject = value; }
        }

        /// <summary>
        /// Email body
        /// </summary>
        public string Body
        {
            get { return m_body; }
            set { m_body = value; }
        }

        /// <summary>
        /// Is the body content in HTML format or plain text
        /// </summary>
        public bool IsHtmlEmail
        {
            get { return m_isHTMLEmail; }
            set { m_isHTMLEmail = value; }
        }

        /// <summary>
        /// Optional AsyncResponseHandler which can be called after email has been sent to communication server
        /// </summary>
        public AsynchronousResponseHandler AsyncResponseHandler
        {
            get { return m_asyncResponseHandler; }
            set { m_asyncResponseHandler = value; }
        }

        /// <summary>
        /// Construct email
        /// </summary>
        /// <param name="p_connectionInfo"></param>
        /// <param name="p_addresses"></param>
        /// <param name="p_subject"></param>
        /// <param name="p_body"></param>
        /// <param name="p_isHTMLEmail"></param>
        public Email(ConnectionInfo p_connectionInfo,
            Addresses p_addresses,
            string p_subject, string p_body, bool p_isHTMLEmail)
            : this(p_connectionInfo,
                p_addresses,
                p_subject, p_body, p_isHTMLEmail, null)
        {
        }

        /// <summary>
        /// Constuct Email
        /// </summary>
        /// <param name="p_connectionInfo"></param>
        /// <param name="p_addresses"></param>
        /// <param name="p_subject"></param>
        /// <param name="p_body"></param>
        /// <param name="p_isHTMLEmail"></param>
        /// <param name="p_attachments"></param>
        public Email(ConnectionInfo p_connectionInfo,
            Addresses p_addresses,
            string p_subject, string p_body, bool p_isHTMLEmail, Attachment[] p_attachments)
            : this(p_connectionInfo,
                p_addresses,
                p_subject, p_body, p_isHTMLEmail, p_attachments, null)
        {
        }

        /// <summary>
        /// Construct Email
        /// </summary>
        /// <param name="p_connectionInfo"></param>
        /// <param name="p_addresses"></param>
        /// <param name="p_subject"></param>
        /// <param name="p_body"></param>
        /// <param name="p_isHTMLEmail"></param>
        /// <param name="p_attachments"></param>
        /// <param name="p_asynchronousResponseHandler"></param>
        public Email(ConnectionInfo p_connectionInfo,
            Addresses p_addresses,
            string p_subject, string p_body, bool p_isHTMLEmail, Attachment[] p_attachments,
            AsynchronousResponseHandler p_asynchronousResponseHandler)
        {
            ConnectionInfo = p_connectionInfo;
            EmailAddresses = p_addresses;
            Subject = p_subject;
            Body = p_body;
            IsHtmlEmail = p_isHTMLEmail;
            Attachments = p_attachments;
            AsyncResponseHandler = p_asynchronousResponseHandler;
        }
    }
}