using ITHit.WebDAV.Server;

namespace CMS.WebDAV
{
    /// <summary>
    /// Gets handler.
    /// </summary>
    internal class GetHandler : IMethodHandler
    {
        #region "Variables"

        private IMethodHandler mOriginalHandler = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Sets original handler.
        /// </summary>
        public IMethodHandler OriginalHandler
        {
            set
            {
                mOriginalHandler = value;
            }
        }

        #endregion


        #region "IMethodHandler Members"

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom GET handler.
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <param name="response">HTTP response</param>
        /// <param name="item">Hierarchy item</param>
        public void ProcessRequest(Request request, IResponse response, IHierarchyItem item)
        {
            // Attachment
            if (item is AttachmentResource)
            {
                AttachmentResource resource = item as AttachmentResource;
                resource.WriteToStream(response.OutputStream, 0, resource.ContentLength);
            }
            // Meta file
            else if (item is MetaFileResource)
            {
                MetaFileResource resource = item as MetaFileResource;
                resource.WriteToStream(response.OutputStream, 0, resource.ContentLength);
            }
            // Media file
            else if (item is MediaFileResource)
            {
                MediaFileResource resource = item as MediaFileResource;
                resource.WriteToStream(response.OutputStream, 0, resource.ContentLength);
            }
            // Content
            else if (item is ContentResource)
            {
                ContentResource resource = item as ContentResource;
                resource.WriteToStream(response.OutputStream, 0, resource.ContentLength);
            }
        }

        #endregion
    }
}