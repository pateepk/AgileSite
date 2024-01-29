using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Field visibility types.
    /// </summary>
    public enum FormFieldVisibilityTypeEnum
    {
        /// <summary>
        /// The field is visible to all.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("all")]
        All = 0,


        /// <summary>
        /// The field is hidden.
        /// </summary>
        [EnumStringRepresentation("none")]
        None,


        /// <summary>
        /// The field is visible to authenticated users.
        /// </summary>
        [EnumStringRepresentation("authenticated")]
        Authenticated,


        /// <summary>
        /// The field is visible to friends.
        /// </summary>
        [EnumStringRepresentation("friends")]
        Friends,
    }
}