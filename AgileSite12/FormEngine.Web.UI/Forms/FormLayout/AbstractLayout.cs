using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Abstract class for form layout.
    /// </summary>
    public abstract class AbstractLayout
    {
        #region "Variables"

        /// <summary>
        /// Dictionary of the collapsible images indexed by category name
        /// </summary>
        protected StringSafeDictionary<CollapsibleImage> mCollapsibleImages = new StringSafeDictionary<CollapsibleImage>();
        private readonly HashSet<string> mInitializedLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        #endregion


        #region "Properties"

        /// <summary>
        /// Main form panel
        /// </summary>
        public Panel FormPanel
        {
            get
            {
                return BasicForm.FormPanel;
            }
        }


        /// <summary>
        /// Form
        /// </summary>
        protected BasicForm BasicForm
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets CSS class for the required mark (*)
        /// </summary>
        public string RequiredMarkCssClass
        {
            get;
            set;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Event representing control registration.
        /// </summary>
        public virtual event EventHandler OnAfterRegisterFormControl = delegate { };

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">Basic form with settings</param>
        protected AbstractLayout(BasicForm basicForm)
        {
            BasicForm = basicForm;
        }

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Initializes form layout.
        /// </summary>
        public abstract void LoadLayout();

        #endregion


        #region "Protected virtual methods"

        /// <summary>
        /// Creates the validation label for a field.
        /// </summary>
        /// <param name="ffi">Field info</param>
        protected virtual LocalizedLabel CreateErrorLabel(FormFieldInfo ffi)
        {
            LocalizedLabel ctrlErrorLabel = new LocalizedLabel
            {
                Visible = false,
                Text = String.Empty,
                ID = ffi.Name + "_lbe",
                CssClass = BasicForm.FieldErrorLabelCssClass,
                EnableViewState = false
            };

            // Add errorLabel to hashtable 'FieldErrorLabels'
            BasicForm.FieldErrorLabels[ffi.Name] = ctrlErrorLabel;

            FormPanel.Controls.Add(ctrlErrorLabel);

            return ctrlErrorLabel;
        }


        /// <summary>
        /// Creates the visibility control for a field.
        /// </summary>
        /// <param name="ffi">Field info</param>
        protected virtual FormEngineUserControl CreateVisibilityControl(FormFieldInfo ffi)
        {
            FormEngineUserControl ctrlVisibility = null;

            if ((ffi != null) && ffi.AllowUserToChangeVisibility)
            {
                // Get form user control object
                FormUserControlInfo ci = FormUserControlInfoProvider.GetFormUserControlInfo(ffi.VisibilityControl);
                if (ci != null)
                {
                    ctrlVisibility = FormUserControlLoader.LoadFormControl(BasicForm.Page, ffi.VisibilityControl, ffi.Name, loadDefaultProperties: false);

                    ctrlVisibility.ID = ffi.Name + "_Visibility";
                    ctrlVisibility.CssClass = BasicForm.FieldVisibilityCssClass;
                    ctrlVisibility.Value = ffi.Visibility;

                    // Add visibility control to hashtable 'FieldVisibilityControls'
                    BasicForm.FieldVisibilityControls[ffi.Name] = ctrlVisibility;

                    FormPanel.Controls.Add(ctrlVisibility);
                }
            }

            return ctrlVisibility;
        }



        /// <summary>
        /// Creates the field label.
        /// </summary>
        /// <param name="ffi">Field info</param>
        /// <param name="addTooltip">Indicates if tooltip will be added to the label</param>
        protected virtual LocalizedLabel CreateFieldLabel(FormFieldInfo ffi, bool addTooltip = true)
        {
            var ctrlFieldLabel = new LocalizedLabel
            {
                ID = string.Format("{0}_lb", ffi.Name),
                EnableViewState = false,
                DisplayColon = BasicForm.UseColonBehindLabel,
                CssClass = BasicForm.GetFieldCssClass(ffi, FormFieldPropertyEnum.CaptionCssClass),
                RequiredMarkCssClass = RequiredMarkCssClass
            };

            // Add caption CSS style
            string captionStyle = ffi.GetPropertyValue(FormFieldPropertyEnum.CaptionStyle, BasicForm.ContextResolver);
            if (!String.IsNullOrEmpty(captionStyle))
            {
                ctrlFieldLabel.Attributes.Add("style", captionStyle);
            }

            // Add label to hashtable 'FieldLabels'
            BasicForm.FieldLabels[ffi.Name] = ctrlFieldLabel;

            FormPanel.Controls.Add(ctrlFieldLabel);

            // Pospone init of label's caption and description which may depend on macro condition
            ctrlFieldLabel.Page.LoadComplete += (senderObj, eventArgs) =>
            {
                SetLabelCaption(ctrlFieldLabel, ffi, addTooltip);
            };

            // Fallback for cases when page load complete event was fired before label creation
            ctrlFieldLabel.PreRender += (senderObj, eventArgs) =>
            {
                SetLabelCaption(ctrlFieldLabel, ffi, addTooltip);
            };

            return ctrlFieldLabel;
        }


        private void SetLabelCaption(LocalizedLabel label, FormFieldInfo fieldInfo, bool addTooltip)
        {
            if (mInitializedLabels.Contains(fieldInfo.Name))
            {
                return;
            }

            var caption = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, BasicForm.ContextResolver);
            label.Text = caption;

            // Set required mark
            if (BasicForm.MarkRequiredFields && !fieldInfo.AllowEmpty && !string.IsNullOrEmpty(caption) && !IsExcludedRequiredField(fieldInfo))
            {
                label.ShowRequiredMark = true;
            }

            // Ensure upper case of the first letter
            if (BasicForm.EnsureFirstLetterUpperCase)
            {
                label.Text = TextHelper.FirstLetterToUpper(label.Text);
            }

            if (addTooltip)
            {
                // Add description
                label.ToolTip = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription, BasicForm.ContextResolver);
            }

            mInitializedLabels.Add(fieldInfo.Name);
        }


        /// <summary>
        /// Adds control to form panel.
        /// </summary>
        /// <param name="control">Control to add</param>
        /// <param name="parentControl">IField to which this control belongs to</param>
        protected virtual void AddControlToPanel(Control control, IDataDefinitionItem parentControl)
        {
            if (parentControl != null)
            {
                if (!BasicForm.AssociatedControls.ContainsKey(parentControl))
                {
                    BasicForm.AssociatedControls.Add(parentControl, new List<Control>());
                }
                BasicForm.AssociatedControls[parentControl].Add(control);
            }
            FormPanel.Controls.Add(control);
        }


        /// <summary>
        /// Gets collapsible image.
        /// </summary>
        protected CollapsibleImage CreateCollapsibleImage(FormCategoryInfo categoryInfo, string anchor, bool forceReload)
        {
            var collapsibleImage = new CollapsibleImage();

            collapsibleImage.CollapsedByDefault = ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.CollapsedByDefault, BasicForm.ContextResolver), false);
            collapsibleImage.ImageUrl = BasicForm.CollapseCategoryImageUrl;
            collapsibleImage.AlternateText = anchor;

            mCollapsibleImages[categoryInfo.CategoryName] = collapsibleImage;

            return collapsibleImage;
        }


        /// <summary>
        /// Creates new EditingFormControl and puts it into FieldEditingControls hash table.
        /// </summary>
        /// <returns>Created control</returns>
        protected virtual Control CreateEditingFormControl(FormFieldInfo ffi)
        {
            // Create new EditingFormControl and add it to controls library
            var efc = new EditingFormControl(ffi, BasicForm);

            efc.ID = ffi.Name + "_control";
            efc.AllowMacroEditing = BasicForm.AllowMacroEditing;
            efc.IsLiveSite = BasicForm.IsLiveSite;
            efc.Enabled = BasicForm.Enabled && ffi.Enabled;

            efc.EnsureControls();

            BasicForm.FieldControls[ffi.Name] = (FormEngineUserControl)efc.NestedControl;
            BasicForm.FieldEditingControls[ffi.Name] = efc;

            return efc;
        }


        /// <summary>
        /// Creates additional field action buttons and puts them into FieldActionsControls dictionary.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        /// <returns>Created control</returns>
        protected virtual Control CreateFieldActions(FormFieldInfo ffi)
        {
            FieldActions fa = new FieldActions(ffi, BasicForm);
            BasicForm.FieldActionsControls[ffi.Name] = fa;
            return fa;
        }


        /// <summary>
        /// Indicates if field is excluded from applying the required field format string.
        /// </summary>
        /// <param name="fieldInfo">Field info</param>
        protected virtual bool IsExcludedRequiredField(FormFieldInfo fieldInfo)
        {
            return FormHelper.IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.CheckBoxControl);
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Returns true if the given category is collapsed
        /// </summary>
        /// <param name="categoryName">Category name</param>
        public bool IsCategoryCollapsed(string categoryName)
        {
            CollapsibleImage img = mCollapsibleImages[categoryName];

            return ((img != null) && img.Collapsed);
        }

        #endregion
    }
}