using System;

using CMS.DataEngine;
using CMS.CustomTables;
using CMS.Base;
using CMS.Helpers;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples for managing custom table data.
    /// </summary>
    /// <pageTitle>Custom table data</pageTitle>
    internal class CustomTableData
    {
        /// <heading>Adding data records to a custom table</heading>
        private void CreateCustomTableItem()
        {
            // Prepares the code name (class name) of the custom table to which the data record will be added
            string customTableClassName = "customtable.sampletable";

            // Gets the custom table
            DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(customTableClassName);
            if (customTable != null)
            {
                // Creates a new custom table item 
                CustomTableItem newCustomTableItem = CustomTableItem.New(customTableClassName);

                // Sets the values for the fields of the custom table (ItemText in this case)
                newCustomTableItem.SetValue("ItemText", "New text");

                // Save the new custom table record into the database
                newCustomTableItem.Insert();
            }
        }


        /// <heading>Loading data records from a custom table</heading>
        private void GetCustomTableItem()
        {
            // Prepares the code name (class name) of the custom table
            string customTableClassName = "customtable.sampletable";

            // Gets the custom table
            DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(customTableClassName);
            if (customTable != null)
            {
                // Gets a custom table record with a specific primary key ID (25)
                CustomTableItem item1 = CustomTableItemProvider.GetItem(25, customTableClassName);

                // Gets the first custom table record whose value in the 'ItemName' field is equal to "SampleName"
                CustomTableItem item2 = CustomTableItemProvider.GetItems(customTableClassName)
                                                                    .WhereEquals("ItemName", "SampleName")
                                                                    .FirstObject;

                // Loads a string value from the 'ItemText' field of the 'item1' custom table record
                string itemTextValue = ValidationHelper.GetString(item1.GetValue("ItemText"), "");
            }
        }


        /// <heading>Updating the data records of a custom table</heading>
        private void GetAndUpdateCustomTableItems()
        {
            // Prepares the code name (class name) of the custom table
            string customTableClassName = "customtable.sampletable";

            // Gets the custom table
            DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(customTableClassName);
            if (customTable != null)
            {
                // Gets all data records from the custom table whose 'ItemText' field value starts with 'New text'
                var customTableData = CustomTableItemProvider.GetItems(customTableClassName)
                                                                    .WhereStartsWith("ItemText", "New text");
                
                // Loops through individual custom table records
                foreach (CustomTableItem item in customTableData)
                {
                    // Gets the text value from the data record's 'ItemText' field
                    string itemText = ValidationHelper.GetString(item.GetValue("ItemText"), "");

                    // Sets a new 'ItemText' value based on the old one
                    item.SetValue("ItemText", itemText.ToLowerCSafe());

                    // Saves the changes to the database
                    item.Update();
                }
            }
        }


        /// <heading>Changing the order of custom table records</heading>
        private void ChangeCustomTableItemOrder()
        {
            // Prepares the code name (class name) of the custom table
            string customTableClassName = "customtable.sampletable";

            // Gets the custom table
            DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(customTableClassName);
            if (customTable != null)
            {
                // Gets a custom table record with a specific ID (25)
                CustomTableItem item = CustomTableItemProvider.GetItem(25, customTableClassName);

                // Moves the data record down in the list
                item.Generalized.MoveObjectDown();

                // Moves the data record up in the list
                item.Generalized.MoveObjectUp();
            }
        }


        /// <heading>Deleting custom table records</heading>
        private void DeleteCustomTableItem()
        {
            // Prepares the code name (class name) of the custom table from which the record will be deleted
            string customTableClassName = "customtable.sampletable";

            // Gets the custom table
            DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(customTableClassName);
            if (customTable != null)
            {
                // Gets the first custom table record whose value in the 'ItemText' starts with 'New text'
                CustomTableItem item = CustomTableItemProvider.GetItems(customTableClassName)
                                                                    .WhereStartsWith("ItemText", "New text")
                                                                    .FirstObject;
                
                if (item != null)
                {
                    // Deletes the custom table record from the database
                    item.Delete();
                }
            }
        }
    }
}