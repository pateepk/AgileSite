using System;

namespace WTE.Communication
{
    /// <summary>
    /// Class for storing email address information.
    /// </summary>
    public class Address
    {
        private string m_email = string.Empty;
        private string m_name = string.Empty;

        /// <summary>
        /// Email associated with address
        /// </summary>
        public String EMail
        {
            get { return m_email; }
            set { m_email = value; }
        }

        /// <summary>
        /// friendly name to associated with email
        /// optional
        /// </summary>
        public String Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Construct an empty address
        /// </summary>
        public Address()
        {
        }

        /// <summary>
        /// Create from an email address
        /// </summary>
        /// <param name="p_address"></param>
        public Address(string p_address)
            : this()
        {
            EMail = p_address;
        }

        /// <summary>
        /// Create from an email address and display name
        /// </summary>
        /// <param name="p_address"></param>
        /// <param name="p_name"></param>
        public Address(string p_address, string p_name)
            : this()
        {
            EMail = p_address;
            Name = p_name;
        }

        /// <summary>
        /// Clone address
        /// </summary>
        /// <param name="orig"></param>
        public Address(Address orig) : this()
        {
            EMail = orig.EMail;
            Name = orig.Name;
        }
    }
}