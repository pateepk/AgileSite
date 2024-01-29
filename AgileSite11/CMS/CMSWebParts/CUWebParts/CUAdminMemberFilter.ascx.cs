//using System;
//using System.Text;
//using System.Web.UI;
//using CMS.Controls;
//using CMS.DataEngine;
//using CMS.Helpers;
//using CMS.SiteProvider;
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

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUAdminMemberFilter : CMSAbstractQueryFilterControl
    {
        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            SetWhere();

            SetupControl();

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Page.IsPostBack)
            {
                SessionHelper.SetValue("AdminMemberSearch", txtMemberNumber.Text.Trim());
            }
            else
            {
                string test = Convert.ToString(SessionHelper.GetValue("AdminMemberSearch"));
                if (!String.IsNullOrWhiteSpace(test))
                {
                    txtMemberNumber.Text = test;
                }
            }

            base.OnLoad(e);
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
            }

            base.OnPreRender(e);
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

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetWhere()
        {
            //init
            this.WhereCondition = "1=0";

            StringBuilder sb = new StringBuilder();

            // Generate WHERE condition based on team selected
            string searchString = txtMemberNumber.Text.Trim();

            if (Page.IsPostBack)
            {
                object o = SessionHelper.GetValue("AdminMemberSearch");
                if (o != null)
                {
                    searchString = Convert.ToString(o);
                    txtMemberNumber.Text = searchString;
                }
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                long searchLong = 0;
                if (long.TryParse(searchString, out searchLong))
                {
                    if (searchLong > 0)
                    {
                        string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
                        if (string.IsNullOrWhiteSpace(statementDB))
                        {
                            sb.Append("Statement Database custom setting in CMS desk was not found. Please add in CMS Site Manager | Settings | Custom Settings");
                        }

                        string memberAccountSQL = "MemberNumber IN "
                             + " (  "
                             + " SELECT {1} MemberNumber \n"
                             + " UNION "
                             + " select MemberNumber from {0}.dbo.Statement S  "
                             + " WHERE S.ID in (SELECT StatementID FROM {0}.dbo.ShareAccount SA "
                             + " WHERE SA.AccountNumber = {1}) \n"
                             + " OR S.ID IN ( SELECT StatementID FROM {0}.dbo.LoanAccount LA  "
                             + " WHERE LA.AccountNumber = {1} )"
                             + " UNION "
                             + " select MemberNumber from {0}.dbo.TaxInfo TI"
                             + " where TI.MemberNumber = {1} or TI.AccountNumber = '{1}'"
                             + " UNION "
                             + " select MemberAgreeNbr from {0}.dbo.Notice"
                             + " where MemberAgreeNbr = {1})";

                        sb.Append(string.Format(memberAccountSQL, statementDB, searchLong));
                    }
                }
                else if (searchString.Length >= 3)
                {
                    sb.Append(string.Format("MemName like '%{0}%'", SqlHelper.GetSafeQueryString(searchString)));
                }
            }

            string where = sb.ToString();

            if (!string.IsNullOrWhiteSpace(where))
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }
    }
}