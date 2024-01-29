using System.Data;

using CMS.DataEngine;
using CMS.Membership;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document authorization handler
    /// </summary>
    public class DocumentAuthorizationHandler : SimpleHandler<DocumentAuthorizationHandler, DocumentAuthorizationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="node">Document node</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="authorized">Authorization result</param>
        public DocumentAuthorizationEventArgs StartEvent(UserInfo userInfo, TreeNode node, string permissionName, ref AuthorizationResultEnum authorized)
        {
            var e = new DocumentAuthorizationEventArgs
                {
                    User = userInfo,
                    Document = node,
                    PermissionName = permissionName,
                    AuthorizationResult = authorized
                };

            StartEvent(e);

            authorized = e.AuthorizationResult;

            return e;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="ds">DataSet with the data</param>
        /// <param name="permissionName">Permission name</param>
        public DocumentAuthorizationEventArgs StartEvent(UserInfo userInfo, ref DataSet ds, string permissionName)
        {
            var e = new DocumentAuthorizationEventArgs
                {
                    User = userInfo,
                    Data = ds,
                    PermissionName = permissionName
                };

            StartEvent(e);

            ds = e.Data;

            return e;
        }
    }
}