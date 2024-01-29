using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form submit field.
    /// </summary>
    [ToolboxData("<{0}:FormSubmit runat=server></{0}:FormSubmit>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormSubmit : FormField
    {
        #region "Properties"

        /// <summary>
        /// Submit button.
        /// </summary>
        public FormSubmitButton SubmitButton
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates submit button in place of form control.
        /// </summary>
        /// <param name="parent">Parent control</param>
        protected override void LoadFormControl(Control parent)
        {
            // Create the submit button
            if (Form == null)
            {
                SubmitButton = new FormSubmitButton();
                SubmitButton.CausesValidation = false;
                SubmitButton.ID = "bOK";
            }
            else
            {
                SubmitButton = Form.SubmitButton;
            }
            SubmitButton.CssClass = CssHelper.EnsureClass(SubmitButton.CssClass, CssClass);

            parent.Controls.Add(SubmitButton);
        }

        #endregion
    }
}