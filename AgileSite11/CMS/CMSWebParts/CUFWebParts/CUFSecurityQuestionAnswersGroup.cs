using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    /// <summary>
    /// data container for security questions
    /// </summary>
    public class CUFSecurityQuestionAnswersGroup
    {
        #region member 

        private int _groupNumber = 0;
        private List<CUFSecurityQuestionAnswer> _questionAnswers = new List<CUFSecurityQuestionAnswer>();

        #endregion

        #region properties

        /// <summary>
        /// The group number
        /// </summary>
        public int GroupNumber
        {
            get
            {
                return _groupNumber;
            }
            set
            {
                _groupNumber = value;
            }
        }

        /// <summary>
        /// The question and answer
        /// </summary>
        public List<CUFSecurityQuestionAnswer> QuestionAnswers
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
        public int Count
        {
            get
            {
                return QuestionAnswers.Count;
            }
        }

        /// <summary>
        /// Get the first answer
        /// </summary>
        public CUFSecurityQuestionAnswer SelectedAnswer
        {
            get
            {
                CUFSecurityQuestionAnswer selectedAnswer = null;

                if (QuestionAnswers.Count > 0)
                {
                    selectedAnswer = QuestionAnswers.Find(delegate(CUFSecurityQuestionAnswer a)
                    {
                        return a.IsValidAnswer;
                    });

                    // return the first one
                    if (selectedAnswer == null)
                    {
                        if (QuestionAnswers.Count > 0)
                        {
                            selectedAnswer = QuestionAnswers[0];
                        }
                    }
                }

                return selectedAnswer;
            }
        }
        #endregion

        #region method

        /// <summary>
        /// Add a question to the group
        /// </summary>
        /// <param name="p_answer"></param>
        public void Add(CUFSecurityQuestionAnswer p_answer)
        {
            if (p_answer != null)
            {
                _questionAnswers.Add(p_answer);
            }
        }

        #endregion
    }
}