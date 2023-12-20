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

namespace CMSApp.CMSWebParts.WTE.Rideology
{
    public partial class ValidateSponsorUser : CMSAbstractWebPart
    {
        #region "Enums"

        public enum ValidationTypeEnum
        {
            User_To_Sponsor = 0
        }

        #endregion "Enums"

        #region "Properties"

        /// <summary>
        /// Gets or sets Ambassador ID passed from web part in cms.
        /// </summary>
        public string SponsorID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("SponsorID"), "");
            }
            set
            {
                this.SetValue("SponsorID", value);
            }
        }

        /// <summary>
        /// Gets or sets User ID passed from web part in cms.
        /// </summary>
        public string UserID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("UserID"), "");
            }
            set
            {
                this.SetValue("UserID", value);
            }
        }

        /// <summary>
        /// Gets or sets BrandID passed from web part in cms.
        /// </summary>

        /// <summary>
        /// Gets or sets Ambassador ID passed from web part in cms.
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
                    case ValidationTypeEnum.User_To_Sponsor:
                        ret = "custom_ValidateUserToSponsor";
                        break;
                }

                return ret;
            }
            set
            {
                this.SetValue("sproc", value);
            }
        }

        #endregion "Properties"

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
                case ValidationTypeEnum.User_To_Sponsor:
                    p.Add("@SponsorID", SponsorID);
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

        #endregion "Methods"
    }
}