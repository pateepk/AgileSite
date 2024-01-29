using ITHit.WebDAV.Server;

namespace CMS.WebDAV
{
    /// <summary>
    /// PUT handler.
    /// </summary>
    internal class PutHandler : IMethodHandler
    {
        #region "Variables"

        private IMethodHandler mOriginalHandler = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Sets original PUT handler.
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
        /// Enables processing of HTTP Web requests by a custom PUT handler. 
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
                resource.SaveFromStream(request.InputStream, resource.ContentType);
            }
            // Meta file
            else if (item is MetaFileResource)
            {
                MetaFileResource resource = item as MetaFileResource;
                resource.SaveFromStream(request.InputStream, resource.ContentType);
            }
            // Media file
            else if (item is MediaFileResource)
            {
                MediaFileResource resource = item as MediaFileResource;
                resource.SaveFromStream(request.InputStream, resource.ContentType);
            }
            // Content
            else if (item is ContentResource)
            {
                ContentResource resource = item as ContentResource;
                resource.SaveFromStream(request.InputStream, resource.ContentType);
            }
            else
            {
                mOriginalHandler.ProcessRequest(request, response, item);
            }
        }

        #endregion
    }
}