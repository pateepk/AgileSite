using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Transformation condition enumeration.
    /// </summary>
    public enum TransformationConditionTypeEnum
    {
        /// <summary>
        /// The input value must be equal to the data value.
        /// </summary>
        Equal,

        /// <summary>
        /// The input value must not be equal to the data value.
        /// </summary>
        NotEqual,

        /// <summary>
        /// The input value must be equal or part of the data data value.
        /// </summary>
        Like,

        /// <summary>
        /// The input value must not be equal or part of the data data value.
        /// </summary>
        NotLike,

        /// <summary>
        /// The input value must be in list of data value.
        /// </summary>
        In,

        /// <summary>
        /// The input value must not be in list of data value.
        /// </summary>
        NotIn,
    }
}