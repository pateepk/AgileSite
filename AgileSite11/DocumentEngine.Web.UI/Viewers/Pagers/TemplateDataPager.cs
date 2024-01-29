using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Template data pager.
    /// </summary>
    [ToolboxData("<{0}:TemplateDataPager runat=server></{0}:TemplateDataPager>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class TemplateDataPager : BasicDataPager
    {
        #region "Private variables"

        private ITemplate mNumberTemplate;
        private ITemplate mSelectedNumberTemplate;
        private ITemplate mFirstItemTemplate;
        private ITemplate mLastItemTemplate;
        private ITemplate mPreviousItemTemplate;
        private ITemplate mNextItemTemplate;
        private ITemplate mSeparatorTemplate;

        private readonly Repeater mRepNumbers = new Repeater();
        private readonly Repeater mRepFirst = new Repeater();
        private readonly Repeater mRepLast = new Repeater();
        private readonly Repeater mRepNext = new Repeater();
        private readonly Repeater mRepPrev = new Repeater();

        #endregion


        #region "Template properties"

        /// <summary>
        /// Number template.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate SeparatorTemplate
        {
            get
            {
                return mSeparatorTemplate;
            }
            set
            {
                mSeparatorTemplate = value;
            }
        }


        /// <summary>
        /// Number template.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate NumberTemplate
        {
            get
            {
                return mNumberTemplate;
            }
            set
            {
                mNumberTemplate = value;
            }
        }


        /// <summary>
        /// First item template.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate FirstItemTemplate
        {
            get
            {
                return mFirstItemTemplate;
            }
            set
            {
                mFirstItemTemplate = value;
            }
        }


        /// <summary>
        /// Last item tamplate.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate LastItemTemplate
        {
            get
            {
                return mLastItemTemplate;
            }
            set
            {
                mLastItemTemplate = value;
            }
        }


        /// <summary>
        /// Previous item template.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate PreviousItemTemplate
        {
            get
            {
                return mPreviousItemTemplate;
            }
            set
            {
                mPreviousItemTemplate = value;
            }
        }


        /// <summary>
        /// Next item template.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate NextItemTemplate
        {
            get
            {
                return mNextItemTemplate;
            }
            set
            {
                mNextItemTemplate = value;
            }
        }


        /// <summary>
        /// Selected number template.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate SelectedNumberTemplate
        {
            get
            {
                return mSelectedNumberTemplate;
            }
            set
            {
                mSelectedNumberTemplate = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Number repeater.
        /// </summary>
        public Repeater NumberRepeater
        {
            get
            {
                return mRepNumbers;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Add(mRepFirst);
            Controls.Add(mRepPrev);
            Controls.Add(mRepNumbers);
            mRepNumbers.ItemDataBound += rep_ItemDataBound;
            Controls.Add(mRepNext);
            Controls.Add(mRepLast);
        }


        /// <summary>
        /// Item databound, use selected number template for selected number if template is defined.
        /// </summary>
        private void rep_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType != ListItemType.Separator) && (e.Item.ItemIndex + 1 == CurrentPage) && (mSelectedNumberTemplate != null))
            {
                e.Item.Controls.Clear();

                DataTable dt = new DataTable();
                dt.Columns.Add("PageNumber", typeof(int));
                DataRow dr = dt.NewRow();
                if ((DataRowView)e.Item.DataItem != null)
                {
                    dr["PageNumber"] = ((DataRowView)e.Item.DataItem).Row["PageNumber"];
                }
                dt.Rows.Add(dr);

                Repeater selRep = new Repeater();
                selRep.DataSource = dt;
                e.Item.Controls.Add(selRep);
                selRep.ItemTemplate = SelectedNumberTemplate;
                selRep.DataBind();
            }
        }


        /// <summary>
        /// Reloads data for pager with current settings.
        /// </summary>
        public void ReloadData(bool forceLoad)
        {
            // First item
            if (mFirstItemTemplate != null)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("PageNumber", typeof(int));
                DataRow dr = dt.NewRow();
                dr["PageNumber"] = 1;
                dt.Rows.Add(dr);
                mRepFirst.ItemTemplate = mFirstItemTemplate;
                mRepFirst.DataSource = dt;
                mRepFirst.DataBind();
            }

            // Previous item
            if (mPreviousItemTemplate != null)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("PageNumber", typeof(int));
                DataRow dr = dt.NewRow();
                dr["PageNumber"] = CurrentPage - 1;
                dt.Rows.Add(dr);
                mRepPrev.ItemTemplate = mPreviousItemTemplate;
                mRepPrev.DataSource = dt;
                mRepPrev.DataBind();
            }

            // Numbers
            if (mNumberTemplate != null)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("PageNumber", typeof(int));

                for (int i = 0; i <= PageCount - 1; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["PageNumber"] = i + 1;
                    dt.Rows.Add(dr);
                }

                mRepNumbers.ItemTemplate = mNumberTemplate;
                mRepNumbers.DataSource = dt;

                if (mSeparatorTemplate != null)
                {
                    mRepNumbers.SeparatorTemplate = mSeparatorTemplate;
                }

                mRepNumbers.DataBind();
            }

            // Next item
            if (mNextItemTemplate != null)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("PageNumber", typeof(int));
                DataRow dr = dt.NewRow();
                dr["PageNumber"] = CurrentPage + 1;
                dt.Rows.Add(dr);
                mRepNext.ItemTemplate = mNextItemTemplate;
                mRepNext.DataSource = dt;
                mRepNext.DataBind();
            }

            // Last item
            if (mLastItemTemplate != null)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("PageNumber", typeof(int));
                DataRow dr = dt.NewRow();
                dr["PageNumber"] = PageCount;
                dt.Rows.Add(dr);
                mRepLast.ItemTemplate = mLastItemTemplate;
                mRepLast.DataSource = dt;
                mRepLast.DataBind();
            }
        }


        /// <summary>
        /// OnPreRender.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            ReloadData(false);

            base.OnPreRender(e);
        }


        /// <summary>
        /// Render.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write(" [ TemplateDataPager : " + ID + " ]");
                return;
            }

            base.Render(writer);
        }

        #endregion
    }
}