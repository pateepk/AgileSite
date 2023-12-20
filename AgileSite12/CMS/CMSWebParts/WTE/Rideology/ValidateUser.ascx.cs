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
    public partial class ValidateUser : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// Sproc to use for validation
        /// </summary>
        public string ValidateSproc
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ValidateSproc"), "custom_ValidateUser");
            }
            set
            {
                this.SetValue("ValidateSproc", value);
            }
        }

        /// <summary>
        /// The object ID param
        /// </summary>
        public string ObjectIDParam
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectIDParam"), "ObjectID");
            }
            set
            {
                this.SetValue("ObjectIDParam_1", value);
            }
        }

        /// <summary>
        /// Param 1 name on the Sproc (associated to ObjectID 1)
        /// </summary>
        public string ObjectIDParam_1
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectIDParam_1"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectIDParam_1", value);
            }
        }

        /// <summary>
        /// Param 2 name on the Sproc (associated to ObjectID 2)
        /// </summary>
        public string ObjectIDParam_2
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectIDParam_2"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectIDParam_2", value);
            }
        }

        /// <summary>
        /// Param 3 name of the Sproc (associated to ObjectID 3)
        /// </summary>
        public string ObjectIDParam_3
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectIDParam_4"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectIDParam_4", value);
            }
        }

        /// <summary>
        /// Default ObjectID
        /// </summary>
        public string ObjectID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectID"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectID", value);
            }
        }

        /// <summary>
        /// The id of item being view or edit.
        /// </summary>
        public string ObjectID_1
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectID_1"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectID_1", value);
            }
        }

        /// <summary>
        /// The id of item being view or edit.
        /// </summary>
        public string ObjectID_2
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectID_2"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectID_2", value);
            }
        }

        /// <summary>
        /// The id of item being view or edit.
        /// </summary>
        public string ObjectID_3
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ObjectID_3"), String.Empty);
            }
            set
            {
                this.SetValue("ObjectID_3", value);
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

        #endregion "Properties"

        #region "Methods"

        /// <summary>
        /// Content loaded event handler. - Main functionality is in here
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();

            // only if visible
            if (Visible)
            {
                QueryDataParameters parameters = new QueryDataParameters();

                //custom parameters depending on validation type
                AddParams(parameters);

                //check for validity and redirect
                if (CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Edit &&
                    CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Design)
                {
                    if (!UserIsValid(parameters, ValidateSproc))
                    {
                        Response.Redirect(RedirectURL);
                    }
                }
            }
        }

        /// <summary>
        /// Get Clean sproc param name
        /// </summary>
        /// <param name="p_name"></param>
        /// <returns></returns>
        protected string GetObjectIDParamName(string p_name)
        {
            string ret = String.Empty;
            if (!String.IsNullOrWhiteSpace(p_name))
            {
                ret = p_name.Replace("@", String.Empty);
                if (!String.IsNullOrWhiteSpace(ret))
                {
                    ret = "@" + ret.Trim();
                }
                else
                {
                    ret = String.Empty;
                }
            }
            return ret;
        }

        protected void AddParams(QueryDataParameters p)
        {
            //we always need the user id
            UserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                p.Add("@UserID", user.UserID.ToString());
            }

            if (!String.IsNullOrWhiteSpace(ObjectIDParam) && !String.IsNullOrWhiteSpace(GetObjectIDParamName(ObjectID)))
            {
                p.Add(GetObjectIDParamName(ObjectIDParam), ObjectID);
            }

            if (!String.IsNullOrWhiteSpace(ObjectID_1) && !String.IsNullOrWhiteSpace(GetObjectIDParamName(ObjectIDParam_1)))
            {
                p.Add(GetObjectIDParamName(ObjectIDParam_1), ObjectID_1);
            }

            if (!String.IsNullOrWhiteSpace(ObjectID_2) && !String.IsNullOrWhiteSpace(GetObjectIDParamName(ObjectIDParam_2)))
            {
                p.Add(GetObjectIDParamName(ObjectIDParam_2), ObjectID_2);
            }

            if (!String.IsNullOrWhiteSpace(ObjectID_3) && !String.IsNullOrWhiteSpace(GetObjectIDParamName(ObjectIDParam_3)))
            {
                p.Add(GetObjectIDParamName(ObjectIDParam_3), ObjectID_3);
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