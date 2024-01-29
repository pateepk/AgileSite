//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using CMS.Controls;
//using CMS.GlobalHelper;
//using CMS.CMSHelper;
//using CMS.Helpers;
//using CMS.Membership;
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
    public partial class CUAdminCycleMemberFilter : CMSAbstractQueryFilterControl
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
            string where = "";

            // Generate WHERE condition based on member number entered
            string memberNumber = txtMemberNumber.Text;
            if (!string.IsNullOrWhiteSpace(memberNumber))
            {
                long memberId = ValidationHelper.GetLong(memberNumber, 0);
                if (memberId >= 0)
                {
                    where = "MemberNumber = " + memberId;
                }
            }

            if (MembershipContext.AuthenticatedUser.IsInRole("MarketingCreditUnion", SiteContext.CurrentSiteName) ||
                MembershipContext.AuthenticatedUser.IsInRole("CustomerService", SiteContext.CurrentSiteName))
            {
                if (!string.IsNullOrWhiteSpace(where))
                {
                    where += " AND "; 
                }
                where += " MemberNumber <> 0";
            }
            
            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }
    }
}
