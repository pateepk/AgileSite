using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Class for Property Panel Rows.
    /// </summary>
    [ToolboxData("<{0}:CategoryPanelRow runat=server></{0}:CategoryPanelRow>")]
    public class CategoryPanelRow : PlaceHolder
    {
        #region "Variables"

        private bool mGenerateDefaultCssClasses = true;
        private bool mShowFormLabelCell = true;
        private string mLabelTitle;
        private string mContextHelp;
        private string mErrorMessage;

        #endregion


        #region "Properties"

        /// <summary>
        /// The field is marked as required (red star).
        /// </summary>
        public bool IsRequired
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if FormLabelCell is to be rendered. Default value is true.
        /// </summary>
        public bool ShowFormLabelCell
        {
            get
            {
                return mShowFormLabelCell;
            }
            set
            {
                mShowFormLabelCell = value;
            }
        }


        /// <summary>
        /// Render generates default CSS classes.
        /// </summary>
        public bool GenerateDefaultCssClasses
        {
            get
            {
                return mGenerateDefaultCssClasses;
            }
            set
            {
                mGenerateDefaultCssClasses = value;
            }
        }


        /// <summary>
        /// Title of the field row.
        /// </summary>
        public string LabelTitle
        {
            get
            {
                return mLabelTitle;
            }
            set
            {
                //solve resource strings
                mLabelTitle = ResHelper.GetString(value);
            }
        }


        /// <summary>
        /// Client id of row.
        /// </summary>
        public string RowClientId
        {
            get
            {
                return ClientID;
            }
        }

        /// <summary>
        /// True if row is marked as display none.
        /// </summary>
        public bool HideRow
        {
            get;
            set;
        }

        /// <summary>
        /// Attribute for grouping rows.
        /// </summary>
        public string ItemGroup
        {
            get;
            set;
        }


        /// <summary>
        /// Context help for row.
        /// </summary>
        public string ContextHelp
        {
            get
            {
                if (String.IsNullOrEmpty(mContextHelp))
                {
                    return LabelTitle;
                }
                return mContextHelp;
            }
            set
            {
                mContextHelp = ResHelper.GetString(value);
            }
        }


        /// <summary>
        /// Errorr message for this row.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return mErrorMessage;
            }
            set
            {
                mErrorMessage = ResHelper.GetString(value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Render children event handler.
        /// </summary>
        protected override void RenderChildren(HtmlTextWriter writer)
        {
            // Render children in special order
            // so do nothing here
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CategoryPanelRow: " + ID + "]");
                return;
            }

            string editingFormRow = String.Empty;
            string editingFormLabelCell = String.Empty;
            string editingFormLabel = String.Empty;
            string editingFormValueCell = String.Empty;
            string errorLabel = String.Empty;

            if (GenerateDefaultCssClasses)
            {
                editingFormRow = String.Format(" class=\"EditingFormRow{0}\"", !String.IsNullOrEmpty(ItemGroup) ? " " + ItemGroup : String.Empty);
                editingFormLabelCell = " class=\"EditingFormLabelCell\"";
                editingFormLabel = " class=\"EditingFormLabel\"";
                editingFormValueCell = " class=\"EditingFormValueCell\"";
                errorLabel = " class=\"ErrorLabel\"";
            }

            // Check the visibility
            string hideRowString = HideRow ? " style=\"display:none\"" : String.Empty;

            

            // Render row envelope
            writer.Write(String.Format("<tr runat=\"server\" ID=\"{1}\"{0}{2}>", editingFormRow, ClientID, hideRowString));
            if (ShowFormLabelCell)
            {
                writer.Write(String.Format("<td{0}><label{1} title=\"{4}\">{2}{3}:</label></td>",
                    editingFormLabelCell, editingFormLabel, LabelTitle, IsRequired ? String.Format("<span class=\"required-mark\">{0}</span>", ResHelper.RequiredMark) : String.Empty, ContextHelp));
            }

            string errorLabelText = !String.IsNullOrEmpty(ErrorMessage) ? String.Format("<label{0} title=\"{1}\">{1}</label>", errorLabel, ErrorMessage) : String.Empty;

            writer.Write(String.Format("<td{0}>{1}", editingFormValueCell, errorLabelText));

            // Render child item filters
            RenderChildrenRow(writer);

            writer.Write("</td></tr>");
        }


        /// <summary>
        /// Renders children in specific time.
        /// </summary>
        /// <param name="writer">Text writer for rendering</param>
        private void RenderChildrenRow(HtmlTextWriter writer)
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.RenderControl(writer);
            }
        }

        #endregion
    }
}