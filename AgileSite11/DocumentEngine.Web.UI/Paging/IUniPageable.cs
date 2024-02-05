using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// IUniPageable interace.
    /// </summary>
    public interface IUniPageable
    {
        /// <summary>
        /// Gets or sets DataSource.
        /// </summary>
        object PagerDataItem
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the pager control.
        /// </summary>
        UniPager UniPagerControl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the number of result. Enables proceed "fake" datasets, where number 
        /// of results in the dataset is not correspondent to the real number of results
        /// This property must be equal -1 if should be disabled
        /// </summary>
        int PagerForceNumberOfResults
        {
            get;
            set;
        }


        /// <summary>
        /// Occurs when the control bind data.
        /// </summary>
        event EventHandler<EventArgs> OnPageBinding;


        /// <summary>
        /// Occurs when the pager change the page and current mode is postback => reload data
        /// </summary>
        event EventHandler<EventArgs> OnPageChanged;


        /// <summary>
        /// Call request to control re-bind.
        /// </summary>
        void ReBind();
    }
}