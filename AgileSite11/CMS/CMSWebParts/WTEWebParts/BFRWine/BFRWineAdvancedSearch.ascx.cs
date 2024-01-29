using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMSApp.CMSWebParts.WTEWebParts.BFRWine
{
    public partial class BFRWineAdvancedSearch : CMSAbstractDataFilterControl
    {
        private Boolean mClearClicked = false;

        #region "Properties"
        #endregion

        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            SetupControl();

            base.OnInit(e);
        }


        /// <summary>
        /// Pre render event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Set filter settings
            if (RequestHelper.IsPostBack())
            {
                SetFilter();
                SessionHelper.SetValue("ddlPointRating", ddlPointRating.SelectedValue);
            }
            else
            {
                SetWhere();
            }

            base.OnPreRender(e);
        }


        private void FillDropDownList(DropDownList ddl, String ddlColumn, String ddlSelectedValue)
        {
            ddl.Items.Add(new ListItem("Any", ""));

            QueryDataParameters parameters = new QueryDataParameters();
            string defaultDDLQueryFormat = "SELECT DISTINCT {0} FROM dbo.custom_BFR_Wine WHERE {0} IS NOT NULL Order By 1";
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format(defaultDDLQueryFormat, ddlColumn), parameters, QueryTypeEnum.SQLQuery);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string value = Convert.ToString(row[ddlColumn]);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        ListItem listItem = new ListItem(value, value);
                        if (value == ddlSelectedValue)
                        {
                            listItem.Selected = true;
                        }

                        ddl.Items.Add(listItem);
                    }
                }
            }
        }

        private void FillCheckBoxList(CheckBoxList ddl, String ddlColumn, String[] ddlSelectedValue)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            string defaultDDLQueryFormat = "SELECT DISTINCT {0} FROM dbo.custom_BFR_Wine WHERE {0} IS NOT NULL Order By 1";

            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format(defaultDDLQueryFormat, ddlColumn), parameters, QueryTypeEnum.SQLQuery);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string value = Convert.ToString(row[ddlColumn]);
                    ListItem listItem = new ListItem(value, value);
                    if (ddlSelectedValue.Contains(value))
                    {
                        listItem.Selected = true;
                    }

                    ddl.Items.Add(listItem);
                }
            }
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
                string ddlSelectedValue = "";

                FillDropDownList(ddlCountry, "BFR_WineCountry", ddlSelectedValue);
                FillDropDownList(ddlProducer, "BFR_WineProducerSearch", ddlSelectedValue);
                FillDropDownList(ddlVarietal, "BFR_WineVarietal", ddlSelectedValue);
                FillDropDownList(ddlVintage, "BFR_WineVintage", ddlSelectedValue);

                //FillCheckBoxList(cblCategory, "BFR_WineCategory", ddlSelectedValue.Split(';'));
                //FillCheckBoxList(cblAgriculturalPractice, "BFR_WineAgMethod", ddlSelectedValue.Split(';'));

                int lastPointSelection = 0;
                Object o = SessionHelper.GetValue("ddlPointRating");
                if (o != null)
                {
                    if (!int.TryParse(Convert.ToString(o), out lastPointSelection))
                    {
                        lastPointSelection = 0;
                    }
                }

                int i = 0;
                ListItem listItem = null;

                for (i = 0; i < 100; i++)
                {
                    if (i == 0)
                    {
                        listItem = new ListItem("Any", "");
                    }
                    else
                    {
                        listItem = new ListItem(string.Format("{0}+", i), Convert.ToString(i));
                    }

                    if (i == lastPointSelection)
                    {
                        listItem.Selected = true;
                    }

                    ddlPointRating.Items.Add(listItem);

                    if (i == 0)
                    {
                        i = 74;
                    }
                }
            }
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

        protected void btnClear_Click(Object sender, EventArgs e)
        {
            mClearClicked = true;

            List<DropDownList> ddlList = new List<DropDownList>();
            ddlList.Add(ddlProducer);
            ddlList.Add(ddlCountry);
            ddlList.Add(ddlVarietal);
            ddlList.Add(ddlVintage);
            ddlList.Add(ddlSRP);
            ddlList.Add(ddlPointRating);
            ddlList.Add(ddlWineReviewer);

            foreach (DropDownList ddl in ddlList)
            {
                ddl.ClearSelection();
                ListItem listItem = ddl.Items.FindByValue("");
                if (listItem != null)
                {
                    listItem.Selected = true;
                }
            }

            List<CheckBoxList> cblList = new List<CheckBoxList>();
            cblList.Add(cblCategory);
            cblList.Add(cblAgriculturalPractice);

            foreach (CheckBoxList cbl in cblList)
            {
                foreach (ListItem testValue in cbl.Items)
                {
                    testValue.Selected = false;
                }
            }
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetWhere()
        {
            string where = "1 = 1";

            if (!Page.IsPostBack || mClearClicked)
            {
                //first time through or clear click do not return results
                where = "1 = 0";
            }

            string pointRating = ddlPointRating.SelectedValue;
            if (!string.IsNullOrWhiteSpace(pointRating))
            {
                where += string.Format(" AND BFR_WineRatings >= '{0}'", pointRating);
            }

            string srp = ddlSRP.SelectedValue;
            if (!string.IsNullOrWhiteSpace(srp))
            {
                where += " AND (ISNULL(BFR_WineSRP, 0) > 0";
                switch (srp)
                {
                    case "1":
                        where += " AND ISNULL(BFR_WineSRP, 0) < 11.00";
                        break;
                    case "2":
                        where += " AND ISNULL(BFR_WineSRP, 0) >= 11.00 AND ISNULL(BFR_WineSRP, 0) < 16.00";
                        break;
                    case "3":
                        where += " AND ISNULL(BFR_WineSRP, 0) >= 16.00 AND ISNULL(BFR_WineSRP, 0) < 20.00";
                        break;
                    case "4":
                        where += " AND ISNULL(BFR_WineSRP, 0) >= 20.00 AND ISNULL(BFR_WineSRP, 0) < 30.00";
                        break;
                    case "5":
                        where += " AND ISNULL(BFR_WineSRP, 0) >= 30.00 AND ISNULL(BFR_WineSRP, 0) < 50.00";
                        break;
                    case "6":
                        where += " AND ISNULL(BFR_WineSRP, 0) >= 50.00";
                        break;
                }
                where += " )";
            }

            string selection = ddlCountry.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                where += string.Format(" AND BFR_WineCountry = '{0}'", selection);
            }

            selection = ddlProducer.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                where += string.Format(" AND BFR_WineProducerSearch = '{0}'", selection);
            }

            selection = ddlVarietal.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                where += string.Format(" AND BFR_WineVarietal = '{0}'", selection);
            }

            selection = ddlVintage.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                where += string.Format(" AND BFR_WineVintage = '{0}'", selection);
            }

            string cblWhere = "";
            foreach (ListItem testValue in cblCategory.Items)
            {
                if (testValue.Selected)
                {
                    cblWhere += string.Format(" OR BFR_WineCategory LIKE '%{0}%'", testValue.Value);
                }
            }
            if (!string.IsNullOrWhiteSpace(cblWhere))
            {
                where += string.Format(" AND (1=0");
                where += cblWhere;
                where += string.Format(" )");
            }

            cblWhere = "";
            foreach (ListItem testValue in cblAgriculturalPractice.Items)
            {
                if (testValue.Selected)
                {
                    cblWhere += string.Format(" OR BFR_WineAgMethod LIKE '%{0}%'", testValue.Value);
                }
            }
            if (!string.IsNullOrWhiteSpace(cblWhere))
            {
                where += string.Format(" AND (1=0");
                where += cblWhere;
                where += string.Format(" )");
            }

            selection = ddlWineReviewer.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                where += string.Format(" AND (BFR_WineReviewer = '{0}' OR BFR_WineReviewer2 = '{0}' OR BFR_WineReviewer3 = '{0}' OR BFR_WineReviewer4 = '{0}')", selection);
            }

            // Set where condition
            this.WhereCondition = where;

            //lblMessage.Text = where;
        }

    }
}