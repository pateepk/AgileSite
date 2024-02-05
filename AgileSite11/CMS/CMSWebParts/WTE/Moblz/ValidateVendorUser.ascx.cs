using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;
using CMS.DataEngine;

namespace CMSApp.CMSWebParts.WTE.MOBLZ
{
    public partial class CMSWebParts_WTE_Custom_ValidateVendorUser : CMSAbstractWebPart
    {
        #region "Enums"
        public enum ValidationTypeEnum
        {
            VendorUser_To_Vendor = 0,
            PMUser_To_Vendor = 1,
            PMUser_To_Property = 2,
            PMUser_To_Event = 3,
            PMUser_To_EventDate = 4,
            VendorUser_To_Document = 5
        }
        #endregion

        #region "Properties"

        /// <summary>
        /// Gets or sets vendor ID passed from web part in cms.
        /// </summary>
        public string VendorID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("VendorID"), "");
            }
            set
            {
                this.SetValue("VendorID", value);
            }
        }

        /// <summary>
        /// Gets or sets Document ID passed from web part in cms.
        /// </summary>
        public string DocumentID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("DocumentID"), "");
            }
            set
            {
                this.SetValue("DocumentID", value);
            }
        }

        /// <summary>
        /// Gets or sets PropertyID passed from web part in cms.
        /// </summary>
        public string PropertyID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("PropertyID"), "");
            }
            set
            {
                this.SetValue("PropertyID", value);
            }
        }

        /// <summary>
        /// Gets or sets EventID passed from web part in cms.
        /// </summary>
        public string EventID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("EventID"), "");
            }
            set
            {
                this.SetValue("EventID", value);
            }
        }

        /// <summary>
        /// Gets or sets vendor ID passed from web part in cms.
        /// </summary>
        public string EventDateID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("EventDateID"), "");
            }
            set
            {
                this.SetValue("EventDateID", value);
            }
        }

        /// <summary>
        /// Gets or sets vendor ID passed from web part in cms.
        /// </summary>
        public ValidationTypeEnum ValidationType
        {
            get
            {
                return (ValidationTypeEnum)ValidationHelper.GetInteger(this.GetValue("ValidationType"), 0);
            }
            set
            {
                this.SetValue("ValidationType", value);
            }
        }

        /// <summary>
        /// Gets or sets redirect URL passed from web part in cms.
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

        /// <summary>
        /// Gets or sets redirect URL passed from web part in cms.
        /// </summary>
        public string sproc
        {
            get
            {
                string ret = "";

                switch (ValidationType)
                {
                    case ValidationTypeEnum.VendorUser_To_Vendor:
                        ret = "custom_ValidateVendorUserToVendor";
                        break;
                    case ValidationTypeEnum.PMUser_To_Vendor:
                        ret = "custom_ValidatePropertyUserToVendor";
                        break;
                    case ValidationTypeEnum.PMUser_To_Property:
                        ret = "custom_ValidatePropertyUserToProperty";
                        break;
                    case ValidationTypeEnum.PMUser_To_Event:
                        ret = "custom_ValidatePropertyUserToEvent";
                        break;
                    case ValidationTypeEnum.PMUser_To_EventDate:
                        ret = "custom_ValidatePropertyUserToEventDate";
                        break;
                    case ValidationTypeEnum.VendorUser_To_Document:
                        ret = "custom_ValidateVendorUserToDocument";
                        break;
                    default:
                        ret = "";
                        break;
                }

                return ret;
            }
            set
            {
                this.SetValue("sproc", value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Content loaded event handler. - Main functionality is in here
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();

            QueryDataParameters parameters = new QueryDataParameters();

            //custom parameters depending on validation type
            addParams(parameters);

            //check for validity and redirect
            if (CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Edit &&
                CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Design)
            {
                if (!UserIsValid(parameters, sproc))
                {
                    Response.Redirect(RedirectURL);
                }
            }
        }


        protected void addParams(QueryDataParameters p)
        {
            //we always need the user id
            UserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                p.Add("@UserID", user.UserID.ToString());
            }

            //Specific to validation type
            switch (ValidationType)
            {
                case ValidationTypeEnum.VendorUser_To_Vendor:
                    p.Add("@VendorID", VendorID);
                    break;
                case ValidationTypeEnum.VendorUser_To_Document:
                    p.Add("@DocumentID", DocumentID);
                    break;
                case ValidationTypeEnum.PMUser_To_Vendor:
                    p.Add("@VendorID", VendorID);
                    break;
                case ValidationTypeEnum.PMUser_To_Property:
                    p.Add("@PropertyID", PropertyID);
                    break;
                case ValidationTypeEnum.PMUser_To_Event:
                    p.Add("@EventID", EventID);
                    break;
                case ValidationTypeEnum.PMUser_To_EventDate:
                    p.Add("@EventDateID", EventDateID);
                    break;
            }
        }

        protected bool UserIsValid(QueryDataParameters parameters, string sproc)
        {
            DataSet ds = null;
            GeneralConnection cn = ConnectionHelper.GetConnection();
            QueryParameters qp = new QueryParameters(sproc, parameters, QueryTypeEnum.StoredProcedure, false);
            ds = cn.ExecuteQuery(qp);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (this.StopProcessing)
            {
                // Do not process
            }
            else
            {

            }
        }

        /// <summary>
        /// Reloads the control data.
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();

            SetupControl();
        }

        #endregion
    }
}