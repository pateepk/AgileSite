using System.Xml.Serialization;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow step security enumeration.
    /// </summary>
    public enum WorkflowStepSecurityEnum
    {
        /// <summary>
        /// Default security settings. (for whole step - no one is allowed, for source point - inherit settings from step)
        /// </summary>
        [XmlEnum("0")]
        Default = 0,

        /// <summary>
        /// Only assigned are granted with permissions.
        /// </summary>
        [XmlEnum("1")]
        OnlyAssigned = 1,

        /// <summary>
        /// All are granted with permissions except assigned.
        /// </summary>
        [XmlEnum("2")]
        AllExceptAssigned = 2
    }
}