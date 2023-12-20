namespace CMS.SalesForce
{

    /// <summary>
    /// Specifies the type of the SalesForce entity attribute value.
    /// </summary>
    public enum EntityAttributeValueType
    {

        /// <summary>
        /// String
        /// </summary>
        String,
        
        /// <summary>
        /// Picklist
        /// </summary>
        Picklist,
        
        /// <summary>
        /// Picklist with multiple selection
        /// </summary>
        MultiPicklist,

        /// <summary>
        /// Combobox
        /// </summary>
        Combobox,

        /// <summary>
        /// Reference to other entity
        /// </summary>
        Reference,

        /// <summary>
        /// Binary data
        /// </summary>
        Base64,

        /// <summary>
        /// Boolean
        /// </summary>
        Boolean,

        /// <summary>
        /// Currency
        /// </summary>
        Currency,

        /// <summary>
        /// Textarea
        /// </summary>
        Textarea,

        /// <summary>
        /// Integer
        /// </summary>
        Integer,

        /// <summary>
        /// Double
        /// </summary>
        Double,

        /// <summary>
        /// Percent
        /// </summary>
        Percent,

        /// <summary>
        /// Phone number
        /// </summary>
        PhoneNumber,

        /// <summary>
        /// Unique identifier
        /// </summary>
        Id,

        /// <summary>
        /// Date
        /// </summary>
        Date,

        /// <summary>
        /// Date and time
        /// </summary>
        DateTime,

        /// <summary>
        /// Time
        /// </summary>
        Time,

        /// <summary>
        /// URL
        /// </summary>
        Url,

        /// <summary>
        /// Email address
        /// </summary>
        EmailAddress,

        /// <summary>
        /// Encrypted string
        /// </summary>
        EncryptedString,

        /// <summary>
        /// Reference to a data category group
        /// </summary>
        DataCategoryGroupReference,

        /// <summary>
        /// Any
        /// </summary>
        Any

    }

}