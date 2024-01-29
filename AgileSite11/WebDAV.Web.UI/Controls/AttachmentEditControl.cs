using System;

using CMS.Base;
using CMS.UIControls;

namespace CMS.WebDAV.Web.UI
{
    /// <summary>
    /// Attachment WebDAV control.
    /// </summary>
    public class AttachmentEditControl : WebDAVEditControl
    {
        /// <summary>
        /// Creates instance.
        /// </summary>
        public AttachmentEditControl()
        {
            mControlType = FileTypeEnum.Attachment;
        }


        /// <summary>
        /// Gets the attachment URL.
        /// </summary>
        protected override string GetUrl()
        {
            string nodeAliasPath = NodeAliasPath;

            // Get group sub node alias path
            if (GroupNode != null)
            {
                if (nodeAliasPath.StartsWithCSafe(GroupNode.NodeAliasPath, true))
                {
                    nodeAliasPath = nodeAliasPath.Remove(0, GroupNode.NodeAliasPath.Length);
                }
            }

            return WebDAVURLProvider.GetAttachmentWebDAVUrl(SiteName, nodeAliasPath, NodeCultureCode, AttachmentFieldName, FileName, GroupName);
        }
    }
}