using System;

namespace WTE.Communication
{
    /// <summary>
    /// represents the connection information
    /// </summary>
    public class ConnectionInfo
    {
        private String m_url = String.Empty;
        private String m_login = String.Empty;
        private String m_password = String.Empty;

        /// <summary>
        /// Connection URL
        /// </summary>
        public String URL
        {
            set { m_url = value; }
            get { return m_url; }
        }

        /// <summary>
        /// Connection Login
        /// </summary>
        public String login
        {
            set { m_login = value; }
            get { return m_login; }
        }

        /// <summary>
        /// Connection password
        /// </summary>
        public String password
        {
            set { m_password = value; }
            get { return m_password; }
        }

        /// <summary>
        /// Constructor for Connection Info
        /// </summary>
        /// <param name="p_URL"></param>
        /// <param name="p_login"></param>
        /// <param name="p_password"></param>
        public ConnectionInfo(String p_URL, String p_login, String p_password)
        {
            URL = p_URL;
            login = p_login;
            password = p_password;
        }
    }
}