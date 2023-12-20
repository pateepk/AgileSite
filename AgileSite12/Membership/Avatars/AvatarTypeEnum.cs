using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Defines type of the avatar object.
    /// </summary>
    public enum AvatarTypeEnum : int
    {
        /// <summary>
        /// Global avatar, can be used anywhere.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("all")]
        All = 1,

        /// <summary>
        /// User avatar, used in forums, message board, etc.
        /// </summary>
        [EnumStringRepresentation("user")]
        User = 2,

        /// <summary>
        /// Group avatar, used in groups.
        /// </summary>
        [EnumStringRepresentation("group")]
        Group = 3
    }
}