using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace CMSApp.CMSWebParts.WTE.TrainingNetwork
{
    #region data classes

    /// <summary>
    /// Base for Quiz data container
    /// </summary>
    public class TNQuizDataContainerBase
    {
        #region methods

        /// <summary>
        /// Get Int value or null
        /// </summary>
        /// <param name="p_object"></param>
        /// <returns></returns>
        protected int? GetIntOrNull(object p_object)
        {
            int? ret = null;
            int val = 0;
            if (int.TryParse(GetStringOrNull(p_object), out val))
            {
                ret = val;
            }
            return ret;
        }

        /// <summary>
        /// Get string value or null
        /// </summary>
        /// <param name="p_object"></param>
        /// <returns></returns>
        protected string GetStringOrNull(object p_object)
        {
            string ret = String.Empty;
            if (p_object != null)
            {
                ret = p_object.ToString();
            }
            return ret;
        }

        #endregion methods
    }

    /// <summary>
    /// Data container for quiz answer
    /// </summary>
    public class TNQuizAnswer : TNQuizDataContainerBase
    {
        #region members

        private int _answerNumber = 0;
        private string _answerText = null;
        private bool _isCorrectAnswer = false;
        private bool _isSelected = false;
        private bool _hasAnswer = false;

        #endregion members

        #region properties

        /// <summary>
        /// The answer number
        /// </summary>
        public int AnswerNumber
        {
            get
            {
                return _answerNumber;
            }
            set
            {
                _answerNumber = value;
            }
        }

        /// <summary>
        /// The answer number value
        /// </summary>
        public string AnswerNumberString
        {
            get
            {
                return AnswerNumber.ToString();
            }
            set
            {
                int val = 0;
                if (int.TryParse(value, out val))
                {
                    AnswerNumber = val;
                }
                else
                {
                    AnswerNumber = 0;
                }
            }
        }

        /// <summary>
        /// The answer text
        /// </summary>
        public string AnswerText
        {
            get
            {
                return _answerText;
            }
            set
            {
                _answerText = value;
            }
        }

        /// <summary>
        /// Is this the correct answer
        /// </summary>
        public bool IsCorrectAnswer
        {
            get
            {
                return _isCorrectAnswer;
            }
            set
            {
                _isCorrectAnswer = value;
            }
        }

        /// <summary>
        /// Is this answer selected by the user
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
            }
        }

        /// <summary>
        /// Do we have an answer.
        /// </summary>
        public bool HasAnswer
        {
            get
            {
                return _hasAnswer;
            }
            set
            {
                _hasAnswer = value;
            }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TNQuizAnswer()
        {
        }

        /// <summary>
        /// The answer number
        /// </summary>
        /// <param name="p_answerNumber"></param>
        /// <param name="p_answerText"></param>
        public TNQuizAnswer(int p_answerNumber, string p_answerText)
        {
            _answerNumber = p_answerNumber;
            _answerText = p_answerText;
        }

        /// <summary>
        /// File the answer using the list item
        /// </summary>
        /// <param name="p_item"></param>
        public TNQuizAnswer(ListItem p_item)
        {
            if (p_item != null)
            {
                AnswerNumberString = p_item.Value;
                AnswerText = p_item.Text;
                IsSelected = p_item.Selected;
            }
        }

        #endregion constructor
    }

    /// <summary>
    /// Quiz Question
    /// </summary>
    public class TNQuizQuestion : TNQuizDataContainerBase
    {
        #region member

        private int _questionNumber = 0;
        private string _questionText = String.Empty;
        private int? _correctAnswer = null;
        private bool _hasQuestion = false;

        private List<TNQuizAnswer> _questionAnswers = new List<TNQuizAnswer>();

        #endregion member

        #region properties

        /// <summary>
        /// The group number
        /// </summary>
        public int QuestionNumber
        {
            get
            {
                return _questionNumber;
            }
            set
            {
                _questionNumber = value;
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
        /// The correct answer
        /// </summary>
        public int? CorrrectAnswer
        {
            get
            {
                return _correctAnswer;
            }
            set
            {
                _correctAnswer = value;
            }
        }

        /// <summary>
        /// Do we have data for this question
        /// </summary>
        public bool HasQuestion
        {
            get
            {
                return _hasQuestion;
            }
            set
            {
                _hasQuestion = value;
            }
        }

        /// <summary>
        /// Get the selected answer
        /// </summary>
        public TNQuizAnswer SelectedAnswer
        {
            get
            {
                TNQuizAnswer selectedAnswer = null;

                if (Answers.Count > 0)
                {
                    selectedAnswer = Answers.Find(delegate (TNQuizAnswer a)
                    {
                        return a.IsSelected;
                    });
                }

                return selectedAnswer;
            }
        }

        /// <summary>
        /// The question and answer
        /// </summary>
        public List<TNQuizAnswer> Answers
        {
            get
            {
                return _questionAnswers;
            }
            set
            {
                _questionAnswers = value;
            }
        }

        /// <summary>
        /// The count
        /// </summary>
        public int AnswerCount
        {
            get
            {
                return Answers.Count;
            }
        }

        #endregion properties

        #region method

        /// <summary>
        /// Add a question to the group
        /// </summary>
        /// <param name="p_answer"></param>
        public void Add(TNQuizAnswer p_answer)
        {
            if (p_answer != null)
            {
                _questionAnswers.Add(p_answer);
            }
        }

        /// <summary>
        /// Constructor fill data using data row
        /// </summary>
        /// <param name="p_questionNumber"></param>
        /// <param name="p_row"></param>
        public TNQuizQuestion(int p_questionNumber, DataRow p_row)
        {
            // the data row contains the quiz.
            if (p_row != null)
            {
                _questionNumber = p_questionNumber;
                if (p_row[GetQuestionColName(p_questionNumber)] != null)
                {
                    _questionText = p_row[GetQuestionColName(p_questionNumber)].ToString().Trim();
                    if (!String.IsNullOrWhiteSpace(_questionText))
                    {
                        _hasQuestion = true;
                    }
                }
                else
                {
                    _hasQuestion = false;
                }

                if (p_row[GetQuestionCorrectAnswerColName(p_questionNumber)] != null)
                {
                    int correctAnswer = 0;
                    if (int.TryParse(p_row[GetQuestionCorrectAnswerColName(p_questionNumber)].ToString().Trim(), out correctAnswer))
                    {
                        _correctAnswer = correctAnswer;
                    }
                }

                for (int answerNumber = 1; answerNumber <= 5; answerNumber++)
                {
                    bool hasAnswer = false;
                    string answerText = String.Empty;
                    if (p_row[GetQuestionAnswerColName(p_questionNumber, answerNumber)] != null)
                    {
                        answerText = p_row[GetQuestionAnswerColName(p_questionNumber, answerNumber)].ToString().Trim();
                        if (!String.IsNullOrWhiteSpace(answerText))
                        {
                            hasAnswer = true;
                        }
                    }
                    TNQuizAnswer answer = new TNQuizAnswer(answerNumber, answerText);
                    answer.HasAnswer = hasAnswer;
                    if (CorrrectAnswer.HasValue && answerNumber == CorrrectAnswer.Value)
                    {
                        answer.IsCorrectAnswer = true;
                    }
                    if (hasAnswer)
                    {
                        Add(answer);
                    }
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
        public TNQuizQuestion(RepeaterItem p_item)
        {
            if (p_item != null)
            {
                HiddenField hfQuestionNumber = (HiddenField)p_item.FindControl("hfQuestionNumber");
                if (hfQuestionNumber != null)
                {
                    int qid = 0;
                    if (int.TryParse(hfQuestionNumber.Value.Trim(), out qid))
                    {
                        _questionNumber = qid;
                        _hasQuestion = true;
                    }

                    RadioButtonList rblAnswers = (RadioButtonList)p_item.FindControl("rblAnswers");
                    if (rblAnswers != null)
                    {
                        foreach (ListItem item in rblAnswers.Items)
                        {
                            Add(new TNQuizAnswer(item));
                        }
                    }
                }
            }
        }

        #endregion method

        #region helper

        private string GetQuestionColName(int p_questionNumber)
        {
            return string.Format("Question{0}", p_questionNumber);
        }

        private string GetQuestionAnswerColName(int p_questionNumber, int p_answerNumber)
        {
            return string.Format("Q{0}Answer{1}", p_questionNumber, p_answerNumber);
        }

        private string GetQuestionCorrectAnswerColName(int p_questionNumber)
        {
            return string.Format("Q{0}CorrectAnswer", p_questionNumber);
        }

        #endregion helper
    }

    /// <summary>
    /// Security Question and Answer list
    /// </summary>
    public class TNQuiz : TNQuizDataContainerBase
    {
        #region member

        private List<TNQuizQuestion> _quizQuestions = new List<TNQuizQuestion>();

        private UserInfo _currentUserInfo = null;
        private int? _siteID = null;
        private int? _userID = null;
        private int? _quizAdminItemID = null;
        private string _quizAdminItemGUID = null;
        private string _quizID = null;

        private string _quizResultID = String.Empty;
        private string _quizResultGUID = String.Empty;

        #endregion member

        #region properties

        /// <summary>
        /// The current user info
        /// </summary>
        protected UserInfo CurrentUserInfo
        {
            get
            {
                return _currentUserInfo;
            }
            set
            {
                _currentUserInfo = value;
            }
        }

        /// <summary>
        /// The question and answer
        /// </summary>
        public List<TNQuizQuestion> QuizQuestions
        {
            get
            {
                return _quizQuestions;
            }
            set
            {
                _quizQuestions = value;
            }
        }

        /// <summary>
        /// The site ID
        /// </summary>
        public int? SiteID
        {
            get
            {
                return _siteID;
            }
            set
            {
                _siteID = value;
            }
        }

        /// <summary>
        /// Agilesite User ID
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
        /// The quiz admin item ID
        /// </summary>
        public int? QuizAdminItemID
        {
            get
            {
                return _quizAdminItemID;
            }
            set
            {
                _quizAdminItemID = value;
            }
        }

        /// <summary>
        /// The quiz admin guid
        /// </summary>
        public string QuizAdminItemGUID
        {
            get
            {
                return _quizAdminItemGUID;
            }
            set
            {
                _quizAdminItemGUID = value;
            }
        }

        /// <summary>
        /// The quiz id
        /// </summary>
        public string QuizID
        {
            get
            {
                return _quizID;
            }
            set
            {
                _quizID = value;
            }
        }

        /// <summary>
        /// The quiz result id
        /// </summary>
        public string QuizResultID
        {
            get
            {
                return _quizResultID;
            }
            set
            {
                _quizResultID = value;
            }
        }

        /// <summary>
        /// The quiz result guid
        /// </summary>
        public string QuizResultGUID
        {
            get
            {
                return _quizResultGUID;
            }
            set
            {
                _quizResultGUID = value;
            }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TNQuiz()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_userID"></param>
        /// <param name="p_cuMemberID"></param>
        public TNQuiz(UserInfo p_userInfo, int? p_quizAdminItemID, string p_quizAdminItemGUID, string p_quizID)
        {
            QuizAdminItemID = p_quizAdminItemID;

            if (!String.IsNullOrWhiteSpace(p_quizAdminItemGUID))
            {
                QuizAdminItemGUID = p_quizAdminItemGUID;
            }
            else
            {
                QuizAdminItemGUID = null;
            }

            if (!String.IsNullOrWhiteSpace(p_quizID))
            {
                QuizID = p_quizID;
            }
            else
            {
                QuizID = null;
            }

            BindUserInfo(p_userInfo);
            GetFromDB();
        }

        /// <summary>
        /// Create list of security questions and answers from a repeater
        /// </summary>
        /// <param name="p_userInfo"></param>
        /// <param name="p_repeater"></param>
        public TNQuiz(UserInfo p_userInfo, Repeater p_repeater, int? p_quizAdminItemID, string p_quizAdminItemGUID, string p_quizID)
        {
            BindUserInfo(p_userInfo);
            _quizAdminItemGUID = p_quizAdminItemGUID;
            _quizID = p_quizID;
            _quizAdminItemID = p_quizAdminItemID;
            GetFromRepeater(p_repeater);
        }

        #endregion constructor

        #region methods

        /// <summary>
        /// Clear list
        /// </summary>
        public void Clear()
        {
            _quizQuestions.Clear();
        }

        /// <summary>
        /// Add Questions to the collection
        /// </summary>
        /// <param name="p_question"></param>
        public void Add(TNQuizQuestion p_question)
        {
            if (p_question != null)
            {
                _quizQuestions.Add(p_question);
            }
        }

        /// <summary>
        /// Save the answers
        /// </summary>
        public void SaveResults()
        {
            Save(null);
        }

        /// <summary>
        /// Save the answers
        /// </summary>
        /// <param name="p_questions"></param>
        public void Save(List<TNQuizQuestion> p_questions)
        {
            if (p_questions == null)
            {
                SaveQuizResults(_quizQuestions);
            }
            else
            {
                SaveQuizResults(p_questions);
            }
        }

        /// <summary>
        /// Bind the user info
        /// </summary>
        /// <param name="p_ui"></param>
        private void BindUserInfo(UserInfo p_ui)
        {
            if (p_ui == null)
            {
                p_ui = UserInfoProvider.GetFullUserInfo(MembershipContext.AuthenticatedUser.UserID);
            }

            if (p_ui != null)
            {
                UserID = p_ui.UserID;
            }
        }

        /// <summary>
        /// Get data from a repeater
        /// </summary>
        /// <param name="p_repeater"></param>
        private void GetFromRepeater(Repeater p_repeater)
        {
            Clear();
            if (p_repeater != null)
            {
                foreach (RepeaterItem item in p_repeater.Items)
                {
                    TNQuizQuestion questions = new TNQuizQuestion(item);
                    if (questions != null)
                    {
                        Add(questions);
                    }
                }
            }
        }

        /// <summary>
        /// Get data from a dataset
        /// </summary>
        /// <param name="p_dataSet"></param>
        private void GetFromDB()
        {
            Clear();
            DataSet ds = GetQuestion();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    QuizAdminItemID = GetIntOrNull(row["ItemID"]);
                    QuizAdminItemGUID = GetStringOrNull(row["ItemGUID"]);
                    QuizID = GetStringOrNull(row["QuizID"]);
                    for (int questionNumber = 1; questionNumber <= 10; questionNumber++)
                    {
                        TNQuizQuestion question = new TNQuizQuestion(questionNumber, row);
                        if (question.HasQuestion)
                        {
                            Add(new TNQuizQuestion(questionNumber, row));
                        }
                    }
                }
            }
        }

        #region data access

        // [ItemID]
        //,[ItemCreatedBy]
        //,[ItemCreatedWhen]
        //,[ItemModifiedBy]
        //,[ItemModifiedWhen]
        //,[ItemOrder]
        //,[ItemGUID]
        //,[QuizID]
        //,[Question1]
        //,[Q1Answer1]
        //,[Q1Answer2]
        //,[Q1Answer3]
        //,[Q1Answer4]
        //,[Q1CorrectAnswer]
        //,[Question2]
        //,[Q2Answer1]
        //,[Q2Answer2]
        //,[Q2Answer3]
        //,[Q2Answer4]
        //,[Q2CorrectAnswer]
        //,[Question3]
        //,[Q3Answer1]
        //,[Q3Answer2]
        //,[Q3Answer3]
        //,[Q3Answer4]
        //,[Q3CorrectAnswer]
        //,[Question4]
        //,[Q4Answer1]
        //,[Q4Answer2]
        //,[Q4Answer3]
        //,[Q4Answer4]
        //,[Q4CorrectAnswer]
        //,[Question5]
        //,[Q5Answer1]
        //,[Q5Answer2]
        //,[Q5Answer3]
        //,[Q5Answer4]
        //,[Q5CorrectAnswer]
        //,[Question6]
        //,[Q6Answer1]
        //,[Q6Answer2]
        //,[Q6Answer3]
        //,[Q6Answer4]
        //,[Q6CorrectAnswer]
        //,[Question7]
        //,[Q7Answer1]
        //,[Q7Answer2]
        //,[Q7Answer3]
        //,[Q7Answer4]
        //,[Q7CorrectAnswer]
        //,[Question8]
        //,[Q8Answer1]
        //,[Q8Answer2]
        //,[Q8Answer3]
        //,[Q8Answer4]
        //,[Q8CorrectAnswer]
        //,[Question9]
        //,[Q9Answer1]
        //,[Q9Answer2]
        //,[Q9Answer3]
        //,[Q9Answer4]
        //,[Q9CorrectAnswer]
        //,[Question10]
        //,[Q10Answer1]
        //,[Q10Answer2]
        //,[Q10Answer3]
        //,[Q10Answer4]
        //,[Q1Answer5]
        //,[Q2Answer5]
        //,[Q3Answer5]
        //,[Q4Answer5]
        //,[Q5Answer5]
        //,[Q7Answer5]
        //,[Q6Answer5]
        //,[Q8Answer5]
        //,[Q9Answer5]
        //,[Q10Answer5]
        //,[Q10CorrectAnswer]

        /// <summary>
        /// Get the quiz questions
        /// </summary>
        /// <returns></returns>
        private DataSet GetQuestion()
        {
            //@QuizID NVARCHAR(25) = NULL,
            //@QuizAdminItemID INT = NULL ,
            //@ItemGUID UNIQUEIDENTIFIER = NULL

            QueryDataParameters qdp = new QueryDataParameters();
            //qdp.Add("QuizID", QuizID);
            qdp.Add("QuizID", QuizID);
            qdp.Add("QuizAdminItemID", QuizAdminItemID);
            qdp.Add("ItemGUID", QuizAdminItemGUID);
            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_customtable_QuizAdmin_GetQuestion", qdp, QueryTypeEnum.StoredProcedure);
            return ds;
        }

        /// <summary>
        /// Update the quiz question and answers
        /// </summary>
        /// <param name="p_questions"></param>
        private void SaveQuizResults(List<TNQuizQuestion> p_questions)
        {
            //@UserID INT = NULL ,
            //@QuizAdminItemID INT = NULL ,
            //@Q1UserAnswer INT = NULL ,
            //@Q2UserAnswer INT = NULL ,
            //@Q3UserAnswer INT = NULL ,
            //@Q4UserAnswer INT = NULL ,
            //@Q5UserAnswer INT = NULL ,
            //@Q6UserAnswer INT = NULL ,
            //@Q7UserAnswer INT = NULL ,
            //@Q8UserAnswer INT = NULL ,
            //@Q9UserAnswer INT = NULL ,
            //@Q10UserAnswer INT = NULL
            QueryDataParameters qdp = new QueryDataParameters();
            qdp.Add("UserID", _userID);
            qdp.Add("QuizAdminItemID", _quizAdminItemID);

            if (p_questions != null && p_questions.Count > 0)
            {
                foreach (TNQuizQuestion question in p_questions)
                {
                    string answerParam = GetQuestionUserAnswerColName(question.QuestionNumber);
                    int? selectedAnswer = null;
                    if (question.HasQuestion)
                    {
                        TNQuizAnswer answer = question.SelectedAnswer;
                        if (answer != null)
                        {
                            selectedAnswer = answer.AnswerNumber;
                        }
                    }
                    qdp.Add(answerParam, selectedAnswer);
                }
            }

            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_customtable_QuizResults_Insert", qdp, QueryTypeEnum.StoredProcedure);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                QuizResultID = GetStringOrNull(row["QuizResultID"]);
                QuizResultGUID = GetStringOrNull(row["QuizResultGUID"]);
            }
        }

        #endregion data access

        #endregion methods

        #region helper

        private string GetQuestionUserAnswerColName(int p_questionNumber)
        {
            return string.Format("Q{0}UserAnswer", p_questionNumber);
        }

        #endregion helper
    }

    #endregion data classes

    public partial class TN_QuizForm : CMSAbstractWebPart
    {
        #region members

        private string _quizResultID = String.Empty;
        private string _quizResultGUID = String.Empty;

        #endregion members

        #region properties

        /// <summary>
        /// Redirect URL on success
        /// </summary>
        protected string RedirectPageURL
        {
            get
            {
                return GetSafeStringValue("RedirectPageUrl", "/QuizResult");
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

        /// <summary>
        /// Quiz ID
        /// </summary>
        protected string QuizID
        {
            get
            {
                string quizid = String.Empty;
                if (Request.Params["quiz"] != null)
                {
                    if (!String.IsNullOrWhiteSpace(Request.Params["quiz"]))
                    {
                        quizid = Request.Params["quiz"];
                    }
                }
                return quizid;
            }
        }

        /// <summary>
        /// Item ID
        /// </summary>
        protected int? ItemID
        {
            get
            {
                int? itemID = null;
                int id = 0;
                if (Request.Params["itemid"] != null)
                {
                    if (int.TryParse(Request.Params["itemid"].ToString().Trim(), out id))
                    {
                        itemID = id;
                    }
                }
                return itemID;
            }
        }

        /// <summary>
        /// ITEM GUID
        /// </summary>
        protected string ItemGUID
        {
            get
            {
                string quizid = String.Empty;
                if (Request.Params["itemGuid"] != null)
                {
                    if (!String.IsNullOrWhiteSpace(Request.Params["itemGuid"]))
                    {
                        quizid = Request.Params["itemGuid"];
                    }
                }
                return quizid;
            }
        }

        /// <summary>
        /// The Amdin QuizItem ID
        /// </summary>
        protected int? AdminQuizItemID
        {
            get
            {
                int? id = null;
                string idString = hfAdminQuizItemID.Value;
                int value = 0;
                if (int.TryParse(idString, out value))
                {
                    id = value;
                }
                return id;
            }
            set
            {
                if (value.HasValue)
                {
                    hfAdminQuizItemID.Value = value.ToString();
                }
            }
        }

        /// <summary>
        /// The quiz result id
        /// </summary>
        public string QuizResultID
        {
            get
            {
                return _quizResultID;
            }
            set
            {
                _quizResultID = value;
            }
        }

        /// <summary>
        /// The quiz result guid
        /// </summary>
        public string QuizResultGUID
        {
            get
            {
                return _quizResultGUID;
            }
            set
            {
                _quizResultGUID = value;
            }
        }

        #endregion properties

        #region page event

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // When HttpCacheability is set to NoCache or ServerAndNoCache
            // the Expires HTTP header is set to -1 by default. This instructs
            // the client to not cache responses in the History folder. Thus,
            // each time you use the back/forward buttons, the client requests
            // a new version of the response.
            Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);

            Response.Cache.SetNoStore();

            // Override the ServerAndNoCache behavior by setting the SetAllowInBrowserHistory
            // method to true. This directs the client browser to store responses in
            // its History folder.
            Response.Cache.SetAllowResponseInBrowserHistory(false);

            if (!RequestHelper.IsPostBack())
            {
                BindQuiz();
            }
        }

        #endregion page event

        #region data binding

        /// <summary>
        /// Bind the Quiz
        /// </summary>
        protected void BindQuiz()
        {
            TNQuiz quiz = new TNQuiz(null, ItemID, QuizID, null);
            AdminQuizItemID = quiz.QuizAdminItemID;
            rptQAForm.DataSource = quiz.QuizQuestions;
            rptQAForm.DataBind();
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
                    TNQuizQuestion question = (TNQuizQuestion)e.Item.DataItem;
                    if (question != null)
                    {
                        RadioButtonList rblAnswer = (RadioButtonList)e.Item.FindControl("rblAnswers");
                        if (rblAnswer != null)
                        {
                            // bind the list
                            rblAnswer.DataSource = question.Answers;
                            rblAnswer.DataTextField = "AnswerText";
                            rblAnswer.DataValueField = "AnswerNumber";
                            rblAnswer.DataBind();
                        }
                    }
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
            try
            {
                TNQuiz quiz = new TNQuiz(null, rptQAForm, AdminQuizItemID, null, null);
                if (quiz != null)
                {
                    quiz.SaveResults();
                    QuizResultID = quiz.QuizResultID;
                    QuizResultGUID = quiz.QuizResultGUID;
                }

                if (!String.IsNullOrWhiteSpace(RedirectPageURL))
                {
                    //string redirUrl = String.Format("{0}?quiz={1}", RedirectPageURL, QuizID);
                    string redirUrl = String.Format("{0}?quiz={1}&result={2}", RedirectPageURL, QuizID, QuizResultGUID);
                    RedirectToURL(redirUrl);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Unable to save test result : " + ex.Message;
            }
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