namespace CMS.SalesForce
{

    /// <summary>
    /// Type of the mapping item source.
    /// </summary>
    public enum MappingItemSourceTypeEnum
    {

        /// <summary>
        /// CMS object field.
        /// </summary>
        Field,

        /// <summary>
        /// Meta-information associated with CMS object.
        /// </summary>
        MetaField,

        /// <summary>
        /// SalesForce picklist entry.
        /// </summary>
        PicklistEntry

    }

}