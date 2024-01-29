using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
//using CMS.Controls;
//using CMS.ExtendedControls;
//using CMS.GlobalHelper;
//using CMS.PortalControls;
//using CMS.SettingsProvider;
using System.Text;
using System.Data;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI;


public partial class CMSWebParts_CUWebParts_CUBasicUniView : CMSAbstractWebPart
{
    #region "Variables"

    // Base datasource instance
    private CMSBaseDataSource mDataSourceControl = null;
    // Indicates whether control was binded
    private bool binded = false;
    // BasicRepeter instance
    private BasicUniView basicUniView = new BasicUniView();
    // Indicates whether current control was added to the filter collection
    private bool mFilterControlAdded = false;

    #endregion


    #region "Public properties"

    /// <summary>
    /// Gets or sets name of source.
    /// </summary>
    public string DataSourceName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("DataSourceName"), String.Empty);
        }
        set
        {
            SetValue("DataSourceName", value);
        }
    }


    /// <summary>
    /// Control with data source.
    /// </summary>
    public CMSBaseDataSource DataSourceControl
    {
        get
        {
            // Check if control is empty and load it with the data
            if (mDataSourceControl == null)
            {
                if (!String.IsNullOrEmpty(DataSourceName))
                {
                    mDataSourceControl = CMSControlsHelper.GetFilter(DataSourceName) as CMSBaseDataSource;
                }
            }

            return mDataSourceControl;
        }
        set
        {
            mDataSourceControl = value;
        }
    }


    /// <summary>
    /// Gets or sets FirstItemTransformationName property.
    /// </summary>
    public string FirstItemTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FirstItemTransformationName"), "");
        }
        set
        {
            SetValue("FirstItemTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets LastItemTransformationName property.
    /// </summary>
    public string LastItemTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("LastItemTransformationName"), "");
        }
        set
        {
            SetValue("LastItemTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets AlternatingItemTemplate property.
    /// </summary>
    public string AlternatingItemTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AlternatingItemTransformationName"), "");
        }
        set
        {
            SetValue("AlternatingItemTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets FooterTemplate property.
    /// </summary>
    public string FooterTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FooterTransformationName"), "");
        }
        set
        {
            SetValue("FooterTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets HeaderTemplate property.
    /// </summary>
    public string HeaderTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("HeaderTransformationName"), "");
        }
        set
        {
            SetValue("HeaderTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets ItemTemplate property.
    /// </summary>
    public string TransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("TransformationName"), "");
        }
        set
        {
            SetValue("TransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets SeparatorTemplate property.
    /// </summary>
    public string SeparatorTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SeparatorTransformationName"), "");
        }
        set
        {
            SetValue("SeparatorTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets HideControlForZeroRows property.
    /// </summary>
    public bool HideControlForZeroRows
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("HideControlForZeroRows"), true);
        }
        set
        {
            SetValue("HideControlForZeroRows", value);
        }
    }


    /// <summary>
    /// Gets or sets ZeroRowsText property.
    /// </summary>
    public string ZeroRowsText
    {
        get
        {
            return ValidationHelper.GetString(GetValue("ZeroRowsText"), "");
        }
        set
        {
            SetValue("ZeroRowsText", value);
        }
    }

    /// <summary>
    /// Gets or sets the value that indicates whether sorting is ascending at default.
    /// </summary>
    public bool SortAscending
    {
        get
        {
            return (ValidationHelper.GetBoolean(GetValue("SortAscending"), false));
        }
        set
        {
            SetValue("SortAscending", value);
        }
    }


    /// <summary>
    /// Gets or sets the value that indicates whether sorting is allowed.
    /// </summary>
    public bool AllowSorting
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("AllowSorting"), false);
        }
        set
        {
            SetValue("AllowSorting", value);
        }
    }


    /// <summary>
    /// Gets or sets the value that inidcates whether sorting process is proceeded in the code.
    /// </summary>
    public bool ProcessSorting
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("ProcessSorting"), false);
        }
        set
        {
            SetValue("ProcessSorting", value);
        }
    }


    /// <summary>
    /// Gets or sets the default sort field.
    /// </summary>
    public string SortField
    {
        get
        {
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(GetValue("SortField"), ""), "");
        }
        set
        {
            SetValue("SortField", value);
        }
    }

    /// <summary>
    /// Gets or sets FooterTemplate for selected item.
    /// </summary>
    public string SelectedItemFooterTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SelectedItemFooterTransformationName"), "");
        }
        set
        {
            SetValue("SelectedItemFooterTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets HeaderTemplate for selected item.
    /// </summary>
    public string SelectedItemHeaderTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SelectedItemHeaderTransformationName"), "");
        }
        set
        {
            SetValue("SelectedItemHeaderTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets ItemTemplate for selected item.
    /// </summary>
    public string SelectedItemTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SelectedItemTransformationName"), "");
        }
        set
        {
            SetValue("SelectedItemTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets ItemTemplate for single item.
    /// </summary>
    public string SingleItemTransformationName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SingleItemTransformationName"), String.Empty);
        }
        set
        {
            SetValue("SingleItemTransformationName", value);
        }
    }


    /// <summary>
    /// Gets or sets the value that indicates whether header and footer items should be hidden if single item is displayed.
    /// </summary>
    public bool HideHeaderAndFooterForSingleItem
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("HideHeaderAndFooterForSingleItem"), basicUniView.HideHeaderAndFooterForSingleItem);
        }
        set
        {
            SetValue("HideHeaderAndFooterForSingleItem", value);
            basicUniView.HideHeaderAndFooterForSingleItem = value;
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// On content loaded override.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (StopProcessing)
        {
            // Do nothing
        }
        else
        {
            // Set properties
            if (!String.IsNullOrEmpty(ZeroRowsText))
            {
                basicUniView.ZeroRowsText = ZeroRowsText;
            }

            basicUniView.HideControlForZeroRows = HideControlForZeroRows;
            basicUniView.HideHeaderAndFooterForSingleItem = HideHeaderAndFooterForSingleItem;
            basicUniView.DataBindByDefault = false;
            basicUniView.OnPageChanged += new EventHandler<EventArgs>(BasicRepeater_OnPageChanged);

            EnsureFilterControl();
        }
    }


    /// <summary>
    /// Ensures current control in the filters collection.
    /// </summary>
    protected void EnsureFilterControl()
    {
        if (!mFilterControlAdded)
        {
            // Add basic repeater to the filter collection
            CMSControlsHelper.SetFilter(ValidationHelper.GetString(GetValue("WebPartControlID"), ClientID), basicUniView);
            mFilterControlAdded = true;
        }
    }

    private Control FindControlRecursive(Control Root, string Id)
    {
        if (Root.ClientID == Id)
        {
            return Root;
        }

        foreach (Control Ctl in Root.Controls)
        {
            Control FoundCtl = FindControlRecursive(Ctl, Id);

            if (FoundCtl != null)
            {
                return FoundCtl;
            }
        }

        return null;
    }

    /// <summary>
    /// OnPageChaged event handler.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">EventArg</param>
    private void BasicRepeater_OnPageChanged(object sender, EventArgs e)
    {
        // Reload data
        if (DataSourceControl != null)
        {
            basicUniView.DataSource = GetDataSource();
            //basicUniView.DataSource = DataSourceControl.DataSource;
            LoadTransformations();
            basicUniView.DataBind();
            binded = true;
        }
    }

    /// <summary>
    /// Loads and setups web part.
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        // Add control to the control collection
        plcBasicUniView.Controls.Add(basicUniView);

        // Indicates whether postback was fired from current control
        bool bindControl = false;

        // Check whether postback was executed from current transformation item
        if (RequestHelper.IsPostBack())
        {
            // Check event target value and callback parameter value
            string eventTarget = ValidationHelper.GetString(Request.Form["__EVENTTARGET"], String.Empty);
            string callbackParam = ValidationHelper.GetString(Request.Form["__CALLBACKPARAM"], String.Empty);
            if (eventTarget.StartsWithCSafe(UniqueID) || callbackParam.StartsWithCSafe(UniqueID) || eventTarget.EndsWithCSafe(ContextMenu.CONTEXT_MENU_SUFFIX))
            {
                bindControl = true;
            }
            // Check whether request key contains some control assigned to current control
            else
            {
                foreach (string key in Request.Form.Keys)
                {
                    if ((key != null) && key.StartsWithCSafe(UniqueID))
                    {
                        bindControl = true;
                        break;
                    }
                }
            }

            if (bindControl)
            {
                // Reload data
                if (DataSourceControl != null)
                {
                    basicUniView.DataSource = GetDataSource();
                    //basicUniView.DataSource = DataSourceControl.DataSource;
                    LoadTransformations();
                    basicUniView.DataBind();
                    binded = true;
                }
            }
        }

        base.OnLoad(e);

        // Handle filter change event
        if (DataSourceControl != null)
        {
            DataSourceControl.OnFilterChanged += DataSourceControl_OnFilterChanged;
        }

        if (RequestHelper.IsPostBack())
        {
            string targetName = Request.Params.Get("__EVENTTARGET");

            if (!string.IsNullOrWhiteSpace(targetName))
            {
                targetName = targetName.Replace("$", "_");
                LinkButton lb = FindControlRecursive(Page, targetName) as LinkButton;
                if (lb != null)
                {
                    string targetUniView = lb.Attributes["UniView"];
                    if (string.IsNullOrWhiteSpace(targetUniView) || targetUniView == this.WebPartID)
                    {
                        switch (lb.CommandName.ToLower())
                        {
                            case "sort":
                                string cuSortColumn = lb.CommandArgument;
                                if (!string.IsNullOrWhiteSpace(cuSortColumn))
                                {
                                    cuSortColumn = cuSortColumn.ToLower();
                                    SetSort(cuSortColumn);
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    public string SortExpression
    {
        get
        {
            string ret = string.Empty;
            object o = ViewState["SortExpression"];
            if (o != null)
            {
                ret = Convert.ToString(o);
            }
            return ret;
        }
        set
        {
            ViewState["SortExpression"] = value;
        }
    }

    protected void SetSort(string p_columnName)
    {
        string lcColumnName = p_columnName.ToLower();
        if (SortExpression.StartsWith(lcColumnName))
        {
            if (SortExpression.EndsWith("desc"))
            {
                SortExpression = lcColumnName + " asc";
            }
            else
            {
                SortExpression = lcColumnName + " desc";
            }
        }
        else
        {
            SortExpression = lcColumnName + " desc";
        }

        //if (DataSourceControl != null)
        //{
        //    DataSourceControl.OrderBy = SortExpression;
        //}

        this.DataSourceControl_OnFilterChanged();
    }

    /// <summary>
    /// Load transformations with dependence on datasource type and datasource state.
    /// </summary>
    protected void LoadTransformations()
    {
        CMSBaseDataSource docDataSource = DataSourceControl as CMSBaseDataSource;
        if ((docDataSource != null) && (docDataSource.IsSelected) && (!String.IsNullOrEmpty(SelectedItemTransformationName)))
        {
            basicUniView.ItemTemplate = TransformationHelper.LoadTransformation(this, SelectedItemTransformationName);

            if (!String.IsNullOrEmpty(SelectedItemFooterTransformationName))
            {
                basicUniView.FooterTemplate = TransformationHelper.LoadTransformation(this, SelectedItemFooterTransformationName);
            }
            else
            {
                basicUniView.FooterTemplate = null;
            }

            if (!String.IsNullOrEmpty(SelectedItemHeaderTransformationName))
            {
                basicUniView.HeaderTemplate = TransformationHelper.LoadTransformation(this, SelectedItemHeaderTransformationName);
            }
            else
            {
                basicUniView.HeaderTemplate = null;
            }
        }
        else
        {
            // Apply transformations if they exist
            if (!String.IsNullOrEmpty(TransformationName))
            {
                basicUniView.ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName);
            }
            if (!String.IsNullOrEmpty(AlternatingItemTransformationName))
            {
                basicUniView.AlternatingItemTemplate = TransformationHelper.LoadTransformation(this, AlternatingItemTransformationName);
            }
            if (!String.IsNullOrEmpty(FooterTransformationName))
            {
                basicUniView.FooterTemplate = TransformationHelper.LoadTransformation(this, FooterTransformationName);
            }
            if (!String.IsNullOrEmpty(HeaderTransformationName))
            {
                basicUniView.HeaderTemplate = TransformationHelper.LoadTransformation(this, HeaderTransformationName);
            }
            if (!String.IsNullOrEmpty(SeparatorTransformationName))
            {
                basicUniView.SeparatorTemplate = TransformationHelper.LoadTransformation(this, SeparatorTransformationName);
            }
            if (!String.IsNullOrEmpty(FirstItemTransformationName))
            {
                basicUniView.FirstItemTemplate = TransformationHelper.LoadTransformation(this, FirstItemTransformationName);
            }
            if (!String.IsNullOrEmpty(LastItemTransformationName))
            {
                basicUniView.LastItemTemplate = TransformationHelper.LoadTransformation(this, LastItemTransformationName);
            }
            if (!String.IsNullOrEmpty(SingleItemTransformationName))
            {
                basicUniView.SingleItemTemplate = TransformationHelper.LoadTransformation(this, SingleItemTransformationName);
            }
        }
    }

    private object GetDataSource()
    {
        DataView dv = null;
        DataSet ds = DataSourceControl.DataSource as DataSet;
        if (ds != null)
        {
            dv = new DataView(ds.Tables[0], "", SortExpression, DataViewRowState.CurrentRows);
        }
        return dv;
    }

    /// <summary>
    /// OnFilter change event handler.
    /// </summary>
    private void DataSourceControl_OnFilterChanged()
    {
        // Set forcibly visibility
        Visible = true;

        // Reload data
        if (DataSourceControl != null && basicUniView != null)
        {
            basicUniView.DataSource = GetDataSource();
            //basicUniView.DataSource = DataSourceControl.DataSource;
            LoadTransformations();
            basicUniView.DataBind();
            binded = true;
        }
    }


    /// <summary>
    /// OnPreRender override.
    /// </summary>
    protected override void OnPreRender(EventArgs e)
    {
        // Datasource data
        object ds = null;

        // Set transformations if data source is not empty
        if (DataSourceControl != null)
        {
            // Get data from datasource
            ds = DataSourceControl.DataSource;

            // Check whether data exist
            if ((!DataHelper.DataSourceIsEmpty(ds)) && (!binded))
            {
                // Initilaize related data if provided
                if (DataSourceControl.RelatedData != null)
                {
                    RelatedData = DataSourceControl.RelatedData;
                }

                basicUniView.DataSource = GetDataSource();
                //basicUniView.DataSource = DataSourceControl.DataSource;
                LoadTransformations();
                basicUniView.DataBind();
            }
        }

        base.OnPreRender(e);

        // Hide control for zero rows
        if (((DataSourceControl == null) || (DataHelper.DataSourceIsEmpty(ds))) && (HideControlForZeroRows))
        {
            Visible = false;
        }
    }


    /// <summary>
    /// Reloads data.
    /// </summary>
    public override void ReloadData()
    {
        SetupControl();
        EnsureFilterControl();
        base.ReloadData();
    }

    #endregion;
}