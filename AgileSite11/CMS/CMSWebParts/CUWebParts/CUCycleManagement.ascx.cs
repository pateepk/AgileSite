using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.PortalEngine.Web.UI;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Membership;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUCycleManagement : CMSAbstractWebPart
    {
        #region "Properties"
        public enum ActionEnum { Unknown, Approve, Reject };
        public enum CycleTypeEnum { Statement, Notice };

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CUDBName
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CUDBName"), "");
            }
            set
            {
                this.SetValue("CUDBName", value);
            }
        }

        /// <summary>
        /// Gets or sets name of StatementRedirectURL.
        /// </summary>
        public string StatementRedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("StatementRedirectURL"), "");
            }
            set
            {
                this.SetValue("StatementRedirectURL", value);
            }
        }

        /// <summary>
        /// Gets or sets name of StatementRedirectURL.
        /// </summary>
        public string NoticeRedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("NoticeRedirectURL"), "");
            }
            set
            {
                this.SetValue("NoticeRedirectURL", value);
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

        public CycleTypeEnum CycleType
        {
            get
            {
                CycleTypeEnum ret = CycleTypeEnum.Statement;
                object o = ViewState["CycleType"];
                if (o != null)
                {
                    ret = (CycleTypeEnum)Convert.ToInt32(o);
                }
                return ret;
            }
            set
            {
                ViewState["CycleType"] = (int)value;
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
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BatchID = 0;
                string actionString = QueryHelper.GetString("action", "").ToLower();
                if (actionString == "approve" || actionString == "reject")
                {
                    BatchID = QueryHelper.GetInteger("BatchID", 0); //Get query parameter from URL
                    
                    switch (actionString)
                    {
                        case "approve":
                            ActionRequested = ActionEnum.Approve;
                            break;
                        case "reject":
                            ActionRequested = ActionEnum.Reject;
                            break;
                    }

                    CycleType = (CycleTypeEnum)QueryHelper.GetInteger("CycleType", 0); //Get query parameter from URL
                    
                }
                RunAction();
            }
        }

        protected void RunAction()
        {
            if (ActionRequested != ActionEnum.Unknown)
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@BatchID", BatchID);
                parameters.Add("@UserName", MembershipContext.AuthenticatedUser.UserName);
                int batchStatusID = 0;
                switch (ActionRequested)
                {
                    case ActionEnum.Approve:
                        batchStatusID = 5;
                        break;
                    case ActionEnum.Reject:
                        batchStatusID = 6;
                        break;
                }
                parameters.Add("@BatchStatusID", batchStatusID);
                
                string query = string.Empty;
                if (BatchID != 0)
                {
                    query = "Update " + CUDBName + ".dbo.Batch set BatchStatusID = @BatchStatusID, ModifiedDate=CURRENT_TIMESTAMP, ModifiedBy=SUBSTRING(@UserName,0,100) WHERE ID = @BatchID";
                    lblMessage.Text = String.Format("Batch status changed to {0}.", Enum.GetName(typeof(ActionEnum), ActionRequested));
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
            switch (CycleType)
            {
                case CycleTypeEnum.Notice:
                    Response.Redirect(NoticeRedirectURL);
                    break;
                default:
                    Response.Redirect(StatementRedirectURL);
                    break;
            }
        }
    }
}