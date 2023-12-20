using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Event arguments for the <see cref="LinksHandler"/>
    /// </summary>
    public class LinksEventArgs : CMSEventArgs
    {        
        /// <summary>
        /// Issue containing the link.
        /// </summary>
        public IssueInfo IssueInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Newsletter the <see cref="LinksEventArgs.IssueInfo"/> is related to.
        /// </summary>
        public NewsletterInfo NewsletterInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of additional parameters. Most of the parameters are obtained from the query string.
        /// </summary>
        public NameValueCollection AdditionalParameters
        {
            get;
            set;
        }
    }
}
