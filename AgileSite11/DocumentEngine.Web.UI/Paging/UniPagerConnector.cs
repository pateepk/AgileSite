using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control that can be used as connector to UniPager without implementing the IUniPageable interface.
    /// </summary>
    [ToolboxItem(false)]
    public class UniPagerConnector : WebControl, IUniPageable
    {
        #region "Variables"

        /// <summary>
        /// Forced number of results for the pager.
        /// </summary>
        protected int mPagerForceNumberOfResults = 0;

        #endregion


        #region "IUniPageable Members"

        /// <summary>
        /// Pager data item object.
        /// </summary>
        public object PagerDataItem
        {
            get;
            set;
        }


        /// <summary>
        /// Pager control.
        /// </summary>
        public UniPager UniPagerControl
        {
            get;
            set;
        }


        /// <summary>
        /// Occurs when the control bind data.
        /// </summary>
        public event EventHandler<EventArgs> OnPageBinding;


        /// <summary>
        /// Occurs when the pager change the page and current mode is postback => reload data
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;


        /// <summary>
        /// Evokes control databind.
        /// </summary>
        public void ReBind()
        {
            if (OnPageChanged != null)
            {
                OnPageChanged(this, null);
            }
        }


        /// <summary>
        /// Raises the OnPageBinding method.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Argument</param>
        public void RaiseOnPageBinding(object sender, EventArgs e)
        {
            if (OnPageBinding != null)
            {
                OnPageBinding(sender, e);
            }
        }


        /// <summary>
        /// Gets or sets the number of result. Enables proceed "fake" datasets, where number 
        /// of results in the dataset is not correspondent to the real number of results
        /// This property must be equal -1 if should be disabled
        /// </summary>
        public int PagerForceNumberOfResults
        {
            get
            {
                return mPagerForceNumberOfResults;
            }
            set
            {
                mPagerForceNumberOfResults = value;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Render.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[UniPagerConnector: " + ID + "]");
                return;
            }

            base.Render(writer);
        }

        #endregion
    }
}