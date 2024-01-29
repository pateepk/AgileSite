using System;

using CMS.FormEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Action event handler for new field creation.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="newField">Newly created field</param>
    public delegate void OnFieldCreatedEventHandler(object sender, FormFieldInfo newField);


    /// <summary>
    /// Action event handler fired when field name was changed.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="oldFieldName">Old field name</param>
    /// <param name="newFieldName">New field name</param>
    public delegate void OnFieldNameChangedEventHandler(object sender, string oldFieldName, string newFieldName);


    /// <summary>
    /// Action event handler fired when form item was deleted.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Field editor event arguments</param>
    public delegate void AfterItemDeletedEventHandler(object sender, FieldEditorEventArgs e);


    /// <summary>
    /// Field editor event args.
    /// </summary>
    public class FieldEditorEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemName">Selected item name</param>
        /// <param name="itemType">Selected item type - field/category</param>
        /// <param name="itemOrder">Selected item order</param>
        public FieldEditorEventArgs(string itemName, FieldEditorSelectedItemEnum itemType, int itemOrder)
        {
            ItemName = itemName;
            ItemType = itemType;
            ItemOrder = itemOrder;
        }


        /// <summary>
        /// Selected item name.
        /// </summary>
        public string ItemName;


        /// <summary>
        /// Selected item order.
        /// </summary>
        public int ItemOrder;


        /// <summary>
        /// Selected item type - field/category.
        /// </summary>
        public FieldEditorSelectedItemEnum ItemType;
    }
}