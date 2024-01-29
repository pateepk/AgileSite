using System;

using CMS.UIControls;
using CMS.Helpers;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Base class for scoring pages.
    /// </summary>
    public class CMSScorePage : CMSDeskPage
    {
        #region "Properties"

        /// <summary>
        /// Score object.
        /// </summary>
        public ScoreInfo Score
        {
            get;
            private set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Page OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // This page is used in global application but inherits from CMSDesk page,
            // so a check whether site is available or not has to be disabled.
            RequireSite = false;

            CheckReadPermission();
        }


        /// <summary>
        /// Checks if user is eligible for reading scoring.
        /// </summary>
        protected virtual void CheckReadPermission()
        {
            // Check that user is authorized for the module
            if (!CurrentUser.IsAuthorizedPerResource("CMS.Scoring", "Read"))
            {
                RedirectToAccessDenied("CMS.Scoring", "Read");
            }

            Score = ScoreInfoProvider.GetScoreInfo(QueryHelper.GetInteger("scoreid", 0));
        }

        #endregion
    }
}