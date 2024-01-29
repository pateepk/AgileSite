using System.Web.Http;
using System.ComponentModel;
using System.Web.Http.Description;

namespace CMS.WebApi
{
    /// <summary>
    /// Base class for all CMS <see cref="ApiController"/>s.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [UseDefaultJsonMediaTypeFormatter]
    public abstract class CMSApiController : ApiController
    {
    }
}
