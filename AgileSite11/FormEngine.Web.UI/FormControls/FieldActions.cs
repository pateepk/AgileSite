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
        #region "Variables"

        private readonly BasicForm mBasicForm;
        private readonly FormFieldInfo mFieldInfo;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        /// <param name="form">Form where the control is used.</param>
        public FieldActions(FormFieldInfo ffi, BasicForm form)
        {
            mBasicForm = form;
            mFieldInfo = ffi;
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if ((mBasicForm != null) && (mFieldInfo != null))
            {
                // Add clone button
                var cloneButton = CreateCloneButton();
                Controls.Add(cloneButton);

                // Add remove button
                var removeButton = CreateRemoveButton();
                Controls.Add(removeButton);
            }
        }


        /// <summary>
        /// Creates button to clone fields.
        /// </summary>
        private CMSButton CreateCloneButton()
        {
            // Prepare confirmation message
            string fieldCaption = ResHelper.LocalizeString(mFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, mBasicForm.ContextResolver));
            string confirmation = String.IsNullOrEmpty(fieldCaption) ? ResHelper.GetString("FormBuilder.CloneSelectedFieldConfirmation") : String.Format(ResHelper.GetString("FormBuilder.CloneFieldConfirmation"), fieldCaption);

            // Create clone button
            CMSAccessibleButton cloneButton = new CMSAccessibleButton
            {
                ID = "CloneButton",
                IconOnly = true,
                IconCssClass = "icon-doc-copy",
                ToolTip = ResHelper.GetString("FormBuilder.CloneComponent"),
                OnClientClick = String.Format(@"if(confirm('{0}')) {{ FormBuilder.cloneField('{1}'); }}; return false;", confirmation, mFieldInfo.Name)
            };

            return cloneButton;
        }


        /// <summary>
        /// Creates button for removing fields.
        /// </summary>
        private CMSButton CreateRemoveButton()
        {
            // Prepare confirmation message
            string fieldCaption = ResHelper.LocalizeString(mFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, mBasicForm.ContextResolver));
            string confirmation = String.IsNullOrEmpty(fieldCaption) ? ResHelper.GetString("FormBuilder.DeleteSelectedFieldConfirmation") : String.Format(ResHelper.GetString("FormBuilder.DeleteFieldConfirmation"), fieldCaption);

            // Create remove button
            CMSAccessibleButton removeButton = new CMSAccessibleButton
            {
                ID = "RemoveButton",
                IconOnly = true,
                IconCssClass = "icon-bin",
                ToolTip = ResHelper.GetString("FormBuilder.RemoveComponent"),
                OnClientClick = String.Format(@"if(confirm('{0}')) {{ FormBuilder.removeField('{1}'); }}; return false;", confirmation, mFieldInfo.Name)
            };

            return removeButton;
        }

        #endregion
    }
}