namespace CMS.FormEngine
{
    /// <summary>
    /// Field control types.
    /// </summary>
    public enum FormFieldControlTypeEnum
    {
        /// <summary>
        /// Label control.
        /// </summary>
        LabelControl = 1,

        /// <summary>
        /// TextBox control.
        /// </summary>
        TextBoxControl = 2,

        /// <summary>
        /// DropDownList control.
        /// </summary>
        DropDownListControl = 3,

        /// <summary>
        /// RadioButtons control.
        /// </summary>
        RadioButtonsControl = 4,

        /// <summary>
        /// MultipleChoice control.
        /// </summary>
        MultipleChoiceControl = 5,

        /// <summary>
        /// ImageSelection control.
        /// </summary>
        ImageSelectionControl = 6,

        /// <summary>
        /// FileSelectionControl.
        /// </summary>
        FileSelectionControl = 7,


        /// <summary>
        /// CustomUser control.
        /// </summary>
        CustomUserControl = 8,


        /// <summary>
        /// Upload control.
        /// </summary>
        UploadControl = 9,


        /// <summary>
        /// TextArea control.
        /// </summary>
        TextAreaControl = 10,


        /// <summary>
        /// HtmlArea control.
        /// </summary>
        HtmlAreaControl = 11,


        /// <summary>
        /// Calendar control.
        /// </summary>
        CalendarControl = 12,


        /// <summary>
        /// CheckBox control.
        /// </summary>
        CheckBoxControl = 13,


        /// <summary>
        /// TextBox with integer number.
        /// </summary>
        IntegerNumberTextBox = 14,


        /// <summary>
        /// TextBox with decimal number.
        /// </summary>
        DecimalNumberTextBox = 15,


        /// <summary>
        /// Unknown control - error control type.
        /// </summary>
        Unknown = 16,


        /// <summary>
        /// Upload file field (HTTP Input file).
        /// </summary>
        UploadFile = 17,


        /// <summary>
        /// Listbox control.
        /// </summary>
        ListBoxControl = 18,


        /// <summary>
        /// BBEditor control.
        /// </summary>
        BBEditorControl = 19,


        /// <summary>
        /// Document attachments control.
        /// </summary>
        DocumentAttachmentsControl = 20,


        /// <summary>
        /// Media selector control.
        /// </summary>
        MediaSelectionControl = 21,


        /// <summary>
        /// Direct uploader control.
        /// </summary>
        DirectUploadControl = 22,


        /// <summary>
        /// TextBox with long integer number.
        /// </summary>
        LongNumberTextBox = 23,


        /// <summary>
        /// Selector for table columns.
        /// </summary>
        SelectColumns = 24,


        /// <summary>
        /// Encrypted password control.
        /// </summary>
        EncryptedPassword = 25
    }
}