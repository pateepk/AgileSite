using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Unigrid filter types enumeration.
    /// </summary>
    public enum UniGridFilterTypeEnum
    {
        /// <summary>
        /// Custom filter type.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("custom")]
        Custom = 0,


        /// <summary>
        /// Text filter type.
        /// </summary>
        [EnumStringRepresentation("text")]
        Text = 1,


        /// <summary>
        /// Boolean filter type.
        /// </summary>
        [EnumStringRepresentation("bool")]
        Bool = 2,


        /// <summary>
        /// Integer filter type.
        /// </summary>
        [EnumStringRepresentation("integer")]
        Integer = 3,


        /// <summary>
        /// Double filter type.
        /// </summary>
        [EnumStringRepresentation("double")]
        Double = 4,


        /// <summary>
        /// Site filter type.
        /// </summary>
        [EnumStringRepresentation("site")]
        Site = 5,


        /// <summary>
        /// Decimal filter type.
        /// </summary>
        [EnumStringRepresentation("decimal")]
        Decimal = 6,
    }
}
