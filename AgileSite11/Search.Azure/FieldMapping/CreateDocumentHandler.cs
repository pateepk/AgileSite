using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

using Microsoft.Azure.Search.Models;
using CMS.DataEngine;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Handler for event raised when Azure Search <see cref="Document"/> is being created from a <see cref="SearchDocument"/>.
    /// </summary>
    /// <seealso cref="DocumentCreator"/>
    public class CreateDocumentHandler : AdvancedHandler<CreateDocumentHandler, CreateDocumentEventArgs>
    {
    }
}
