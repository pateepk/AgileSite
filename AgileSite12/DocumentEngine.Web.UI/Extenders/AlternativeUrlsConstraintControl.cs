using System;
using System.Text.RegularExpressions;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Class for alternative url regex constraint text box form control.
    /// </summary>
    public class AlternativeUrlsConstraintControl : TextBoxControl
    {
        /// <summary>
        /// Textbox control
        /// </summary>
        protected override CMSTextBox TextBox { get; } = new CMSTextBox { ID = "txtText" };

        /// <summary>
        /// Returns true if provided value is a valid regex and pattern is not inefficient to compute.
        /// </summary>
        public override bool IsValid()
        {
            try
            {
                new Regex(TextBox.Text, RegexOptions.None, TimeSpan.FromSeconds(5d));
            }
            catch (RegexMatchTimeoutException)
            {
                ErrorMessage = GetString("formcontrol.alternativeurlsconstraintcontrol.regextimeout");
                return false;
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = string.Format(GetString("formcontrol.alternativeurlsconstraintcontrol.invalidregex"), HTMLHelper.HTMLEncode(ex.Message));
                return false;
            }
            
            return base.IsValid();
        }

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            Controls.Add(TextBox);
            base.OnInit(e);
        }
    }
}
