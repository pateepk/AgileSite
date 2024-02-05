﻿using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
//using CMS.PortalControls;
using CMS.SiteProvider;

using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFSwitchToUser : CMSAbstractWebPart
    {
        public enum ActionEnum { Unknown, SwitchUser };

        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
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

        public long CompanyIDToUse
        {
            get;
            set;
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
            if (!Page.IsPostBack)
            {
                string actionString = QueryHelper.GetString("action", "").ToLower();
                if (actionString == "switch")
                {
                    CompanyIDToUse = QueryHelper.GetInteger("companyid", 0);

                    if (CompanyIDToUse != 0)
                    {
                        switch (actionString)
                        {
                            case "switch":
                                
                                UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(CurrentUser.UserID);
                                userSettings.SetValue("UserCompany", CompanyIDToUse);
                                UserInfoProvider.SetUserInfo(CurrentUser);
                                
                                //ActionRequested = ActionEnum.SwitchUser;
                                break;
                        }
                        //RunAction();
                    }
                    else
                    {
                        lblMsg.Text = "CompanyIDToUse was not passed or was 0.";
                    }
                }
            }
        }
    }
}