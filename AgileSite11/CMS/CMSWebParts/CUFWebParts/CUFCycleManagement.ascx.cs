using System;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;


namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFCycleManagement : CMSAbstractWebPart
    {
        #region "Properties"

        public enum ActionEnum { Unknown, Approve = 5, Reject = 6, ApproveForPrint = 10, PrintRejected = 12 };

        /// <summary>
        /// Gets or sets name of StatementRedirectURL.
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectURL"), "");
            }
            set
            {
                this.SetValue("RedirectURL", value);
            }
        }

        public int BatchID
        {
            get
            {
                int ret = 0;
                object o = ViewState["BatchID"];
                if (o != null)
                {
                    ret = Convert.ToInt32(o);
                }
                return ret;
            }
            set
            {
                ViewState["BatchID"] = value;
            }
        }

        public string CycleType
        {
            get
            {
                string ret = "";
                object o = ViewState["CycleType"];
                if (o != null)
                {
                    ret = Convert.ToString(o);
                }
                return ret;
            }
            set
            {
                ViewState["CycleType"] = value;
            }
        }

        public ActionEnum ActionRequested
        {
            get
            {
                int ret = 0;
                object o = ViewState["ActionRequested"];
                if (o != null)
                {
                    ret = Convert.ToInt32(o);
                }
                return (ActionEnum)ret;
            }
            set
            {
                ViewState["ActionRequested"] = (int)value;
            }
        }

        #endregion "Properties"

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack && !IsDesign && !PortalContext.IsDesignMode(ViewMode))
            {
                BatchID = 0;
                ActionRequested = (ActionEnum)QueryHelper.GetInteger("action", 0);
                BatchID = QueryHelper.GetInteger("BatchID", 0); //Get query parameter from URL
                CycleType = QueryHelper.GetString("CycleType", "");
                RunAction();
            }
        }

        protected void RunAction()
        {
            if (ActionRequested != ActionEnum.Unknown)
            {
                String dataDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);

                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@BatchID", BatchID);
                string userName = MembershipContext.AuthenticatedUser.UserName;
                parameters.Add("@UserName", userName.Substring(0, Math.Min(userName.Length, 100)), typeof(string));
                parameters.Add("@BatchStatusID", (int)ActionRequested);

                string query = string.Empty;
                if (BatchID != 0)
                {
                    string actionArea = "BatchStatusID";
                    switch (ActionRequested)
                    {
                        case ActionEnum.ApproveForPrint:
                        case ActionEnum.PrintRejected:
                            actionArea = "BatchPrintStatusID";
                            break;
                    }

                    query = string.Format("Update {0}.dbo.Batch set {1} = @BatchStatusID, ModifiedDate=CURRENT_TIMESTAMP, ModifiedBy=@UserName WHERE BatchID = @BatchID", dataDB, actionArea);
                    lblMessage.Text = String.Format("{0} changed to {1}.", actionArea, Enum.GetName(typeof(ActionEnum), ActionRequested));
                }

                try
                {
                    //lblMessage.Text = query + " " + BatchID + " " + Enum.GetName(typeof(ActionEnum), ActionRequested);
                    ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Unable to update batch." + ex.Message;
                }
            }
            else
            {
                lblMessage.Text = "Unknown managment action requested to be performed.";
            }
        }

        protected void CancelBtn_Click(Object sender, EventArgs e)
        {
            string concatChar = (RedirectURL.Contains("?")) ? "&" : "?";

            string url = string.Format("{0}{1}CycleType={2}", RedirectURL, concatChar, CycleType);
            Response.Redirect(url);
        }
    }
}