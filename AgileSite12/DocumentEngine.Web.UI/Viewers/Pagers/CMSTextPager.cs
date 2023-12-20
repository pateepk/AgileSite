using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Text-pager control.
    /// </summary>
    [ToolboxData("<{0}:CMSTextPager runat=server></{0}:CMSTextPager>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    [ParseChildren(true)]
    [PersistChildren(true)]
    public class CMSTextPager : CMSWebControl, INamingContainer
    {
        #region "Variables"

        // Indicates whether init call was fired (due to dynamically added control to the control collection after Init phase)
        bool defaultLoadCalled = false;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Text source property.
        /// </summary>
        protected string mTextSource = null;

        /// <summary>
        /// Number of chars on a page. If it is -1, the text source is not split.
        /// </summary>
        protected int mPageSize = -1;

        /// <summary>
        /// If true, the plain text is processed as decoded.
        /// </summary>
        protected bool mProcessPlainTextDecoded = true;

        /// <summary>
        /// DataPager variable.
        /// </summary>
        protected DataPager mDataPager = null;

        /// <summary>
        /// Paged text source.
        /// </summary>
        protected string[] pagedSource = null;

        /// <summary>
        /// Content literal.
        /// </summary>
        protected Literal ltlContent = new Literal();

        /// <summary>
        /// Content panel.
        /// </summary>
        protected Panel pnlContent = new Panel();

        /// <summary>
        /// Separator literal.
        /// </summary>
        protected Literal ltlSeparator = new Literal();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether dynamic controls should be resolved
        /// </summary>
        public bool ResolveDynamicControls
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ResolveDynamicControls"], true);
            }
            set
            {
                ViewState["ResolveDynamicControls"] = value;
            }
        }


        /// <summary>
        /// Text source.
        /// </summary>
        public string TextSource
        {
            get
            {
                return mTextSource;
            }
            set
            {
                mTextSource = ValidationHelper.GetString(value, "");
                if (!DataHelper.IsEmpty(mTextSource))
                {
                    // Split the pages
                    pagedSource = TextHelper.SplitPages(mTextSource, PageSize, mProcessPlainTextDecoded);

                    PagerControl.DataSource = GetDataSet(pagedSource);
                    PagerControl.PageSize = 1;
                }
            }
        }


        /// <summary>
        /// Number of chars on a page. If it is -1, the text source is not split.
        /// </summary>
        [Category("Behavior"), DefaultValue(-1), Description("Number of chars on a page. If it is -1, the text source will not be split.")]
        public int PageSize
        {
            get
            {
                return mPageSize;
            }
            set
            {
                mPageSize = ValidationHelper.GetInteger(value, -1);
                if (!DataHelper.IsEmpty(TextSource))
                {
                    pagedSource = TextHelper.SplitPages(TextSource, mPageSize, mProcessPlainTextDecoded);

                    // Set pager datasource to determine number of pages
                    PagerControl.DataSource = GetDataSet(pagedSource);
                    PagerControl.PageSize = 1;
                }
            }
        }


        /// <summary>
        /// Represents pager control.
        /// </summary>
        [Category("Data Pager"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.Attribute), NotifyParentProperty(true), RefreshProperties(RefreshProperties.Repaint)]
        public DataPager PagerControl
        {
            get
            {
                if (mDataPager == null)
                {
                    mDataPager = new DataPager();
                    mDataPager.ID = "pager";
                    mDataPager.PageSize = 1;
                    mDataPager.PagingMode = PagingModeTypeEnum.PostBack;
                    mDataPager.OnPageChange += mDataPager_OnPageChange;

                    Controls.AddAt(0, PagerControl);
                }
                if (Page != null)
                {
                    mDataPager.Page = Page;
                }
                return mDataPager;
            }
            set
            {
                mDataPager = value;
            }
        }


        /// <summary>
        /// Text CSS class.
        /// </summary>
        [Category("Behavior"), Description("Text CSS class")]
        public string TextCSSClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["TextCssClass"], "");
            }
            set
            {
                ViewState["TextCssClass"] = value;
            }
        }


        /// <summary>
        /// Pager CSS class.
        /// </summary>
        [Category("Behavior"), Description("Pager CSS class")]
        public string PagerCSSClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["PagerCssClass"], "");
            }
            set
            {
                ViewState["PagerCssClass"] = value;
            }
        }


        /// <summary>
        /// Pager separator (HTML code to insert between the paged text and the pager).
        /// </summary>
        [Category("Behavior"), Description("Pager separator")]
        public string PagerSeparator
        {
            get
            {
                return ValidationHelper.GetString(ViewState["PagerSeparator"], "");
            }
            set
            {
                ViewState["PagerSeparator"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Create child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            pnlContent = new Panel();
            pnlContent.ID = "pnlContent";
            Controls.Add(pnlContent);

            ltlContent = new Literal();
            ltlContent.ID = "ltlContent";
            pnlContent.Controls.Add(ltlContent);

            ltlSeparator = new Literal();
            ltlSeparator.ID = "ltlSeparator";
            Controls.Add(ltlSeparator);

            base.CreateChildControls();
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
            Page.InitComplete += Page_InitComplete;
        }


        /// <summary>
        /// Init complete event handler
        /// </summary>
        void Page_InitComplete(object sender, EventArgs e)
        {
            defaultLoadCalled = true;
            ReloadData(false);
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (!defaultLoadCalled)
            {
                ReloadData(false);
            }
            base.OnLoad(e);
        }


        private void mDataPager_OnPageChange(object sender, EventArgs e)
        {
            ReloadData(true);
        }


        /// <summary>
        /// Reloads current data.
        /// </summary>
        /// <param name="forceReload">Force reload</param>
        public void ReloadData(bool forceReload)
        {
            ltlContent.Text = "";

            // Setup classes
            if (PagerCSSClass != "")
            {
                PagerControl.ControlCssClass = PagerCSSClass;
            }
            if (TextCSSClass != "")
            {
                pnlContent.CssClass = TextCSSClass;
            }

            pnlContent.Controls.Clear();
            // Setup page text
            if (pagedSource != null)
            {
                if (pagedSource.Length >= PagerControl.CurrentPage)
                {
                    ltlContent.Text = pagedSource[PagerControl.CurrentPage - 1];
                    pnlContent.Controls.Add(ltlContent);

                    if (ResolveDynamicControls)
                    {
                        ControlsHelper.ResolveDynamicControls(pnlContent);
                    }
                }
            }

            // Setup the separator
            if (PagerSeparator != "")
            {
                ltlSeparator.Text = PagerSeparator;
            }
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[CMSRepeater: " + ID + "]");
                return;
            }

            if (pagedSource != null)
            {
                // Top DataPager 
                if ((PagerControl.PagerPosition == PagingPlaceTypeEnum.Top) && (PageSize > 0))
                {
                    PagerControl.RenderControl(output);

                    ltlSeparator.RenderControl(output);
                }

                var isVisible = PagerControl.Visible;
                PagerControl.Visible = false;

                pnlContent.RenderControl(output);

                PagerControl.Visible = isVisible;

                // Down DataPager
                if ((PagerControl.PagerPosition == PagingPlaceTypeEnum.Bottom) && (PageSize > 0))
                {
                    ltlSeparator.RenderControl(output);

                    PagerControl.RenderControl(output);
                }
            }
            else
            {
                PagerControl.Visible = false;
            }
        }


        /// <summary>
        /// Gets dataset from string array.
        /// </summary>
        /// <param name="source">String array</param>
        protected DataSet GetDataSet(string[] source)
        {
            // Create dataset
            DataSet ds = new DataSet();
            // Create a DataTable 
            DataTable table = new DataTable();

            // Create a DataColumn and set various properties 
            DataColumn column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.AllowDBNull = false;
            column.ColumnName = "Text";

            // Add the column to the table 
            table.Columns.Add(column);

            // Add rows and set values 
            foreach (string s in source)
            {
                var row = table.NewRow();
                row["Text"] = s;

                table.Rows.Add(row);
            }

            ds.Tables.Add(table);

            return ds;
        }

        #endregion
    }
}