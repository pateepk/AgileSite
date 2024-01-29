using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Error transformation template class.
    /// </summary>
    public class CMSErrorTransformationTemplate : ITemplate
    {
        private readonly Exception mInnerException;
        private bool mFirst = true;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public CMSErrorTransformationTemplate(Exception innerException)
        {
            mInnerException = innerException;
        }


        /// <summary>
        /// Instantiates the error transformation object.
        /// </summary>
        /// <param name="container">Binding container</param>
        public void InstantiateIn(Control container)
        {
            CMSErrorTransformation tr = new CMSErrorTransformation();

            // Display error only in first instance
            if (mFirst)
            {
                mFirst = false;
                tr.InnerException = mInnerException;
            }

            container.Controls.Add(tr);
        }
    }


    /// <summary>
    /// Error transformation class.
    /// </summary>
    public class CMSErrorTransformation : CMSAbstractTransformation
    {
        private readonly Label mLblErrorMessage = new Label();


        /// <summary>
        /// Inner exception to display.
        /// </summary>
        public Exception InnerException
        {
            get;
            set;
        }


        /// <summary>
        /// Creates the child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (InnerException != null)
            {
                mLblErrorMessage.CssClass = "TransformationError";
                mLblErrorMessage.Text = HTMLHelper.HTMLEncode(InnerException.Message);
                mLblErrorMessage.ToolTip = HTMLHelper.HTMLEncode(EventLogProvider.GetExceptionLogMessage(InnerException));
                Controls.Add(mLblErrorMessage);
            }
        }
    }
}