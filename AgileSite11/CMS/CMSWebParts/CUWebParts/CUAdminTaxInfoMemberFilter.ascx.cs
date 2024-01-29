//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using CMS.GlobalHelper;
//using CMS.Controls;
//using CMS.CMSHelper;
//using CMS.SettingsProvider;
//using CMS.Helpers;
//using CMS.Membership;
//using CMS.SiteProvider;
//using CMS.DataEngine;
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
    public partial class CUAdminTaxInfoMemberFilter : CMSAbstractQueryFilterControl
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

            // Generate WHERE condition based on team selected
            string searchString = txtMemberNumber.Text.Trim();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                long searchLong = 0;
                if (long.TryParse(searchString, out searchLong))
                {
                    if (searchLong > 0)
                    {
                        where = string.Format("(TI.MemberNumber = {0} or TI.AccountNumber = '{0}')", searchLong);
                    }
                }
                else if (searchString.Length >= 3)
                {
                    where = string.Format("(TI.Name like '%{0}%' or TI.Address1 like '%{0}%' or TI.AccountNumber = '{0}')", SqlHelper.GetSafeQueryString(searchString));
                }
            }

            if (MembershipContext.AuthenticatedUser.IsInRole("MarketingCreditUnion", SiteContext.CurrentSiteName) ||
                MembershipContext.AuthenticatedUser.IsInRole("CustomerService", SiteContext.CurrentSiteName))
            {
                if (!string.IsNullOrWhiteSpace(where))
                {
                    where += " AND ";
                }
                where += " TI.MemberNumber <> 0";
            }

            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }
    }
}
