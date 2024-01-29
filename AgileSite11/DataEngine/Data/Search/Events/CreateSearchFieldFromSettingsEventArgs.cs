using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Arguments of event represented by <see cref="CreateSearchFieldFromSettingsHandler"/>.
    /// </summary>
    public class CreateSearchFieldFromSettingsEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Search field created by the factory.
        /// </summary>
        public ISearchField SearchField
        {
            get;
            set;
        }


        /// <summary>
        /// Search settings for which to create the field.
        /// </summary>
        public SearchSettingsInfo SearchSettings
        {
            get;
            set;
        }
    }
}
