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

namespace CMSApp.CMSWebParts.CUFWebParts.ComFirst
{
    public partial class CUCFAdminMemberFilter : CMSAbstractQueryFilterControl
    {
        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Creates the child controls
            SetupControl();
            base.OnInit(e);
        }

        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Checks if the current request is a postback
            if (RequestHelper.IsPostBack())
            {
                // Applies the filter to the displayed data
                //SetFilter();
            }

            //need this here to handle column header clicks
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
            //init
            string where = "";
            string order = null;

            StringBuilder sb = new StringBuilder();

            // Generate WHERE condition based on team selected
            string searchString = SqlHelper.GetSafeQueryString(txtMemberNumber.Text.Trim());

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
                if (string.IsNullOrWhiteSpace(statementDB))
                {
                    sb.Append("Statement Database custom setting in CMS desk was not found. Please add in CMS Site Manager | Settings | Custom Settings");
                }

                string memberAccountSQL = string.Format("M.MemberID IN (select MemberId FROM {0}.dbo.fn_getMemberIdsForSearch('{1}'))", statementDB, searchString);
                sb.Append(memberAccountSQL);
            }
            else
            {
                sb.Append("1=0");
            }

            if (sb.Length > 0)
            {
                where = sb.ToString();
            }

            if (!string.IsNullOrWhiteSpace(where))
            {
                //macro processing not needed in this filter, keeping as an example
                //where = ResolveMacros(where);

                // Set where condition
                this.WhereCondition = where;
            }

            if (order != null)
            {
                // Sets the OrderBy clause
                this.OrderBy = order;
            }

            this.RaiseOnFilterChanged();
        }

        public virtual string ResolveMacros(string inputText, MacroSettings settings = null)
        {
            // Ensure macro context object
            if (settings == null)
            {
                settings = new MacroSettings();
                settings.Culture = Thread.CurrentThread.CurrentCulture.ToString();
            }

            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();
            return resolver.ResolveMacros(inputText, settings);
        }
    }
}