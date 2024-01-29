using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMSApp.CMSWebParts.WTE.TrainingNetwork
{
    /// <summary>
    /// TN product filter with new icons
    /// </summary>
    public partial class TN_Streamery_ProductFilter2 : CMSAbstractQueryFilterControl
    {
        #region "classes"

        /// <summary>
        /// Storage for query string data
        /// </summary>
        public struct QueryStringData
        {
            public string Key;
            public string Value;
        }

        #endregion "classes"

        #region "Constant"

        private const string cMostRecentString = "Any";

        #endregion "Constant"

        #region "Members"

        private bool mClearClicked = false;
        private bool mRedirect = true;

        private List<QueryStringData> queryStrings = new List<QueryStringData>();
        private List<string> mQueryStringKey = new List<string>(("nodeid,sort,keyword,language,recent").Split(','));

        #endregion "Members"

        #region "Page event"

        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            SetupControl();

            base.OnInit(e);
        }

        /// <summary>
        /// Save the current selected values
        /// </summary>
        private void SaveCurrentSelection()
        {
            SaveToSession("lstCategory", GetSelectedValue(lstCategory));
            SaveToSession("lstSortBy", GetSelectedValue(lstSortBy));
            SaveToSession("lstLanguage", GetSelectedValue(lstLanguage));
            SaveToSession("lstRecentlyAdded", GetSelectedValue(lstRecentlyAdded));
            SaveToSession("txtKeywords", GetSelectedValue(txtKeywords));
        }

        /// <summary>
        /// Pre render event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Set filter settings
            if (RequestHelper.IsPostBack())
            {
                SaveCurrentSelection();
                if (mRedirect)
                {
                    // need  a setting
                    Redirect();
                }
                else
                {
                    SetFilter();
                }
            }
            else
            {
                SetWhere();
            }

            base.OnPreRender(e);
        }

        #endregion "Page event"

        #region "data binding"

        /// <summary>
        /// Resets filter to default state.
        /// </summary>
        public override void ResetFilter()
        {
            SetSelectedValue(lstCategory, "0");
            SetSelectedValue(lstSortBy, "0");
            SetSelectedValue(lstLanguage, "0");
            SetSelectedValue(lstRecentlyAdded, "0");
            SetSelectedValue(txtKeywords, "");
        }

        /// <summary>
        /// Set filter values from session
        /// </summary>
        private void SetFitlerValues()
        {
            BindCategoryList();
            BindSortByList();
            SetSelectedValue(lstLanguage, GetFromSession("lstLanguage"));
            SetSelectedValue(lstRecentlyAdded, GetFromSession("lstRecentlyAdded"));
            SetSelectedValue(txtKeywords, GetFromSession("txtKeywords"));
        }

        /// <summary>
        /// Bind the category drop down or radio button list
        /// </summary>
        private void BindCategoryList()
        {
            string selectedNode = GetFromSession("lstCategory");
            selectedNode = SqlHelper.EscapeLikeText(QueryHelper.GetString("CurrentNode", selectedNode));

            //populate type drop down
            string nodeid = QueryHelper.GetString("NodeID", ValidationHelper.GetString(DocumentContext.CurrentDocument.NodeID, String.Empty));
            // we should be taking the nodeid from the query string first.
            QueryDataParameters qdp = new QueryDataParameters();
            //nodeID = ValidationHelper.GetInteger(DocumentContext.CurrentDocument.NodeID, 0);
            qdp.Add("NodeID", nodeid);

            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_custom_GetCategoryNodes", qdp, QueryTypeEnum.StoredProcedure, true);

            if (ds.Tables.Count > 0)
            {
                lstCategory.DataSource = ds.Tables[0];
                lstCategory.DataValueField = "nodeid";
                lstCategory.DataTextField = "nodename";
                lstCategory.DataBind();

                if (lstCategory.GetType() == typeof(DropDownList))
                {
                    lstCategory.Items.Insert(0, new ListItem(cMostRecentString, "0"));
                }
            }
            SetSelectedValue(lstCategory, "0");
            SetSelectedValue(lstCategory, selectedNode);
        }

        /// <summary>
        /// Bind the sort by drop down or radio button list
        /// </summary>
        private void BindSortByList()
        {
            string currentSort = GetFromSession("lstSortBy");

            //populate status drop down
            lstSortBy.Items.Add(new ListItem("Date Added", "0"));
            lstSortBy.Items.Add(new ListItem("Course ID/SKU", "1"));
            lstSortBy.Items.Add(new ListItem("Title (A to Z)", "7"));
            lstSortBy.Items.Add(new ListItem("Production Year (Newest)", "3"));
            //lstSortBy.Items.Add(new ListItem("Production Year (Oldest)", "6"));
            lstSortBy.Items.Add(new ListItem("Length (Shortest)", "5"));
            //lstSortBy.Items.Add(new ListItem("Length (Longest)", "4"));

            SetSelectedValue(lstSortBy, "0");
            SetSelectedValue(lstSortBy, currentSort);
        }

        /// <summary>
        /// Setup the inner controls.
        /// </summary>
        private void SetupControl()
        {
            if (this.StopProcessing)
            {
                this.Visible = false;
            }
            else if (!RequestHelper.IsPostBack())
            {
                SetFitlerValues();
            }
        }

        /// <summary>
        /// Redirect instead of set filter
        /// </summary>
        protected void Redirect()
        {
            string path = QueryHelper.GetString("aliaspath", "xx");
            /*
            string selectedCategory = lstCategory.SelectedValue;
            string selectedSort = lstSortBy.SelectedValue;
            string selectedKeywords = txtKeywords.Text;
            string selectedLanguage = lstLanguage.SelectedValue;
            string selectedRecent = lstRecentlyAdded.SelectedValue;
            string url = string.Format("{0}?NodeID={1}&sort={2}&keyword={3}&language={4}&recent={5}", path, selectedCategory, selectedSort, selectedKeywords, selectedLanguage, selectedRecent);
            */
            string url = AddQueryString(path, GetFilterQueryString());
            Response.Redirect(url, true);
        }

        /// <summary>
        /// Get query string from the selected values
        /// </summary>
        /// <returns></returns>
        protected string GetFilterQueryString()
        {
            string ret = String.Empty;
            string selectedCategory = lstCategory.SelectedValue;
            string selectedSort = lstSortBy.SelectedValue;
            string selectedKeywords = txtKeywords.Text;
            string selectedLanguage = lstLanguage.SelectedValue;
            string selectedRecent = lstRecentlyAdded.SelectedValue;

            ret = ConcatQueryString(ret, "nodeid", selectedCategory);
            ret = ConcatQueryString(ret, "sort", selectedSort);
            ret = ConcatQueryString(ret, "keyword", selectedKeywords);
            ret = ConcatQueryString(ret, "language", selectedLanguage);
            ret = ConcatQueryString(ret, "recent", selectedRecent);

            return ret;
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetFilter()
        {
            SetWhere();
            this.OrderBy = null;
            this.RaiseOnFilterChanged();
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetWhere()
        {
            // need this for "reset function"
            if (!Page.IsPostBack || mClearClicked)
            {
                // first time through or "cleared" show the default result?
                this.WhereCondition = "";
            }
            else
            {
                StringBuilder where = new StringBuilder();

                string selectedType = lstCategory.SelectedValue;
                if (!string.IsNullOrWhiteSpace(selectedType))
                {
                    if (selectedType != cMostRecentString)
                    {
                        // where.Append(string.Format(" BT.MasterType = '{0}'", selectedType));
                    }
                    else
                    {
                        this.TopN = 50;
                        this.OrderBy = "NodeName Asc";
                    }
                }

                string selectedStatus = lstSortBy.SelectedValue;
                if (!string.IsNullOrWhiteSpace(selectedStatus) && selectedStatus != "0")
                {
                    if (where.Length > 0)
                    {
                        where.Append(" AND ");
                    }
                    where.Append(string.Format(" NodeParentID = {0}", selectedStatus));
                }

                if (where.Length > 0)
                {
                    // Set where condition
                    this.WhereCondition = where.ToString();
                }
            }
        }

        #endregion "data binding"

        #region "general events"

        /// <summary>
        /// Size drop down index changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void txtKeywords_OnTextChaged(object sender, EventArgs e)
        {
            // do nothing, we are just wiring up.
        }

        /// <summary>
        /// Reset button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbtnReset_Click(object sender, EventArgs e)
        {
            mClearClicked = true;
            ResetFilter();
            Redirect();
        }

        /// <summary>
        /// Apply filter clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbtnApplyFilter_Click(object sender, EventArgs e)
        {
            Redirect();
        }

        #endregion "general events"

        #region "methods"

        #region "helpers"

        /// <summary>
        /// Get the current query string
        /// </summary>
        /// <returns></returns>
        private string GetQueryStrings()
        {
            string ret = String.Empty;
            foreach (String key in Request.QueryString.AllKeys)
            {
                //Response.Write("Key: " + key + " Value: " + Request.QueryString[key]);
                ret = ConcatQueryString(ret, key, Request.QueryString[key]);
            }
            return ret;
        }

        /// <summary>
        /// Add query string to url
        /// </summary>
        /// <param name="p_url"></param>
        /// <param name="p_queryString"></param>
        /// <returns></returns>
        private string AddQueryString(string p_url, string p_queryString)
        {
            string ret = String.Empty;
            if (!String.IsNullOrWhiteSpace(p_queryString))
            {
                if (!String.IsNullOrWhiteSpace(p_url))
                {
                    ret = p_url + "?" + p_queryString;
                }
            }
            return ret;
        }

        /// <summary>
        /// Concat 2 query string
        /// </summary>
        /// <param name="p_string"></param>
        /// <param name="p_key"></param>
        /// <param name="p_value"></param>
        /// <returns></returns>
        private string ConcatQueryString(string p_string, string p_key, string p_value)
        {
            string ret = String.Empty;
            if (!String.IsNullOrWhiteSpace(p_string))
            {
                ret = p_string;
            }
            string qstring = MakeQueryString(p_key, p_value);
            if (!String.IsNullOrWhiteSpace(qstring))
            {
                if (!String.IsNullOrWhiteSpace(ret))
                {
                    ret += "&";
                }
                ret += qstring;
            }
            return ret;
        }

        /// <summary>
        /// Make string value pair
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_value"></param>
        /// <returns></returns>
        private string MakeQueryString(string p_key, string p_value)
        {
            string ret = String.Empty;
            if (!String.IsNullOrWhiteSpace(p_key) && !String.IsNullOrWhiteSpace(p_value))
            {
                ret = p_key + "=" + p_value;
            }
            return ret;
        }

        /// <summary>
        /// Get string value from the session
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        private string GetFromSession(string p_key)
        {
            Object o = SessionHelper.GetValue(p_key);
            string ret = "";
            if (o != null)
            {
                try
                {
                    ret = Convert.ToString(o);
                }
                catch
                {
                    //noop
                }
            }
            return ret;
        }

        /// <summary>
        /// Save a string value to session
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_value"></param>
        private void SaveToSession(string p_key, string p_value)
        {
            SessionHelper.SetValue(p_key, p_value);
        }

        /// <summary>
        /// Get the current value for the control
        /// </summary>
        /// <param name="p_control"></param>
        private string GetSelectedValue(Control p_control)
        {
            string ret = String.Empty;

            if (p_control != null)
            {
                if (p_control.GetType() == typeof(DropDownList))
                {
                    ret = ((DropDownList)p_control).SelectedValue;
                }
                else if (p_control.GetType() == typeof(RadioButtonList))
                {
                    ret = ((RadioButtonList)p_control).SelectedValue;
                }
                else if (p_control.GetType() == typeof(TextBox))
                {
                    ret = ((TextBox)p_control).Text;
                }
            }

            return ret;
        }

        /// <summary>
        /// Set control selected value.
        /// </summary>
        /// <param name="p_control"></param>
        /// <param name="p_value"></param>
        private void SetSelectedValue(Control p_control, string p_value)
        {
            if (p_control != null)
            {
                if (p_control.GetType() == typeof(DropDownList))
                {
                    //ListItem di = ((DropDownList)p_control).Items.FindByValue(p_value);
                    //if (di != null)
                    //{
                    //    di.Selected = true;
                    //}
                    ((DropDownList)p_control).SelectedValue = p_value;
                }
                else if (p_control.GetType() == typeof(RadioButtonList))
                {
                    //ListItem ri = ((RadioButtonList)p_control).Items.FindByValue(p_value);
                    //if (ri != null)
                    //{
                    //    ri.Selected = true;
                    //}
                    ((RadioButtonList)p_control).SelectedValue = p_value;
                }
                else if (p_control.GetType() == typeof(TextBox))
                {
                    ((TextBox)p_control).Text = p_value;
                }
            }
        }

        #endregion "helpers"

        #endregion "methods"
    }
}