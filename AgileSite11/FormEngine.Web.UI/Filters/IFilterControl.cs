using System.Web.UI;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Action event handler.
    /// </summary>
    public delegate void ActionEventHandler();


    /// <summary>
    /// Interface for a general filter control
    /// </summary>
    public interface IFilterControl
    {
        /// <summary>
        /// Unique control ID
        /// </summary>
        string UniqueID
        {
            get;
        }


        /// <summary>
        /// Control which is being filtered by this control
        /// </summary>
        Control FilteredControl
        {
            get;
            set;
        }


        /// <summary>
        /// Filter value
        /// </summary>
        object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the where condition for the filter
        /// </summary>
        string WhereCondition
        {
            get;
        }


        /// <summary>
        /// Raises when the filter changes its state
        /// </summary>
        event ActionEventHandler OnFilterChanged;


        /// <summary>
        /// Resets the filter state
        /// </summary>
        void ResetFilter();


        /// <summary>
        /// Stores filter state to the specified object.
        /// </summary>
        /// <param name="state">The object that holds the filter state.</param>
        void StoreFilterState(FilterState state);


        /// <summary>
        /// Restores filter state from the specified object.
        /// </summary>
        /// <param name="state">The object that holds the filter state.</param>
        void RestoreFilterState(FilterState state);
    }
}