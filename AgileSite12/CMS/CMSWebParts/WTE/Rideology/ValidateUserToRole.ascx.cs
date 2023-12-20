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
    public partial class ValidateUserToRole : CMSAbstractWebPart
    {
        #region "Enums"

        public enum ValidationTypeEnum
        {
            User_To_Ambassador = 0,
            User_To_AmbassadorBrand = 1,
            User_To_Sponsor = 2,
            User_To_EventManager = 3,
            User_To_CarMeet = 4,
            User_To_CarClub = 5,
            User_To_EventProfessional = 6,
            User_To_Charity = 7
        }

        #endregion "Enums"

        #region "Properties"

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
        public string ObjectID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectID"), "");
            }
            set
            {
                this.SetValue("ObjectID", value);
            }
        }

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
                    case ValidationTypeEnum.User_To_Ambassador:
                        ret = "custom_ValidateAmbassadorUserToAmbassador";
                        break;

                    case ValidationTypeEnum.User_To_AmbassadorBrand:
                        ret = "custom_ValidateAmbassadorUserToBrand";
                        break;

                    case ValidationTypeEnum.User_To_Sponsor:
                        ret = "custom_ValidateUserToSponsor";
                        break;

                    case ValidationTypeEnum.User_To_EventManager:
                        ret = "custom_ValidateUserToEventManager";
                        break;

                    case ValidationTypeEnum.User_To_CarMeet:
                        ret = "custom_ValidateUserToCarMeet";
                        break;

                    case ValidationTypeEnum.User_To_CarClub:
                        ret = "custom_ValidateUserToCarClub";
                        break;

                    case ValidationTypeEnum.User_To_EventProfessional:
                        ret = "custom_ValidateUserToEventProfessional";
                        break;

                    case ValidationTypeEnum.User_To_Charity:
                        ret = "custom_ValidateUserToCharity";
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
                case ValidationTypeEnum.User_To_Ambassador:
                    p.Add("@AmbassadorID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_AmbassadorBrand:
                    p.Add("@BrandID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_Sponsor:
                    p.Add("@SponsorID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_EventManager:
                    p.Add("@EventManagerID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_CarMeet:
                    p.Add("@CarMeetID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_CarClub:
                    p.Add("@CarClubID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_EventProfessional:
                    p.Add("@EventProfessionalID", ObjectID);
                    break;

                case ValidationTypeEnum.User_To_Charity:
                    p.Add("@CharityID", ObjectID);
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