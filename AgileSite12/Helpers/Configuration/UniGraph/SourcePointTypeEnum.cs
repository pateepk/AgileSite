using System.Xml.Serialization;

namespace CMS.Helpers
{
    /// <summary>
    /// Graph source point type enumeration.
    /// </summary>
    public enum SourcePointTypeEnum : int
    {
        /// <summary>
        /// Standard source point.
        /// </summary>
        [XmlEnum("0")] 
        Standard = 0,

        /// <summary>
        /// Source point representing 'if' branch in conditional nodes.
        /// </summary>
        [XmlEnum("1")]
        SwitchCase = 1,

        /// <summary>
        /// Source point representing 'else' branch in conditional nodes.
        /// </summary>
        [XmlEnum("2")]
        SwitchDefault = 2,
        
        /// <summary>
        /// Source point representing timeout branch.
        /// </summary>
        [XmlEnum("3")]
        Timeout = 3
    }
}
