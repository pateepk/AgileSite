using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Handler for event raised when Azure Search <see cref="Document"/> value is being added.
    /// </summary>
    /// <seealso cref="DocumentFieldCreator"/>
    public class AddDocumentValueHandler : SimpleHandler<AddDocumentValueHandler, AddDocumentValueEventArgs>
    {
    }
}
