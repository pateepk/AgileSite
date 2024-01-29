using System.ComponentModel;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// TextBoxWithLabel, inherited from CMSTextBox.
    /// </summary>
    [ToolboxItem(false)]
    public class TextBoxWithLabel : CMSTextBox
    {
        private string mLabelCssClass = string.Empty;


        /// <summary>
        /// CSS class for label.
        /// </summary>
        public string LabelCssClass
        {
            get
            {
                return mLabelCssClass;
            }
            set
            {
                mLabelCssClass = value ?? string.Empty;
            }
        }


        /// <summary>
        /// Label text.
        /// </summary>
        public string LabelText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["LabelText"], string.Empty);
            }
            set
            {
                ViewState["LabelText"] = value ?? string.Empty;
            }
        }


        /// <summary>
        /// Indicates if label has to be placed before text box.
        /// </summary>
        public bool LabelFirst
        {
            get;
            set;
        }


        /// <summary>
        /// Renders text box and label.
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Render base text box when label goes to the end
            if (!LabelFirst)
            {
                base.Render(writer);
            }

            string attributes = "";

            // Append css class attribute
            if (!string.IsNullOrEmpty(LabelCssClass))
            {
                attributes += string.Format("class=\"{0}\"", LabelCssClass);
            }

            // Render label
            writer.Write("<span {1}>{0}</span>", LabelText, attributes);

            // Render base text box at the end
            if (LabelFirst)
            {
                base.Render(writer);
            }
        }
    }
}