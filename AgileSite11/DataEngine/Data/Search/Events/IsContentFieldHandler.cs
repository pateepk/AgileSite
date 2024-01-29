using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Handler for event raised when field is being decided whether it is a content field.
    /// </summary>
    /// <seealso cref="SearchFieldsHelper.IsContentField"/>
    public class IsContentFieldHandler : SimpleHandler<IsContentFieldHandler, IsContentFieldEventArgs>
    {
    }
}
