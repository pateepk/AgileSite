using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Represents custom user layout specified by text with macros.
    /// </summary>
    public class LayoutForm : AbstractLayout
    {
        #region "Variables"

        private readonly StringBuilder mLayoutCtrls;
        private bool mSubmitButtonAlreadyUsed;


        /// <summary>
        /// Regular expression to match layout macros.
        /// </summary>
        private static readonly CMSRegex mRegExLayoutMacro = new CMSRegex("\\$\\$\\w+(?::(?:[A-Za-z]|_)(\\w|_)*|)\\$\\$");

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">Basic form</param>
        public LayoutForm(BasicForm basicForm)
            : base(basicForm)
        {
            mLayoutCtrls = new StringBuilder();
        }

        #endregion


        #region "Overriding methods"

        /// <summary>
        /// Loads form layout
        /// </summary>
        public override void LoadLayout()
        {
            // If no layout given, do not load
            if ((BasicForm.FormLayout == null) || (BasicForm.FormInformation == null))
            {
                return;
            }

            // Strip possible comments in layout
            BasicForm.FormLayout = HTMLHelper.RemoveComments(BasicForm.FormLayout);

            // Localize layout
            BasicForm.FormLayout = ResHelper.LocalizeString(BasicForm.FormLayout);

            // Read form layout definition from the beginning to the end
            MatchCollection matchColl = mRegExLayoutMacro.Matches(BasicForm.FormLayout);

            int actualPos = 0;
            foreach (Match match in matchColl)
            {
                GenerateMacroContent(match, ref actualPos);
            }

            AddNonMacroContent(actualPos, BasicForm.FormLayout.Length);

            // Resolve dynamic controls
            ControlsHelper.ResolveDynamicControls(FormPanel);

            LoadHiddenControls();

            AddSubmitButton();
        }

        #endregion


        #region "Protected virtual methods"

        /// <summary>
        /// Creates form content according to found macro.
        /// </summary>
        protected virtual void GenerateMacroContent(Match match, ref int actualPos)
        {
            int newPos = match.Index;
            AddNonMacroContent(actualPos, newPos);
            actualPos = newPos + match.Length;

            // Read macro value
            string macro = match.Value;
            macro = macro.Replace("$$", string.Empty);

            int colonPos = macro.IndexOfCSafe(":");
            string ffType;
            string ffName;
            if (colonPos < 0)
            {
                ffType = macro;
                ffName = string.Empty;
            }
            else
            {
                ffType = macro.Substring(0, colonPos);
                ffName = macro.Substring(colonPos + 1, macro.Length - colonPos - 1);
            }

            FormFieldInfo ffi = BasicForm.FormInformation.GetFormField(ffName);

            AddFormElement(ffi, ffType, ffName);
        }


        /// <summary>
        /// Adds other content from form layout into the form.
        /// </summary>
        /// <param name="positionStart">Start position in the layout</param>
        /// <param name="positionEnd">End position in the layout</param>
        protected virtual void AddNonMacroContent(int positionStart, int positionEnd)
        {
            if (positionStart < positionEnd)
            {
                string content = BasicForm.FormLayout.Substring(positionStart, positionEnd - positionStart);

                // Resolve macros
                content = BasicForm.ContextResolver.ResolveMacros(content);

                var ltl = new LiteralControl(URLHelper.MakeLinksAbsolute(content));
                FormPanel.Controls.Add(ltl);
            }
        }


        /// <summary>
        /// Adds control for the field according to specified type.
        /// </summary>
        protected virtual void AddFormElement(FormFieldInfo ffi, string ffType, string ffName)
        {
            if (ElementCanBeAdded(ffi, ffType))
            {
                switch (ffType)
                {
                    case "label":
                        AddControlToPanel(CreateFieldLabel(ffi), ffi);
                        break;

                    case "input":
                        AddControlToPanel(CreateEditingFormControl(ffi), ffi);
                        mLayoutCtrls.Append("control:" + ffName + ",");
                        break;

                    case "validation":
                        CreateErrorLabel(ffi);
                        break;

                    case "submitbutton":
                        if (BasicForm.ShowImageButton)
                        {
                            FormPanel.Controls.Add(BasicForm.SubmitImageButton);
                        }
                        else
                        {
                            FormPanel.Controls.Add(BasicForm.SubmitButton);
                        }
                        mSubmitButtonAlreadyUsed = true;
                        break;

                    case "visibility":
                        if (BasicForm.AllowEditVisibility)
                        {
                            CreateVisibilityControl(ffi);
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Indicates if element can be added into layout.
        /// </summary>
        protected virtual bool ElementCanBeAdded(FormFieldInfo ffi, string ffType)
        {
            return
                // Skip if there is no form field for layout element
                ((((ffi != null) && ffi.Visible) || (ffType.Equals("submitbutton", StringComparison.InvariantCultureIgnoreCase)) || (ffType.Equals("visibility", StringComparison.InvariantCultureIgnoreCase)))
                // Show only public fields or if ShowPrivateFields is true.
                 &&
                 ((ffType.Equals("submitbutton", StringComparison.InvariantCultureIgnoreCase)) || (ffType.Equals("visibility", StringComparison.InvariantCultureIgnoreCase)) || ((ffi != null) && ffi.PublicField) ||
                  (BasicForm.ShowPrivateFields))
                // Hide system fields if set
                 && ((ffi == null) || !(ffi.System && BasicForm.HideSystemFields)));
        }


        /// <summary>
        /// Inserts submit button to form.
        /// </summary>
        protected virtual void AddSubmitButton()
        {
            // Register submit button
            if (BasicForm.ShowImageButton && BasicForm.SubmitImageButton.Visible)
            {
                FormPanel.DefaultButton = BasicForm.SubmitImageButton.ID;
            }
            else if (BasicForm.SubmitButton.Visible)
            {
                FormPanel.DefaultButton = BasicForm.SubmitButton.ID;
            }

            // Add button if not already placed by form layout controls
            if (!mSubmitButtonAlreadyUsed)
            {
                if (BasicForm.ShowImageButton)
                {
                    FormPanel.Controls.Add(BasicForm.SubmitImageButton);
                }
                else
                {
                    FormPanel.Controls.Add(BasicForm.SubmitButton);
                }
            }
        }


        /// <summary>
        /// Loads hidden controls from layout.
        /// </summary>
        protected virtual void LoadHiddenControls()
        {
            var formFieldInfos = BasicForm.FormInformation.GetFields(true, false);
            string layoutControls = mLayoutCtrls.ToString();

            if ((formFieldInfos != null) && !string.IsNullOrEmpty(layoutControls))
            {
                foreach (FormFieldInfo ffi in formFieldInfos)
                {
                    if (!layoutControls.ToLowerCSafe().Contains("control:" + ffi.Name.ToLowerCSafe() + ","))
                    {
                        // Create invisible field control
                        ffi.Visible = false;
                        BasicForm.FieldsToHide.Add(ffi.Name);
                        Control ctrl = CreateEditingFormControl(ffi);
                        ctrl.Visible = false;
                        AddControlToPanel(ctrl, ffi);

                        // Create invisible field validation label if not present
                        if (!BasicForm.FieldErrorLabels.Contains(ffi.Name))
                        {
                            FormPanel.Controls.Add(CreateErrorLabel(ffi));
                        }
                    }
                }
            }
        }

        #endregion
    }
}