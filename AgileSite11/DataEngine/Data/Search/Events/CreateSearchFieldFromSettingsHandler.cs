using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Handler for event raised when search field is being created from search settings.
    /// </summary>
    /// <seealso cref="SearchFieldFactory.CreateFromSettings"/>
    public class CreateSearchFieldFromSettingsHandler : SimpleHandler<CreateSearchFieldFromSettingsHandler, CreateSearchFieldFromSettingsEventArgs>
    {
    }
}
