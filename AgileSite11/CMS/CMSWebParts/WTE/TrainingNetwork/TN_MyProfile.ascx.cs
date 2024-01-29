using System;

using CMS.FormEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.WTE.TrainingNetwork
{
    public partial class TN_MyProfile : CMSAbstractWebPart
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets the name of alternative form
        /// Default value is cms.user.EditProfile
        /// </summary>
        public string AlternativeFormName
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("AlternativeFormName"), "cms.user.EditProfile");
            }
            set
            {
                SetValue("AlternativeFormName", value);
                myProfile.AlternativeFormName = value;
            }
        }

        /// <summary>
        /// Indicates if field visibility could be edited on user form.
        /// </summary>
        public bool AllowEditVisibility
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowEditVisibility"), myProfile.AllowEditVisibility);
            }
            set
            {
                SetValue("AllowEditVisibility", value);
                myProfile.AllowEditVisibility = value;
            }
        }


        /// <summary>
        /// Relative URL where user is redirected, after form content is successfully modified.
        /// </summary>
        public string AfterSaveRedirectURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AfterSaveRedirectURL"), String.Empty);
            }
            set
            {
                SetValue("AfterSaveRedirectURL", value);
            }
        }


        /// <summary>
        /// Submit button label. Valid input is a resource string. Default value is general.submit.
        /// </summary>
        public string SubmitButtonResourceString
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("SubmitButtonResourceString"), "general.submit");
            }
            set
            {
                SetValue("SubmitButtonResourceString", value);
            }
        }


        /// <summary>
        /// Displays required field mark next to field labels if fields are required. Default value is false.
        /// </summary>
        public bool MarkRequiredFields
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("MarkRequiredFields"), false);
            }
            set
            {
                SetValue("MarkRequiredFields", value);
            }
        }


        /// <summary>
        /// Displays colon behind label text in form. Default value is true.
        /// </summary>
        public bool UseColonBehindLabel
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseColonBehindLabel"), true);
            }
            set
            {
                SetValue("UseColonBehindLabel", value);
            }
        }


        /// <summary>
        /// Form CSS class.
        /// </summary>
        public string FormCSSClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FormCSSClass"), String.Empty);
            }
            set
            {
                SetValue("FormCSSClass", value);
            }
        }

        /// <summary>
        /// Self mode
        /// </summary>
        public bool SelfMode
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("SelfMode"), true);
            }
            set
            {
                SetValue("SelfMode", value);
            }
        }

        /// <summary>
        /// user guid as string
        /// </summary>
        public string UserGuid
        {
            get
            {
                //object guid = GetValue("UserGuid");
                //if (guid != null && !String.IsNullOrWhiteSpace(guid.ToString()))
                //{
                //    return ValidationHelper.GetGuid(GetValue("UserGuid"), Guid.Empty);
                //}
                //return null;
                string ret = ValidationHelper.GetString(GetValue("UserGuid"), String.Empty);
                return ValidationHelper.GetString(GetValue("UserGuid"), String.Empty);
            }
            set
            {
                SetValue("UserGuid", value);
            }
        }

        #endregion


        /// <summary>
        /// Content loaded event handler.
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }


        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (Visible)
            {
                myProfile.IsLiveSite = true;

                if (StopProcessing)
                {
                    myProfile.StopProcessing = true;
                }
                else
                {
                    // Get alternative form info
                    AlternativeFormInfo afi = AlternativeFormInfoProvider.GetAlternativeFormInfo(AlternativeFormName);
                    if (afi != null)
                    {
                        myProfile.SelfMode = SelfMode;
                        myProfile.UserGUID = UserGuid;
                        myProfile.AlternativeFormName = AlternativeFormName;
                        myProfile.AllowEditVisibility = AllowEditVisibility;
                        myProfile.AfterSaveRedirectURL = AfterSaveRedirectURL;
                        myProfile.SubmitButtonResourceString = SubmitButtonResourceString;
                        myProfile.FormCSSClass = FormCSSClass;
                        myProfile.MarkRequiredFields = MarkRequiredFields;
                        myProfile.UseColonBehindLabel = UseColonBehindLabel;
                        myProfile.ValidationErrorMessage = GetString("general.errorvalidationerror");
                    }
                    else
                    {
                        lblError.Text = String.Format(GetString("altform.formdoesntexists"), AlternativeFormName);
                        lblError.Visible = true;
                        plcContent.Visible = false;
                    }
                }
            }
        }


        /// <summary>
        /// Reload data.
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();
            SetupControl();
        }
    }
}