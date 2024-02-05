using CMS.Helpers;

namespace CMS.Personas
{
    /// <summary>
    /// Specifies types of mass action on personas and nodes.
    /// </summary>
    public enum MultipleDocumentsActionTypeEnum
    {
        /// <summary>
        /// Assign personas to nodes
        /// </summary>
        [EnumStringRepresentation("tag")]
        Tag,


        /// <summary>
        /// Delete assignment between personas and nodes
        /// </summary>
        [EnumStringRepresentation("untag")]
        Untag,
    }
}