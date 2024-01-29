using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Argument of event represented by <see cref="IsIndexFieldHandler"/>.
    /// </summary>
    public class IsIndexFieldEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Indicates whether field is to be added to search index.
        /// </summary>
        public bool Result
        {
            get;
            set;
        }


        /// <summary>
        /// Index for which to indicate whether field represented by <see cref="SearchSettings"/> is an index field, or null to perform index agnostic detection (i.e. whether field is included in any index type).
        /// </summary>
        public ISearchIndexInfo Index
        {
            get;
            set;
        }


        /// <summary>
        /// Search settings representing field for which to detect whether it is an index field.
        /// </summary>
        public SearchSettingsInfo SearchSettings
        {
            get;
            set;
        }
    }
}
