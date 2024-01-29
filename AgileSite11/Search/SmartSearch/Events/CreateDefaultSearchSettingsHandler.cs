using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Handler for event raised when default search field settings is being created.
    /// </summary>
    /// <seealso cref="SearchHelper.CreateDefaultSearchSettings"/>
    public class CreateDefaultSearchSettingsHandler : SimpleHandler<CreateDefaultSearchSettingsHandler, CreateDefaultSearchSettingsEventArgs>
    {
    }
}
