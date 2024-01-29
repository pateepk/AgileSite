using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Handler for event raised when search field is being created.
    /// </summary>
    /// <seealso cref="SearchFieldFactory.Create"/>
    public class CreateSearchFieldHandler : SimpleHandler<CreateSearchFieldHandler, CreateSearchFieldEventArgs>
    {
    }
}
