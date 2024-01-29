using System;

using CMS.DocumentEngine.Web.UI;
using CMS.UIControls;

namespace CMS.Synchronization.Web.UI
{
    /// <summary>
    /// Abstract base class for staging tasks filters.
    /// </summary>
    public abstract class StagingTasksFilterBase : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        /// <summary>
        /// Decides what kind of task types will be in filter.
        /// Depends on EnumCategoryAttribute in TaskTypeEnum.
        /// If empty all task types are selected.
        /// </summary>
        public abstract string TaskTypeCategories
        {
            get;
            set;
        }


        /// <summary>
        /// Decides whether to show task group selector.
        /// </summary>
        public abstract bool TaskGroupSelectorEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Filtered Grid.
        /// </summary>
        public UniGrid Grid
        {
            get
            {
                return FilteredControl as UniGrid;
            }
        }

        #endregion


        #region "Life-cycle methods"

        /// <summary>
        /// Decides whether to hide filter or not.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Hide filter button, this filter has its own
            if (Grid != null)
            {
                Grid.HideFilterButton = true;
            }
        }

        #endregion
    }
}
