using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Arguments of event represented by <see cref="CreateDefaultSearchSettingsHandler"/>.
    /// </summary>
    public class CreateDefaultSearchSettingsEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Search settings object being created.
        /// </summary>
        public SearchSettingsInfo SearchSettings
        {
            get;
            set;
        }


        /// <summary>
        /// Name of column the <see cref="SearchSettings"/> is being created for.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Data type of the column.
        /// </summary>
        public Type DataType
        {
            get;
            set;
        }
    }
}
