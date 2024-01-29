using CMS.FormEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds form control API examples.
    /// </summary>
    /// <pageTitle>Form controls</pageTitle>
    internal class FormControls
    {
        /// <heading>Creating a new form control</heading>
        private void CreateFormControl()
        {
            // Creates a new form control object
            FormUserControlInfo newControl = new FormUserControlInfo();

            // Sets the form control properties
            newControl.UserControlDisplayName = "New control";
            newControl.UserControlCodeName = "NewControl";
            newControl.UserControlFileName = "~/CMSFormControls/Basic/TextBoxControl.ascx";
            newControl.UserControlType = FormUserControlTypeEnum.Input;
            newControl.UserControlForText = true;

            // Saves the form control to the database
            FormUserControlInfoProvider.SetFormUserControlInfo(newControl);
        }


        /// <heading>Updating an existing form control</heading>
        private void GetAndUpdateFormControl()
        {
            // Gets the form control
            FormUserControlInfo updateControl = FormUserControlInfoProvider.GetFormUserControlInfo("NewControl");
            if (updateControl != null)
            {
                // Updates the form control properties
                updateControl.UserControlDisplayName = updateControl.UserControlDisplayName.ToLower();

                // Saves the changes to the database
                FormUserControlInfoProvider.SetFormUserControlInfo(updateControl);
            }
        }


        /// <heading>Updating multiple form controls</heading>
        private void GetAndBulkUpdateFormControls()
        {
            // Loads the form controls whose code name starts with 'NewControl'
            var formControls = FormUserControlInfoProvider.GetFormUserControls().WhereStartsWith("UserControlCodeName", "NewControl");

            // Loops through the form controls
            foreach (FormUserControlInfo modifyControl in formControls)
            {
                // Updates the properties of the form control
                modifyControl.UserControlDisplayName = modifyControl.UserControlDisplayName.ToUpper();

                // Saves the changes to the database
                FormUserControlInfoProvider.SetFormUserControlInfo(modifyControl);
            }            
        }


        /// <heading>Deleting a form control</heading>
        private void DeleteFormControl()
        {
            // Gets the form control
            FormUserControlInfo deleteControl = FormUserControlInfoProvider.GetFormUserControlInfo("NewControl");

            // Deletes the form control
            FormUserControlInfoProvider.DeleteFormUserControlInfo(deleteControl);
        }
    }
}
