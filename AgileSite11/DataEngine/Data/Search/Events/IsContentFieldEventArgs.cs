using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Argument of event represented by <see cref="IsContentFieldHandler"/>.
    /// </summary>
    public class IsContentFieldEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Indicates whether field is considered a content field. Values of content fields are aggregated into a designated index column.
        /// </summary>
        public bool Result
        {
            get;
            set;
        }


        /// <summary>
        /// Index for which to indicate whether field represented by <see cref="SearchSettings"/> is considered a content field, or null to perform index agnostic detection (i.e. whether field is considered a content field in any index type).
        /// </summary>
        public ISearchIndexInfo Index
        {
            get;
            set;
        }


        /// <summary>
        /// Search settings representing field for which to detect whether it is considered a content field.
        /// </summary>
        public SearchSettingsInfo SearchSettings
        {
            get;
            set;
        }
    }
}
