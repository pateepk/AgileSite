using System.Collections.Generic;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class for workflow e-mail sending settings.
    /// </summary>
    public class WorkflowEmailSettings
    {
        #region "Private variables"

        private string mDefaultSubject = LocalizationHelper.GetString("general.nosubject");

        #endregion


        #region "Properties"

        /// <summary>
        /// Macro resolver to use for resolving macros in e-mail
        /// </summary>
        public MacroResolver Resolver
        {
            get;
            set;
        }


        /// <summary>
        /// List of e-mail recipients
        /// </summary>
        public List<string> Recipients
        {
            get;
            set;
        }


        /// <summary>
        /// Target workflow step
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail template name to use
        /// </summary>
        public string EmailTemplateName
        {
            get;
            set;
        }


        /// <summary>
        /// Default subject used when 'Subject' and selected template subject are empty.
        /// </summary>
        public string DefaultSubject
        {
            get
            {
                return mDefaultSubject;
            }
            set
            {
                mDefaultSubject = value;
            }
        }


        /// <summary>
        /// Subject with higher priority then subject from template
        /// </summary>
        public string Subject
        {
            get;
            set;
        }


        /// <summary>
        /// Body with higher priority then body from template
        /// </summary>
        public string Body
        {
            get;
            set;
        }


        /// <summary>
        /// Sender with higher priority then sender from template
        /// </summary>
        public string Sender
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if errors should be logged to event log
        /// </summary>
        public bool LogEvents 
        { 
            get; 
            set; 
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">User</param>
        public WorkflowEmailSettings(UserInfo user)
        {
            User = user;
        }

        #endregion
    }
}