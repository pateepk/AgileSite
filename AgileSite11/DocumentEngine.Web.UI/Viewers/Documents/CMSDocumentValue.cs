using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSDocumentValue.
    /// </summary>
    public class CMSDocumentValue : WebControl
    {
        #region "Variables"

        private string mValue = "";

        #endregion


        #region "Properties"

        /// <summary>
        /// Stop processing.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Document list for what will be showed this control. Split by semicolon.
        /// </summary>
        public string ClassNames
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ClassNames"], "");
            }
            set
            {
                ViewState["ClassNames"] = value;
            }
        }


        /// <summary>
        /// Name of attribute.
        /// </summary>
        public string AttributeName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["AttributeName"], "");
            }
            set
            {
                ViewState["AttributeName"] = value;
            }
        }


        /// <summary>
        /// Formatting string.
        /// </summary>
        public string FormattingString
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FormattingString"], "");
            }
            set
            {
                ViewState["FormattingString"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnLoad function.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing)
            {
                return;
            }

            TreeNode node = DocumentContext.CurrentDocument;

            // Check if current document isn't null and is selected some attribute name
            if ((node == null) || (AttributeName == ""))
            {
                StopProcessing = true;
                return;
            }

            // Check class name
            if (ClassNames != "")
            {
                string classNames = ";" + ClassNames.ToLowerCSafe() + ";";
                if (classNames.IndexOfCSafe(node.NodeClassName.ToLowerCSafe()) < 0)
                {
                    StopProcessing = true;
                    return;
                }
            }

            // Get specified column from datarow
            mValue = ValidationHelper.GetString(node.GetValue(AttributeName), "");
        }


        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer.
        /// </summary>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            // There is no html opening tag to be rendered
        }


        /// <summary>
        /// Renders the HTML closing tag of the control into the specified writer.
        /// </summary>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            // There is no html closing tag to be rendered
        }


        /// <summary>
        /// Renders the contents of the control to the specified writer.
        /// </summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CMSDocumentValue: " + ID + "]");
                return;
            }

            base.RenderContents(writer);

            if (!StopProcessing)
            {
                string format = FormattingString;
                if (format == "")
                {
                    format = "{0}";
                }

                object value = mValue;

                if (ValidationHelper.IsInteger(mValue))
                {
                    value = Convert.ToInt32(mValue);
                }

                if (ValidationHelper.IsDouble(mValue))
                {
                    value = Convert.ToDouble(mValue);
                }

                writer.Write(format, value);
            }
        }

        #endregion
    }
}