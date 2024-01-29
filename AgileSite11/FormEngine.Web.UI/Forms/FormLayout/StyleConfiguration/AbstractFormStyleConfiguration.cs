namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Container for layout specific form style configuration.
    /// </summary>
    public abstract class AbstractFormStyleConfiguration : IFormStyleConfiguration
    {
        #region "IFormStyleConfiguration properties"

        /// <summary>
        /// CSS class which will be used to wrap form control.
        /// </summary>
        public string FormCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the whole field (usually row).
        /// </summary>
        public string FieldCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the whole field group with heading.
        /// </summary>
        public string GroupCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the whole field group.
        /// </summary>
        public string FieldGroupCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the field group caption.
        /// </summary>
        public string FieldGroupCaptionCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field control cell.
        /// </summary>
        public string FieldValueCellCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field label cell.
        /// </summary>
        public string FieldCaptionCellCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field label.
        /// </summary>
        public string FieldCaptionCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field error label cell.
        /// </summary>
        public string FieldErrorCellCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field error label.
        /// </summary>
        public string FieldErrorLabelCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class that is added to editing form control after its validation failed.
        /// </summary>
        public string FieldErrorCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field visibility cell.
        /// </summary>
        public string FieldVisibilityCellCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the individual field visibility control.
        /// </summary>
        public string FieldVisibilityCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class that is added to container that is wrapping content after text (only if content before text is empty).
        /// </summary>
        public string ExplanationTextCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the form button.
        /// </summary>
        public string FormButtonCssClass
        {
            get;
            protected set;
        }


        /// <summary>
        /// CSS class for the form button panel.
        /// </summary>
        public string FormButtonPanelCssClass
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Copy constructor used to copy form style configuration to layout style so it can be merged.
        /// </summary>
        /// <param name="formStyle">Form style configuration</param>
        protected AbstractFormStyleConfiguration(IFormStyleConfiguration formStyle)
        {
            if (formStyle != null)
            {
                FormCssClass = formStyle.FormCssClass;
                FieldCssClass = formStyle.FieldCssClass;
                GroupCssClass = formStyle.GroupCssClass;
                FieldGroupCssClass = formStyle.FieldGroupCssClass;
                FieldGroupCaptionCssClass = formStyle.FieldGroupCaptionCssClass;
                FieldValueCellCssClass = formStyle.FieldValueCellCssClass;
                FieldCaptionCellCssClass = formStyle.FieldCaptionCellCssClass;
                FieldCaptionCssClass = formStyle.FieldCaptionCssClass;
                FieldErrorCellCssClass = formStyle.FieldErrorCellCssClass;
                FieldErrorLabelCssClass = formStyle.FieldErrorLabelCssClass;
                FieldErrorCssClass = formStyle.FieldErrorCssClass;
                FieldVisibilityCellCssClass = formStyle.FieldVisibilityCellCssClass;
                FieldVisibilityCssClass = formStyle.FieldVisibilityCssClass;
                ExplanationTextCssClass = formStyle.ExplanationTextCssClass;
                FormButtonCssClass = formStyle.FormButtonCssClass;
                FormButtonPanelCssClass = formStyle.FormButtonPanelCssClass;
            }
        }

        #endregion
    }
}
