using System;

using CMS.Helpers;

namespace CMS.UIControls
{

    /// <summary>
    /// Represents a state of the UniGrid filter field, i.e., the state of all filter controls.
    /// </summary>
    [Serializable]
    internal sealed class UniGridFilterFieldState
    {

        #region "Properties"

        /// <summary>
        /// Gets or sets the name of the UniGrid filter field.
        /// </summary>
        public string Name { get; set; }

        
        /// <summary>
        /// Gets or sets the state of filter controls.
        /// </summary>
        public FilterState State { get; set; }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the UniGridFilterFieldState class.
        /// </summary>
        public UniGridFilterFieldState()
        {
            State = new FilterState();
        }

        #endregion

    }

}