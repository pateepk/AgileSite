using CMS.Base;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Handler for event raised when Azure Search <see cref="Index"/> is created or updated.
    /// </summary>
    public class CreateOrUpdateIndexHandler : SimpleHandler<CreateOrUpdateIndexHandler, CreateOrUpdateIndexEventArgs>
    {
    }
}
