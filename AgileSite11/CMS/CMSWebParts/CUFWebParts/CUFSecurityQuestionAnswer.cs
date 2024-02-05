using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Controls;
using CMS.GlobalHelper;
using System.Data;
using CMS.SettingsProvider;
using CMS.CMSHelper;
using System.Text;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.DataEngine;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    /// <summary>
    /// Data container for security question and answers
    /// </summary>
    public class CUFSecurityQuestionAnswer
    {
        #region members

        private int? _questionID = null;
        private int? _answerID = null;
        private int? _userID = null;
        private int? _CUMemberID = null;
        private string _answer = String.Empty;
        private string _questionText = String.Empty;
        private string _encryptionKey = String.Empty;

        #endregion

        #region properties

        /// <summary>
        /// Security Question ID
        /// </summary>
        public int? QuestionID
        {
            get
            {
                return _questionID;
            }
            set
            {
                _questionID = value;
            }
        }

        /// <summary>
        /// The answer ID
        /// </summary>
        public int? AnswerID
        {
            get
            {
                return _answerID;
            }
            set
            {
                _answerID = value;
            }
        }

        /// <summary>
        /// Agile site user id
        /// </summary>
        public int? UserID
        {
            get
            {
                return _userID;
            }
            set
            {
                _userID = value;
            }
        }

        /// <summary>
        /// CU member id
        /// </summary>
        public int? CUMemberID
        {
            get
            {
                return _CUMemberID;
            }
            set
            {
                _CUMemberID = value;
            }
        }

        /// <summary>
        /// Answer
        /// </summary>
        public string Answer
        {
            get
            {
                return _answer;
            }
            set
            {
                _answer = value;
            }
        }

        /// <summary>
        /// The question text
        /// </summary>
        public string QuestionText
        {
            get
            {
                return _questionText;
            }
            set
            {
                _questionText = value;
            }
        }

        /// <summary>
        /// Encryption key
        /// </summary>
        public string EncryptionKey
        {
            get
            {
                return _encryptionKey;
            }
            set
            {
                _encryptionKey = value;
            }
        }

        /// <summary>
        /// Validate the answer
        /// </summary>
        public bool IsValidAnswer
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Answer);
            }
        }

        #endregion

        #region constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public CUFSecurityQuestionAnswer()
        {
        }

        /// <summary>
        /// Constructor fill data using data row
        /// </summary>
        /// <param name="p_row"></param>
        public CUFSecurityQuestionAnswer(DataRow p_row)
        {
            if (p_row != null)
            {
                if (p_row["QuestionID"] != null)
                {
                    int qid = 0;
                    if (int.TryParse(p_row["QuestionID"].ToString().Trim(), out qid))
                    {
                        _questionID = qid;
                    }
                }

                if (p_row["AnswerID"] != null)
                {
                    int aid = 0;
                    if (int.TryParse(p_row["AnswerID"].ToString().Trim(), out aid))
                    {
                        _answerID = aid;
                    }
                }

                if (p_row["UserID"] != null)
                {
                    int uid = 0;
                    if (int.TryParse(p_row["UserID"].ToString().Trim(), out uid))
                    {
                        _userID = uid;
                    }
                }

                if (p_row["CUMemberID"] != null)
                {
                    int mid = 0;
                    if (int.TryParse(p_row["CUMemberID"].ToString().Trim(), out mid))
                    {
                        _CUMemberID = mid;
                    }
                }

                if (p_row["Answer"] != null)
                {
                    _answer = p_row["Answer"].ToString();
                }

                if (p_row["QuestionText"] != null)
                {
                    _questionText = p_row["QuestionText"].ToString();
                }

                if (p_row["EncryptionKey"] != null)
                {
                    _encryptionKey = p_row["EncryptionKey"].ToString();
                }
            }
        }

        /// <summary>
        /// Fill data using a repeater item
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_userID"></param>
        /// <param name="p_siteID"></param>
        /// <param name="p_memberID"></param>
        public CUFSecurityQuestionAnswer(RepeaterItem p_item)
        {
            if (p_item != null)
            {
                HiddenField hfdQuestionGroupID = (HiddenField)p_item.FindControl("hfdQuestionGroupID");
                if (hfdQuestionGroupID == null)
                {
                    #region binding from the form repeater

                    HiddenField hfquestionid = (HiddenField)p_item.FindControl("hfQuestionid");
                    HiddenField hfanswerid = (HiddenField)p_item.FindControl("hfAnswerID");
                    TextBox txtAnswer = (TextBox)p_item.FindControl("txtSecurityAnswer");

                    if (hfanswerid != null)
                    {
                        int aid = 0;
                        if (int.TryParse(hfanswerid.Value.Trim(), out aid))
                        {
                            _answerID = aid;
                        }
                    }

                    if (hfquestionid != null)
                    {
                        int qid = 0;
                        if (int.TryParse(hfquestionid.Value.Trim(), out qid))
                        {
                            _questionID = qid;
                        }
                    }

                    if (txtAnswer != null)
                    {
                        _answer = txtAnswer.Text;
                    }

                    #endregion
                }
                else
                {
                    #region we are binding from drop down list repeater

                    DropDownList ddldQuestionDropdown = (DropDownList)p_item.FindControl("ddldQuestionDropdown");
                    TextBox txtdSecurityAnswer = (TextBox)p_item.FindControl("txtdSecurityAnswer");

                    if (ddldQuestionDropdown != null)
                    {
                        int qid = 0;
                        if (int.TryParse(ddldQuestionDropdown.SelectedValue, out qid))
                        {
                            _questionID = qid;
                        }
                    }

                    if (txtdSecurityAnswer != null)
                    {
                        _answer = txtdSecurityAnswer.Text;
                    }

                    #endregion
                }
            }
        }

        #endregion
    }
}