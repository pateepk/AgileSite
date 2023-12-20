using System;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class providing additional actions with fields.
    /// </summary>
    public class FieldActions : CompositeControl
    {
        #region "Variables and properties"

        private readonly BasicForm mBasicForm;
        private readonly FormFieldInfo mFieldInfo;

        private string fieldCaption;
        private bool captionResolved;


        private string FieldCaption
        {
            get
            {
                if (!captionResolved)
                {
                    fieldCaption = ResHelper.LocalizeString(mFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, mBasicForm.ContextResolver));

                    captionResolved = true;
                }

                return fieldCaption;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        /// <param name="form">Form where the control is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="ffi"/> or <paramref name="form"/> is <c>null</c></exception>
        public FieldActions(FormFieldInfo ffi, BasicForm form)
        {
            mBasicForm = form ?? throw new ArgumentNullException(nameof(form));
            mFieldInfo = ffi ?? throw new ArgumentNullException(nameof(ffi));
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Add clone button
            var cloneButton = CreateCloneButton();
            Controls.Add(cloneButton);

            // Add remove button
            var removeButton = CreateRemoveButton();
            Controls.Add(removeButton);
        }


        /// <summary>
        /// Creates button to clone fields.
        /// </summary>
        private CMSButton CreateCloneButton()
        {
            // Prepare confirmation message
            string confirmation = String.IsNullOrEmpty(fieldCaption) ? ResHelper.GetString("FormBuilder.CloneSelectedFieldConfirmation") : String.Format(ResHelper.GetString("FormBuilder.CloneFieldConfirmation"), FieldCaption);

            // Create clone button
            CMSAccessibleButton cloneButton = new CMSAccessibleButton
            {
                ID = "CloneButton",
                IconOnly = true,
                IconCssClass = "icon-doc-copy",
                ToolTip = ResHelper.GetString("FormBuilder.CloneComponent"),
                OnClientClick = $"if(confirm({ScriptHelper.GetString(confirmation, encodeNewLine: false)})) {{ FormBuilder.cloneField({ScriptHelper.GetString(mFieldInfo.Name)}); }}; return false;"
            };

            return cloneButton;
        }


        /// <summary>
        /// Creates button for removing fields.
        /// </summary>
        private CMSButton CreateRemoveButton()
        {
            // Prepare confirmation message
            string confirmation = String.IsNullOrEmpty(fieldCaption) ? ResHelper.GetString("FormBuilder.DeleteSelectedFieldConfirmation") : String.Format(ResHelper.GetString("FormBuilder.DeleteFieldConfirmation"), FieldCaption);

            // Create remove button
            CMSAccessibleButton removeButton = new CMSAccessibleButton
            {
                ID = "RemoveButton",
                IconOnly = true,
                IconCssClass = "icon-bin",
                ToolTip = ResHelper.GetString("FormBuilder.RemoveComponent"),
                OnClientClick = $"if(confirm({ScriptHelper.GetString(confirmation, encodeNewLine: false)})) {{ FormBuilder.removeField({ScriptHelper.GetString(mFieldInfo.Name)}); }}; return false;"
            };

            return removeButton;
        }

        #endregion
    }
}