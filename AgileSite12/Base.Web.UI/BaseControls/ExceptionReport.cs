using System;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Displays an exception report
    /// </summary>
    public class ExceptionReport : CMSWebControl
    {
        #region "Variables"

        private static int mMaxLength = 1000;

        #endregion


        #region "Properties"

        /// <summary>
        /// Exception to display
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }


        /// <summary>
        /// Error title
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum length of the message displayed
        /// </summary>
        public static int MaxLength
        {
            get
            {
                return mMaxLength;
            }
            set
            {
                mMaxLength = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ex">Exception to report</param>
        /// <param name="title">Error title</param>
        public ExceptionReport(Exception ex, string title)
        {
            Title = title;
            Exception = ex;
        }


        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            var title = TextHelper.LimitLength(Title, MaxLength, null , true, CutTextEnum.Middle);
            string content = String.Format("<div class=\"WebPartError\"><div class=\"WebPartErrorTitle\">{0}</div></div>", title);

            writer.Write(content);
        }
        
        #endregion
    }
}
