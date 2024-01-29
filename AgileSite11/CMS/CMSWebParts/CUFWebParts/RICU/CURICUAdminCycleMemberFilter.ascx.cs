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

namespace CMSApp.CMSWebParts.CUFWebParts.RICU
{
    public partial class CURICUAdminCycleMemberFilter : CMSAbstractQueryFilterControl
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


        /// <summary>
        /// Pre render event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Set filter settings
            if (!RequestHelper.IsPostBack())
            {
                object o = SessionHelper.GetValue("AdminCycleMemberSearch");
                if (o != null)
                {
                    string searchString = Convert.ToString(o);
                    //12/14/14 - CES request to not hold last search
                    //txtMemberNumber.Text = searchString;
                    txtMemberNumber.Text = "";
                }
            }
            else if (RequestHelper.IsPostBack())
            {
                SessionHelper.SetValue("AdminCycleMemberSearch", txtMemberNumber.Text);
            }

            SetFilter();

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
            this.WhereCondition = "1=1";

            StringBuilder sb = new StringBuilder();

            // Generate WHERE condition based on team selected
            string searchString = SqlHelper.GetSafeQueryString(txtMemberNumber.Text.Trim());

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string statementDB = SettingsKeyInfoProvider.GetStringValue("StatementDatabase", SiteContext.CurrentSiteID);
                if (string.IsNullOrWhiteSpace(statementDB))
                {
                    sb.Append("Statement Database custom setting in CMS desk was not found. Please add in CMS Site Manager | Settings | Custom Settings");
                }

                string memberAccountSQL = string.Format("M.MemberID IN (select MemberId FROM {0}.dbo.fn_getMemberIdsForSearch('{1}'))", statementDB, searchString);
                sb.Append(memberAccountSQL);
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