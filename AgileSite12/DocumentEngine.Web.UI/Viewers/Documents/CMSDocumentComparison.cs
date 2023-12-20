using System;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Document Comparison control.
    /// </summary>
    [ToolboxData("<{0}:CMSDocumentComparison runat=server></{0}:CMSDocumentComparison>"), Serializable()]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSDocumentComparison : CMSAbstractControlProperties, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// Multi column table variable.
        /// </summary>
        protected BasicMultiColumnTable mMultiColumnTable = null;

        /// <summary>
        /// CMSDropDownList variable.
        /// </summary>
        protected CMSDropDownList mDrpDocumentList = null;

        /// <summary>
        /// Add button.
        /// </summary>
        protected Button mBtnAdd = null;

        /// <summary>
        /// Table params.
        /// </summary>
        protected string[,] mTableParams = null;

        /// <summary>
        /// New display node Id.
        /// </summary>
        protected string newDisplayNodeIDs = "";

        /// <summary>
        /// Basic URL params.
        /// </summary>
        protected string basicURLParams = null;

        /// <summary>
        /// When DataSource is empty NoData  = true.
        /// </summary>
        protected bool NoData = false;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Text to be shown when control havent any data.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Text to be shown when control havent any data.")]
        public virtual string ZeroRowsText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ZeroRowsText"], "");
            }
            set
            {
                ViewState["ZeroRowsText"] = value;
            }
        }


        ///<summary>Drop-down list with all comparable documents.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public CMSDropDownList DocumentList
        {
            get
            {
                return mDrpDocumentList;
            }
            set
            {
                mDrpDocumentList = value;
            }
        }


        ///<summary>Table displaying the documents.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public BasicMultiColumnTable BasicMultiColumnTable
        {
            get
            {
                return mMultiColumnTable;
            }
            set
            {
                mMultiColumnTable = value;
            }
        }


        ///<summary>Add document button.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public Button AddButton
        {
            get
            {
                return mBtnAdd;
            }
            set
            {
                mBtnAdd = value;
            }
        }


        ///<summary>Text of the link for removing all compared documents.</summary>
        [Category("Behavior"), DefaultValue(""), Description("Text of the link for removing all compared documents."), NotifyParentProperty(true)]
        public string RemoveAllText
        {
            get
            {
                if ((ViewState["RemoveAllText"] == null) && (Context != null))
                {
                    ViewState["RemoveAllText"] = ResHelper.GetString("CMSDocumentComparison.RemoveAll");
                }

                return ValidationHelper.GetString(ViewState["RemoveAllText"], "");
            }
            set
            {
                ViewState["RemoveAllText"] = value;
            }
        }


        ///<summary>Text of the link for removing selected document.</summary>
        [Category("Behavior"), DefaultValue(""), Description("Text of the link for removing selected document."), NotifyParentProperty(true)]
        public string RemoveText
        {
            get
            {
                if ((ViewState["RemoveText"] == null) && (Context != null))
                {
                    ViewState["RemoveText"] = ResHelper.GetString("general.remove");
                }
                return ValidationHelper.GetString(ViewState["RemoveText"], "");
            }
            set
            {
                ViewState["RemoveText"] = value;
            }
        }


        ///<summary>Column to be displayed in the drop-down list.</summary>
        [Category("Behavior"), DefaultValue(""), Description("Column to be displayed in the drop-down list."), NotifyParentProperty(true)]
        public string DropDownListColumn
        {
            get
            {
                if (ViewState["DropDownListColumn"] == null)
                {
                    ViewState["DropDownListColumn"] = "";
                }
                return Convert.ToString(ViewState["DropDownListColumn"]);
            }
            set
            {
                ViewState["DropDownListColumn"] = value;
            }
        }


        ///<summary>Params of the table to be displayed. A string array of size (x, 4).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public string[,] TableParams
        {
            get
            {
                return mTableParams;
            }
            set
            {
                mTableParams = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (Context == null)
            {
                return;
            }

            DataRow valueDR;

            DataSet listingDS = TreeProvider.SelectNodes(SiteName, Path, CultureCode, CombineWithDefaultCulture, ClassNames, WhereCondition, OrderBy, MaxRelativeLevel, SelectOnlyPublished);

            if (CheckPermissions)
            {
                UserInfo userInfo = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);
                listingDS = TreeSecurityProvider.FilterDataSetByPermissions(listingDS, NodePermissionsEnum.Read, userInfo);
            }

            //Check if Data was found
            if (DataHelper.DataSourceIsEmpty(listingDS))
            {
                NoData = true;
                return;
            }

            // Add controls
            Controls.Add(mDrpDocumentList);
            Controls.Add(new LiteralControl("&nbsp;"));
            Controls.Add(mBtnAdd);
            Controls.Add(new LiteralControl("<br />&nbsp;<br />"));
            Controls.Add(mMultiColumnTable);


            // fill in the CMSDropDownList
            if (!(RequestHelper.IsPostBack()))
            {
                mDrpDocumentList.Items.Clear();

                if (!DataHelper.DataSourceIsEmpty(listingDS))
                {
                    foreach (DataRow transTemp0 in listingDS.Tables[0].Rows)
                    {
                        valueDR = transTemp0;
                        mDrpDocumentList.Items.Add(new ListItem(Convert.ToString(valueDR[DropDownListColumn]), Convert.ToInt32(valueDR["NodeID"]).ToString()));
                    }
                }
            }

            foreach (string transTemp1 in Context.Request.QueryString.Keys)
            {
                var key = transTemp1;
                if (key != null)
                {
                    if (key.ToLowerCSafe() != "removenodeid" & key.ToLowerCSafe() != "aliaspath")
                    {
                        if (key.ToLowerCSafe() == "displaynodeids")
                        {
                            if ((Context.Request.QueryString["removenodeid"] != null) && (Context.Request.QueryString["removenodeid"] != ""))
                            {
                                // replace removed node 
                                newDisplayNodeIDs = "," + Context.Request.QueryString[key] + ",";
                                newDisplayNodeIDs = newDisplayNodeIDs.Replace("," + Context.Request.QueryString["removenodeid"] + ",", ",");

                                if (newDisplayNodeIDs.StartsWithCSafe(","))
                                {
                                    newDisplayNodeIDs = newDisplayNodeIDs.Substring(1);
                                }
                                if (newDisplayNodeIDs.EndsWithCSafe(","))
                                {
                                    newDisplayNodeIDs = newDisplayNodeIDs.Substring(0, newDisplayNodeIDs.Length - 1);
                                }
                            }
                            else
                            {
                                newDisplayNodeIDs = Context.Request.QueryString["displayNodeIDs"];
                            }
                        }
                        else
                        {
                            basicURLParams += "&" + key + "=" + Context.Request.QueryString[key];
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(newDisplayNodeIDs))
            {
                // get dataset with selected products in the selected order
                var displayNodeIDs = newDisplayNodeIDs.Split(',');
                var tableDS = listingDS.Clone();
                if (displayNodeIDs != null && displayNodeIDs.Length > 0)
                {
                    int rowIndex;
                    for (rowIndex = 0; rowIndex <= displayNodeIDs.GetUpperBound(0); rowIndex++)
                    {
                        if ((displayNodeIDs[rowIndex] != null) && (displayNodeIDs[rowIndex].Trim() != ""))
                        {
                            var newDR = tableDS.Tables[0].NewRow();
                            valueDR = listingDS.Tables[0].Select("nodeid = " + displayNodeIDs[rowIndex])[0];
                            foreach (DataColumn transTemp2 in tableDS.Tables[0].Columns)
                            {
                                var col = transTemp2;
                                newDR[col.ColumnName] = valueDR[col.ColumnName];
                            }
                            tableDS.Tables[0].Rows.Add(newDR);
                        }
                    }
                }
                BasicMultiColumnTable.DataSource = tableDS.Tables[0];
            }
        }


        /// <summary>
        /// Prerender event - set table parameters.
        /// </summary>
        protected void ControlPrerender(object sender, EventArgs e)
        {
            if ((Context == null) || (TableParams == null))
            {
                return;
            }
            string currentPage = Context.Request.Path;

            if (TableParams.GetUpperBound(1) > 0)
            {
                if (!String.IsNullOrEmpty(basicURLParams))
                {
                    TableParams[TableParams.GetUpperBound(0), 0] = @"<a href=""" + currentPage + "?" + basicURLParams.Substring(1) + @""">" + RemoveAllText + "</a>";
                }
                else
                {
                    TableParams[TableParams.GetUpperBound(0), 0] = @"<a href=""" + currentPage + @""">" + RemoveAllText + "</a>";
                }
                TableParams[TableParams.GetUpperBound(0), 1] = "NodeID";
                if (TableParams.GetUpperBound(1) > 1)
                {
                    TableParams[TableParams.GetUpperBound(0), 2] = @"<a href=""" + currentPage + "?removenodeid={%NodeID%}" + basicURLParams + "&amp;displayNodeIDs=" + HttpUtility.UrlPathEncode(newDisplayNodeIDs) + @""">" + RemoveText + "</a>";
                }
                BasicMultiColumnTable.TableParams = TableParams;
            }
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[ CMSDocumentComparison : " + ClientID + " ]");
                return;
            }

            if (!StopProcessing)
            {
                EnableViewState = false;
                if (!NoData)
                {
                    if (Context == null)
                    {
                        mDrpDocumentList.RenderControl(output);
                        output.Write("&nbsp;");
                        mBtnAdd.RenderControl(output);
                    }
                    else
                    {
                        base.Render(output);
                    }
                }
                else
                {
                    output.Write(ZeroRowsText);
                    base.Render(output);
                }
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSDocumentComparison()
            : base()
        {
            if (Context == null)
            {
                return;
            }

            mDrpDocumentList = new CMSDropDownList();
            mDrpDocumentList.CssClass = "CMSDocumentComparisonDropDownList";
            mBtnAdd = new CMSButton();
            mBtnAdd.CausesValidation = false;
            mBtnAdd.ID = "btnAdd";
            mBtnAdd.CssClass = "CMSDocumentComparisonAddButton";
            mBtnAdd.Text = ResHelper.GetString("general.add");
            mMultiColumnTable = new BasicMultiColumnTable();
            mMultiColumnTable.TableCssClass = "CMSDocumentComparisonTable";
            mBtnAdd.Click += AddSelectedItem;
            PreRender += ControlPrerender;
        }


        /// <summary>
        /// Adds item to the table.
        /// </summary>
        protected void AddSelectedItem(object sender, EventArgs e)
        {
            string newIDs = "," + newDisplayNodeIDs.Trim(',') + ",";

            // Add to the list of nodes if not already present
            if (newIDs.IndexOfCSafe("," + mDrpDocumentList.SelectedValue + ",") < 0)
            {
                newIDs += mDrpDocumentList.SelectedValue;
            }

            // Redirect to the new URL
            string url = URLHelper.AddParameterToUrl(RequestContext.CurrentURL, "displaynodeids", newIDs.Trim(','));
            url = URLHelper.RemoveParameterFromUrl(url, "removenodeid");
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (SettingsKeyInfoProvider.GetValue(SiteName + ".CMSControlElement").ToLowerCSafe().Trim() == "div")
                {
                    return HtmlTextWriterTag.Div;
                }
                else
                {
                    return HtmlTextWriterTag.Span;
                }
            }
        }

        #endregion
    }
}