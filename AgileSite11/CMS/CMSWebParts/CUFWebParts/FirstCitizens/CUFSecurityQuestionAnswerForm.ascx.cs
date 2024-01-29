using System;
using System.Web.UI.WebControls;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    /// <summary>
    /// Security question/answer form
    /// </summary>
    public partial class CUFSecurityQuestionAnswerForm : CMSAbstractWebPart
    {
        #region properties

        #region custom data field from CMS

        /// <summary>
        /// Redirect URL on success
        /// </summary>
        protected string RedirectPageURL
        {
            get
            {
                return GetSafeStringValue("RedirectPageUrl", String.Empty);
            }
        }

        /// <summary>
        /// Minimum number of answer the user must answer
        /// </summary>
        protected int MinimumAnswerCount
        {
            get
            {
                return GetSafeIntValue("MinimumAnswerCount", 3);
            }
        }

        /// <summary>
        /// Use drop down version of the form
        /// </summary>
        protected bool UseDropDown
        {
            get
            {
                return GetSafeBoolValue("UseDropDown", true);
            }
        }

        #endregion custom data field from CMS

        #endregion properties

        #region page event

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!RequestHelper.IsPostBack())
            {
                BindSecurityQuestionAnswer();
            }
        }

        #endregion page event

        #region data binding

        /// <summary>
        /// Bind the repeater
        /// </summary>
        protected void BindSecurityQuestionAnswer()
        {
            //CUFSecurityQuestionAnswers answer = new CUFSecurityQuestionAnswers();
            //if (UseDropDown)
            //{
            //    rptQADropDown.DataSource = answer.QuestionAnswersGroup;
            //    rptQADropDown.DataBind();
            //}
            //else
            //{
            //    rptQAForm.DataSource = answer.QuestionAnswers;
            //    rptQAForm.DataBind();
            //}
        }

        /// <summary>
        /// Bind data item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptQAForm_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                if (e.Item != null)
                {
                    //CUFSecurityQuestionAnswer answer = (CUFSecurityQuestionAnswer)e.Item.DataItem;
                    //if (answer != null)
                    //{
                    //    #region rebind the info
                    //    HiddenField hfquestionid = (HiddenField)e.Item.FindControl("hfQuestionid");
                    //    HiddenField hfanswerid = (HiddenField)e.Item.FindControl("hfAnswerID");
                    //    TextBox txtAnswer = (TextBox)e.Item.FindControl("txtSecurityAnswer");
                    //    if (hfanswerid != null)
                    //    {
                    //        if (answer.AnswerID.HasValue)
                    //        {
                    //            hfanswerid.Value = answer.AnswerID.Value.ToString();
                    //        }
                    //    }

                    //    if (hfquestionid != null)
                    //    {
                    //        if (answer.QuestionID.HasValue)
                    //        {
                    //            hfquestionid.Value = answer.QuestionID.Value.ToString();
                    //        }
                    //    }

                    //    if (txtAnswer != null)
                    //    {
                    //        txtAnswer.Text = answer.Answer;
                    //    }

                    //    #endregion
                    //}
                }
            }
        }

        /// <summary>
        /// Bind data item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptQADropDown_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                if (e.Item != null)
                {
                    //CUFSecurityQuestionAnswersGroup group = (CUFSecurityQuestionAnswersGroup)e.Item.DataItem;
                    //if (group != null)
                    //{
                    //    #region rebind the info

                    //    DropDownList ddldQuestionDropdown = (DropDownList)e.Item.FindControl("ddldQuestionDropdown");
                    //    TextBox txtdSecurityAnswer = (TextBox)e.Item.FindControl("txtdSecurityAnswer");

                    //    if (ddldQuestionDropdown != null)
                    //    {
                    //        // bind the list
                    //        ddldQuestionDropdown.DataSource = group.QuestionAnswers;
                    //        ddldQuestionDropdown.DataTextField = "QuestionText";
                    //        ddldQuestionDropdown.DataValueField = "QuestionID";
                    //        ddldQuestionDropdown.DataBind();
                    //    }

                    //    #endregion
                    //}
                }
            }
        }

        #endregion data binding

        #region general events

        /// <summary>
        /// Saves data of edited user from TextBoxes into DB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    //    CUFSecurityQuestionAnswers answers = null;

            //    //    if (UseDropDown)
            //    //    {
            //    //        answers = new CUFSecurityQuestionAnswers(rptQADropDown);
            //    //    }
            //    //    else
            //    //    {
            //    //        answers = new CUFSecurityQuestionAnswers(rptQAForm);
            //    //    }

            //    //    if (answers.HasMinimumAnswers)
            //    //    {
            //    //        CUFSecurityQuestionAnswers.ResetSecurityQuestionAnswer();

            //    //        // save the new set of answers
            //    //        answers.Save();

            //    //        // add the cookie
            //    //        answers.SetCookie();

            //    //        // rebind the repeater
            //    //        BindSecurityQuestionAnswer();

            //    //        // redirect to the member page or return URL
            //    //        // Redirect user to the return url, or if is not defined redirect to the default target url
            //    //        string url = QueryHelper.GetString("ReturnURL", string.Empty);
            //    //        if (!String.IsNullOrWhiteSpace(url))
            //    //        {
            //    //            RedirectToURL(url);
            //    //        }
            //    //        else
            //    //        {
            //    //            // redirect to a specific page
            //    //            RedirectToURL(RedirectPageURL);
            //    //        }
            //    //    }
            //    //    else
            //    //    {
            //    //        // show error and return.
            //    //        lblError.Text = String.Format("Please answer at least {0} questions", answers.MinimumAnswerCount);
            //    //    }
            //    //}
            //    //catch (Exception ex)
            //    //{
            //    //    lblError.Text = "Unable to update : " + ex.Message;
            //    //}

            //    // make sure the user is authenticated
            //    //log in replicated user
            //    //AuthenticationHelper.AuthenticateUser(MembershipContext.AuthenticatedUser.UserName, false);
            //}
        }

        /// <summary>
        /// Button cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //RedirectToUrl(RedirectAfterCancelURL);
        }

        #endregion general events

        #region text helper

        /// <summary>
        /// Get string property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected string GetSafeStringValue(string p_key, string p_default)
        {
            string value = p_default;

            object obj = GetValue(p_key);

            if (obj != null)
            {
                value = obj.ToString();
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = p_default;
            }

            return value;
        }

        /// <summary>
        /// Get Bool property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected bool GetSafeBoolValue(string p_key, bool p_default)
        {
            bool value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!bool.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        /// <summary>
        /// Get int value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected int GetSafeIntValue(string p_key, int p_default)
        {
            int value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!int.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        #endregion text helper

        #region redirection

        /// <summary>
        /// Redirect to a page
        /// </summary>
        /// <param name="p_url"></param>
        protected void RedirectToURL(string p_url)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                URLHelper.Redirect(ResolveUrl(p_url));
            }
        }

        #endregion redirection
    }
}