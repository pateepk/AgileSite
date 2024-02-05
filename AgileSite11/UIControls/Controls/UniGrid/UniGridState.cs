using System;
using System.Collections.Generic;

namespace CMS.UIControls
{

    /// <summary>
    /// Represents a UniGrid state, i.e., filter, page number, page size and sorting order.
    /// </summary>
    [Serializable]
    internal sealed class UniGridState
    {

        #region "Properties"

        /// <summary>
        /// Gets or sets the filter condition.
        /// </summary>
        public string WhereClause { get; set; }
        
        
        /// <summary>
        /// Gets or sets the sorting order.
        /// </summary>
        public string SortDirect { get; set; }
        
        
        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        
        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int CurrentPage { get; set; }


        /// <summary>
        /// Gets or sets the value indicating whether the filter is specified.
        /// </summary>
        public bool FilterIsSet { get; set; }

        
        /// <summary>
        /// Gets or sets a list of individual filter field states.
        /// </summary>
        public List<UniGridFilterFieldState> FilterFieldStates { get; set; }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the UniGridState class.
        /// </summary>
        public UniGridState()
        {
            FilterFieldStates = new List<UniGridFilterFieldState>();
        }

        #endregion

    }

}