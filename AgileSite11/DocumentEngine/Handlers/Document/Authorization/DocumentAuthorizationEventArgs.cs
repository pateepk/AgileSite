using System.Data;

using CMS.DataEngine;
using CMS.Membership;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document authorization event arguments
    /// </summary>
    public class DocumentAuthorizationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// User to check
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Data to authorize
        /// </summary>
        public DataSet Data
        {
            get;
            set;
        }


        /// <summary>
        /// Document to authorize
        /// </summary>
        public TreeNode Document
        {
            get;
            set;
        }


        /// <summary>
        /// Permission name
        /// </summary>
        public string PermissionName
        {
            get;
            set;
        }


        /// <summary>
        /// Document authorization result
        /// </summary>
        public AuthorizationResultEnum AuthorizationResult
        {
            get;
            set;
        }
    }
}