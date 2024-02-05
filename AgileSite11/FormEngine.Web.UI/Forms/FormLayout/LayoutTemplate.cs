using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Represents layout based on a template.
    /// </summary>
    public class LayoutTemplate : AbstractLayout
    {
        #region "Events"

        /// <summary>
        /// Event representing control registration.
        /// </summary>
        public override event EventHandler OnAfterRegisterFormControl;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if a submit button was registered in ASCX layout.
        /// </summary>
        private bool SubmitButtonRegistered
        {
            get;
            set;
        }

        #endregion


        #region "Public methods and constructors"

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="basicForm">Reference to BasicForm</param>
        public LayoutTemplate(BasicForm basicForm)
            : base(basicForm)
        {
        }


        /// <summary>
        /// Loads the template layout for the form.
        /// </summary>
        public override void LoadLayout()
        {
            if ((BasicForm != null) && (BasicForm.LayoutTemplate != null))
            {
                // Init template
                BasicForm.LayoutTemplate.InstantiateIn(FormPanel);

                // Collect the form elements
                CollectFormElements(FormPanel, null);

                // Ensure the error labels
                EnsureErrorLabels();

                // Ensure the table layout if used
                EnsureLayout();

                // Ensure submit button in the form
                EnsureSubmitButton();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Collects form elements from the given container.
        /// </summary>
        /// <param name="container">Container to examine</param>
        /// <param name="fieldName">Field name provided by the parent container</param>
        /// <param name="useFFI">Indicates if form field information should be used to initialize inner form controls</param>
        protected virtual void CollectFormElements(Control container, string fieldName, bool useFFI = false)
        {
            // Process all controls
            foreach (Control child in container.Controls)
            {
                string currentFieldName = fieldName;

                // Register the control
                RegisterFormControl(child, currentFieldName, useFFI);

                // Ensure field name from encapsulating form field for inner controls
                if (child is FormField)
                {
                    currentFieldName = ((FormField)child).Field;
                    useFFI = ((FormField)child).UseFFI;
                }
                
                // Process inner controls
                if ((!(child is UserControl) || (child is CMSAbstractFormLayout)) && !(child is FormControl) && (child.Controls.Count > 0))
                {
                    CollectFormElements(child, currentFieldName, useFFI);
                }
            }
        }


        /// <summary>
        /// Registers the given form control within the form.
        /// </summary>
        /// <param name="child">Control to register</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="useFFI">Indicates if form field information should be used to initialize inner form controls</param>
        protected virtual void RegisterFormControl(Control child, string fieldName, bool useFFI = false)
        {
            if (child is FormField)
            {
                RegisterFormFieldControl((FormField)child, fieldName);
            }
            else if (child is EditingFormControl)
            {
                RegisterEditingFormControl((EditingFormControl)child, fieldName, useFFI);
            }
            else if (child is FormControl)
            {
                RegisterFormControlControl((FormControl)child, fieldName);
            }
            else if (child is FormEngineUserControl)
            {
                RegisterFormEngineUserControl((FormEngineUserControl)child, fieldName, useFFI);
            }
            else if (child is FormErrorLabel)
            {
                RegisterErrorLabel((FormErrorLabel)child, fieldName);
            }
            else if (child is FormInfoMessageLabel)
            {
                RegisterFormInfoMessageLabel((FormInfoMessageLabel)child);
            }
            else if (child is FormErrorMessageLabel)
            {
                RegisterFormErrorMessageLabel((FormErrorMessageLabel)child);
            }
            else if (child is FormLabel)
            {
                RegisterLabel((FormLabel)child, fieldName);
            }
            else if (child is FormSubmitButton)
            {
                RegisterSubmitButton((FormSubmitButton)child, fieldName);
            }
            else if (child is FormCategory)
            {
                RegisterCategory((FormCategory)child);
            }

            // Event after control registration
            if (OnAfterRegisterFormControl != null)
            {
                var args = new ControlRegistrationEventArgs(child, fieldName);
                OnAfterRegisterFormControl(this, args);
            }
        }


        /// <summary>
        /// Gets the name for the given field.
        /// </summary>
        /// <param name="parent">Parent field name</param>
        /// <param name="fieldName">Field name</param>
        protected virtual string GetFieldName(string parent, string fieldName)
        {
            return string.IsNullOrEmpty(fieldName) ? parent : fieldName;
        }


        /// <summary>
        /// Ensures the existence of the error labels for fields.
        /// </summary>
        protected virtual void EnsureErrorLabels()
        {
            // Load the controls
            foreach (DictionaryEntry item in BasicForm.FieldControls)
            {
                Control control = (Control)item.Value;
                string name = (string)item.Key;

                LocalizedLabel errorLabel = BasicForm.FieldErrorLabels[name];
                if (errorLabel == null)
                {
                    // Create new form error label and register it
                    errorLabel = new FormErrorLabel
                    {
                        ID = "e" + name
                    };

                    // Place the control next to the editing control
                    Control container = control.Parent;
                    container.Controls.AddAt(container.Controls.IndexOf(control) + 1, errorLabel);

                    // Register the control
                    RegisterFormControl(errorLabel, name);
                }
            }
        }


        /// <summary>
        /// Ensures the table layout.
        /// </summary>
        private void EnsureLayout()
        {
            if (BasicForm.DefaultFormLayout != FormLayoutEnum.Divs)
            {
                switch (BasicForm.DefaultFieldLayout)
                {
                    case FieldLayoutEnum.Default:
                    case FieldLayoutEnum.ThreeColumns:
                    case FieldLayoutEnum.TwoColumns:
                       
                    
                    if (BasicForm.FirstField != null && BasicForm.FirstField.Parent != null)
                    { 
                        // Table elements are stored in nested ControlCollection set
                        var fieldParent = BasicForm.FirstField.Parent;

                        // Columns layout is encapsulated into the table
                        int firstFieldIndex = fieldParent.Controls.IndexOf(BasicForm.FirstField);
                        fieldParent.Controls.AddAt(firstFieldIndex, new LiteralControl("<table class=\"EditingFormTable\">"));
                    
                        // Encapsulation is ensured for input closing tag on the last field index
                        int lastFieldIndex = fieldParent.Controls.IndexOf(BasicForm.LastField);
                        fieldParent.Controls.AddAt(lastFieldIndex + 1, new LiteralControl("</table>"));
                    }

                    break;
                }
            }
        }


        /// <summary>
        /// Ensures submit button in the form.
        /// </summary>
        private void EnsureSubmitButton()
        {
            if (SubmitButtonRegistered)
            {
                return;
            }

            Control button;

            if (BasicForm.ShowImageButton)
            {
                button = BasicForm.SubmitImageButton;
            }
            else
            {
                button = BasicForm.SubmitButton;
            }

            button.Load += ((sender, eventArgs) => { FormPanel.DefaultButton = button.UniqueID; });

            FormPanel.Controls.Add(button);
        }

        #endregion


        #region "Control registration methods"

        /// <summary>
        ///  Registers FormField into BasicForm controls.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        protected virtual void RegisterFormFieldControl(FormField child, string fieldName)
        {
            // Ensure field name
            string name = GetFieldName(fieldName, child.Field);

            // Load form info
            if (child.UseFFI)
            {
                FormFieldInfo fieldInfo = GetFFI(name);
                EnsureFieldVisibility(fieldInfo, child);

                child.FormFieldInfo = fieldInfo;
            }

            // Ensure form
            child.Field = name;
            child.Form = BasicForm;

            // Ensure controls
            child.EnsureControls();

            // Register control
            if (!String.IsNullOrEmpty(name) && (child.Layout != FieldLayoutEnum.Inline))
            {
                if ((BasicForm.Fields != null) && (!BasicForm.Fields.Contains(name)))
                {
                    BasicForm.Fields.Add(name);
                }
            }
        }


        /// <summary>
        ///  Registers EditingFormControl into BasicForm controls.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="useFFI">Indicates if form field information should be used to initialize inner form controls</param>
        protected virtual void RegisterEditingFormControl(EditingFormControl child, string fieldName, bool useFFI)
        {
            FormEngineUserControl ctrl = (FormEngineUserControl)child.NestedControl;

            string name = GetFieldName(fieldName, child.Field);

            if (useFFI)
            {
                // Ensure visibility
                FormFieldInfo fieldInfo = GetFFI(name);
                EnsureFieldVisibility(fieldInfo, child);
            }

            // Ensure form
            child.Field = name;
            child.Form = BasicForm;

            // Initialize inner control
            if (ctrl != null)
            {
                ctrl.Field = name;
                ctrl.Form = BasicForm;
            }

            // Register control
            if (!String.IsNullOrEmpty(name))
            {
                BasicForm.FieldEditingControls[name] = child;
                BasicForm.FieldControls[name] = ctrl;
                if ((BasicForm.Fields != null) && (!BasicForm.Fields.Contains(name)))
                {
                    BasicForm.Fields.Add(name);
                }
            }
        }


        /// <summary>
        ///  Registers FormControl into BasicForm controls.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        protected virtual void RegisterFormControlControl(FormControl child, string fieldName)
        {
            // Ensure field name
            string name = GetFieldName(fieldName, child.Field);

            // Load form info
            if (child.UseFFI)
            {
                FormFieldInfo fieldInfo = GetFFI(name);
                EnsureFieldVisibility(fieldInfo, child);

                child.FormFieldInfo = fieldInfo;
            }

            // Ensure form
            child.Field = name;
            child.Form = BasicForm;

            // Do not reset references in parent's (FormField) EditingControl property
            if (!child.InitCompleted || !(child.Parent is FormField && ((FormField)child.Parent).EditingControl == child.EditingControl))
            {
                // Reload control
                child.Reload();
            }

            // Register inner control
            if (child.EditingControl != null)
            {
                if (!String.IsNullOrEmpty(name))
                {
                    BasicForm.FieldControls[name] = child.EditingControl;
                    if ((BasicForm.Fields != null) && (!BasicForm.Fields.Contains(name)))
                    {
                        BasicForm.Fields.Add(name);
                    }
                }
            }
        }


        /// <summary>
        ///  Registers FormEngineUserControl into BasicForm controls.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="useFFI">Indicates if form field information should be used to initialize inner form controls</param>
        protected virtual void RegisterFormEngineUserControl(FormEngineUserControl child, string fieldName, bool useFFI)
        {
            // Ensure field name
            string name = GetFieldName(fieldName, child.Field);

            // Ensure form
            child.Field = name;
            child.Form = BasicForm;

            if (useFFI)
            {
                // Ensure visibility
                FormFieldInfo fieldInfo = GetFFI(name);
                EnsureFieldVisibility(fieldInfo, child);
            }
            else if ((BasicForm != null) && !String.IsNullOrEmpty(name) && !RequestHelper.IsPostBack())
            {
                child.Value = BasicForm.GetDataValue(name);
            }

            // Register control
            if (!String.IsNullOrEmpty(name))
            {
                BasicForm.FieldControls[name] = child;
                if ((BasicForm.Fields != null) && (!BasicForm.Fields.Contains(name)))
                {
                    BasicForm.Fields.Add(name);
                }
            }
        }


        /// <summary>
        /// Registers error label for specified field in BasicForm.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        protected virtual void RegisterErrorLabel(FormErrorLabel child, string fieldName)
        {
            // Ensure field name
            string name = GetFieldName(fieldName, child.Field);
            child.Field = name;

            // Field error label
            if (!String.IsNullOrEmpty(name))
            {
                BasicForm.FieldErrorLabels[name] = child;
            }
        }


        /// <summary>
        /// Registers info label for specified control.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        protected virtual void RegisterLabel(FormLabel child, string fieldName)
        {
            // Ensure field name
            string name = GetFieldName(fieldName, child.Field);

            child.Field = name;
            child.Form = BasicForm;

            // Field label
            if (!String.IsNullOrEmpty(name))
            {
                BasicForm.FieldLabels[name] = child;
            }
        }


        /// <summary>
        /// Registers message info label for whole form.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        protected virtual void RegisterFormInfoMessageLabel(FormInfoMessageLabel child)
        {
            // Info label
            BasicForm.InfoLabel = child;

            if (String.IsNullOrEmpty(child.CssClass))
            {
                child.CssClass = "InfoLabel";
            }
        }


        /// <summary>
        /// Registers message error label for whole form.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        protected virtual void RegisterFormErrorMessageLabel(FormErrorMessageLabel child)
        {
            // Error label
            BasicForm.ErrorLabel = child;

            if (String.IsNullOrEmpty(child.CssClass))
            {
                child.CssClass = "ErrorLabel";
            }
        }


        /// <summary>
        /// Registers BasicForm's submit button.
        /// </summary>
        /// <param name="child">Control to be registered</param>
        /// <param name="fieldName">Field name</param>
        protected virtual void RegisterSubmitButton(FormSubmitButton child, string fieldName)
        {
            // Register the form button
            child.CausesValidation = false;
            child.Form = BasicForm;
            child.Load += ((sender, eventArgs) => { FormPanel.DefaultButton = child.UniqueID; });

            // Register button
            BasicForm.SubmitButton = child;

            // Set flag
            SubmitButtonRegistered = true;
        }


        /// <summary>
        /// Registers BasicForm's form category.
        /// </summary>
        /// <param name="child">Category to be registered</param>
        protected virtual void RegisterCategory(FormCategory child)
        {
            if (child != null && BasicForm != null && BasicForm.FormInformation != null)
            {
                child.Form = BasicForm;

                // Get first field name from category's FormFields or FormControls
                string firstFieldName = FindFieldNameInCategory(child);
                if (!String.IsNullOrEmpty(firstFieldName))
                {
                    // Get index of the found field in form information list
                    int fieldIndex = BasicForm.FormInformation.ItemsList.IndexOf(BasicForm.FormInformation.GetFormField(firstFieldName));

                    FormCategoryInfo formCategory = new FormCategoryInfo();

                    // Add new "dummy" category into form information
                    BasicForm.FormInformation.AddFormCategory(formCategory, (fieldIndex > 0 ? fieldIndex : 0));

                    // Associate the category control with the "dummy" category
                    BasicForm.AssociatedControls.Add(formCategory, new List<Control> { child });
                }
            }
        }


        /// <summary>
        /// Returns first field name from one of the child controls of specified category control.
        /// Returns NULL if no field name found.
        /// </summary>
        /// <param name="parent">Parent control</param>
        private string FindFieldNameInCategory(Control parent)
        {
            string fieldname = null;
            foreach (Control child in parent.Controls)
            {
                if (child is FormField)
                {
                    fieldname = ((FormField)child).Field;
                }
                else if (child is FormControl)
                {
                    fieldname = ((FormControl)child).Field;
                }

                if (!String.IsNullOrEmpty(fieldname))
                {
                    break;
                }
                else if (child.Controls.Count > 0 && !(child is FormCategory))
                {
                    fieldname = FindFieldNameInCategory(child);
                    if (!String.IsNullOrEmpty(fieldname))
                    {
                        break;
                    }
                }
            }

            return fieldname;
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Gets FormFieldInfo for given field name.
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        private FormFieldInfo GetFFI(string fieldName)
        {
            if ((fieldName != null) && (BasicForm != null) && (BasicForm.FormInformation != null))
            {
                return BasicForm.FormInformation.GetFormField(fieldName);
            }

            return null;
        }


        /// <summary>
        /// Checks if given field can be displayed in the form layout based on form settings (may affect private and system fields).
        /// </summary>
        /// <param name="fieldInfo">Form field info</param>
        /// <param name="control">Field control</param>
        private void EnsureFieldVisibility(FormFieldInfo fieldInfo, Control control)
        {
            if ((fieldInfo != null) && !FieldCanBeAdded(fieldInfo))
            {
                // Hide the field
                BasicForm.FieldsToHide.Add(fieldInfo.Name);
                control.Visible = false;
                fieldInfo.Visible = false;
            }
        }


        /// <summary>
        /// Returns TRUE if a field can be added into layout.
        /// </summary>
        private bool FieldCanBeAdded(FormFieldInfo ffi)
        {
            // Skip if there is no form field for layout element
            return (ffi == null) || ((ffi.Visible) &&
                // Show only public fields or if ShowPrivateFields is true.
                ((ffi.PublicField) || BasicForm.ShowPrivateFields)
                // Hide system fields if set
                && (!(ffi.System && BasicForm.HideSystemFields)));
        }

        #endregion
    }
}