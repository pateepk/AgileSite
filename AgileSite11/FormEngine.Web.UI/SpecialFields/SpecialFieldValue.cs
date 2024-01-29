using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for storing values of special fields.
    /// </summary>
    public class SpecialFieldValue : AbstractDataContainer<SpecialFieldValue>
    {
        /// <summary>
        /// Represents 'none' or 'global' record selected.
        /// </summary>
        [RegisterColumn("None")]
        public const int NONE = 0;


        /// <summary>
        /// Represents 'all' record selected.
        /// </summary>
        [RegisterColumn("All")]
        public const int ALL = -1;


        /// <summary>
        /// Represents 'global' record selected.
        /// </summary>
        [RegisterColumn("Global")]
        public const int GLOBAL = -4;


        /// <summary>
        /// Represents 'global or site' record selected.
        /// </summary>
        [RegisterColumn("GlobalAndSite")]
        public const int GLOBAL_AND_SITE = -5;
    }
}
