using CMS.OnlineForms;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples that show how to manage data submitted through on-line forms.
    /// </summary>
    /// <pageTitle>Form data</pageTitle>
    internal class FormData
    {
        /// <heading>Adding data records to a form</heading>
        private void CreateFormItem()
        {
            // Gets the form object representing the 'ContactUs' form on the current site
            BizFormInfo formObject = BizFormInfoProvider.GetBizFormInfo("ContactUs", SiteContext.CurrentSiteID);

            if (formObject != null)
            {
                // Gets the class name of the 'ContactUs' form
                DataClassInfo formClass = DataClassInfoProvider.GetDataClassInfo(formObject.FormClassID);
                string formClassName = formClass.ClassName;

                // Creates a new data record for the form
                BizFormItem newFormItem = BizFormItem.New(formClassName);

                // Sets the values for the form's fields (UserMessage in this case)
                newFormItem.SetValue("UserMessage", "This is a message submitted through the API.");

                // Saves the new form record into the database
                // Set values for all 'Required' fields in the form before calling the Insert method, otherwise an exception will occur
                newFormItem.Insert();
            }
        }


        /// <heading>Loading data records from a form</heading>
        private void GetFormItems()
        {
            // Gets the form object representing the 'ContactUs' form on the current site
            BizFormInfo formObject = BizFormInfoProvider.GetBizFormInfo("ContactUs", SiteContext.CurrentSiteID);

            if (formObject != null)
            {
                // Gets the class name of the 'ContactUs' form
                DataClassInfo formClass = DataClassInfoProvider.GetDataClassInfo(formObject.FormClassID);
                string formClassName = formClass.ClassName;

                // Loads the form's data
                ObjectQuery<BizFormItem> data = BizFormItemProvider.GetItems(formClassName);

                // Loops through the form's data records
                foreach (BizFormItem item in data)
                {
                    // Gets the values of the 'UserEmail' and 'UserMessage' text fields for the given data record
                    string emailFieldValue = item.GetStringValue("UserEmail", "");
                    string messageFieldValue = item.GetStringValue("UserMessage", "");
                }
            }
        }


        /// <heading>Updating the data records of a form</heading>
        private void UpdateFormItems()
        {
            // Gets the form object representing the 'ContactUs' form on the current site
            BizFormInfo formObject = BizFormInfoProvider.GetBizFormInfo("ContactUs", SiteContext.CurrentSiteID);

            if (formObject != null)
            {
                // Gets the class name of the 'ContactUs' form
                DataClassInfo formClass = DataClassInfoProvider.GetDataClassInfo(formObject.FormClassID);
                string formClassName = formClass.ClassName;

                // Loads all data records from the form whose 'UserEmail' field ends with the ".net" suffix
                ObjectQuery<BizFormItem> data = BizFormItemProvider.GetItems(formClassName)
                                                                        .WhereEndsWith("UserEmail", ".net");

                // Loops through the form's data records
                foreach (BizFormItem item in data)
                {
                    // Assigns and saves a value for the form record's 'InternalNote' field
                    item.SetValue("InternalNote", "This note was added by the system.");
                    item.SubmitChanges(false);
                }
            }
        }


        /// <heading>Deleting form data records</heading>
        private void DeleteFormItems()
        {
            // Gets the form object representing the 'ContactUs' form on the current site
            BizFormInfo formObject = BizFormInfoProvider.GetBizFormInfo("ContactUs", SiteContext.CurrentSiteID);

            if (formObject != null)
            {
                // Gets the class name of the 'ContactUs' form
                DataClassInfo formClass = DataClassInfoProvider.GetDataClassInfo(formObject.FormClassID);
                string formClassName = formClass.ClassName;

                // Loads all data records from the form that have an empty value in the 'UserMessage' field
                ObjectQuery<BizFormItem> data = BizFormItemProvider.GetItems(formClassName)
                                                                        .WhereEmpty("UserMessage");

                // Loops through the form's data records
                foreach (BizFormItem item in data)
                {
                    // Deletes all files stored in the form's fields
                    BizFormInfoProvider.DeleteBizFormRecordFiles(formClass.ClassFormDefinition, item, SiteContext.CurrentSiteName);

                    // Deletes the form record from the database
                    item.Delete();
                }
            }
        }
    }
}
