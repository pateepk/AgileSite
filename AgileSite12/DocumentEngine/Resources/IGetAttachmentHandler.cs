using System.Web;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents the contract that Kentico requires when processing HTTP requests for attachments.
    /// </summary>
    public interface IGetAttachmentHandler : IHttpHandler
    {
    }
}
