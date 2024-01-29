using System;
using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents key value pair data used for Google Tag Manager integration.
    /// </summary>
    /// <remarks>
    /// Properties of the object are case insensitive.
    /// </remarks>
    public class GtmData : Dictionary<string, object>
    {
        /// <summary>
        /// Creates expandable Google Tag Manager object.
        /// </summary>
        public GtmData()
            : base(StringComparer.Ordinal)
        {
        }
    }
}
