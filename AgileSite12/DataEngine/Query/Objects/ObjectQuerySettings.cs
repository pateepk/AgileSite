using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object query parameters
    /// </summary>
    [Obsolete("Use type CMS.DataEngine.DataQuerySettings instead")]
    public class ObjectQuerySettings : DataQuerySettingsBase<ObjectQuerySettings>
    {
        /// <summary>
        /// If true, the query includes the object binary data. Default is false
        /// </summary>
        [Obsolete("Use property IDataQuery.IncludeBinaryData instead", true)]
        public bool IncludeBinaryData
        {
            get;
            set;
        }


        /// <summary>
        /// Sets whether the binary data should be included to the result
        /// </summary>
        /// <param name="binary">Include binary data?</param>
        [Obsolete("Use property IDataQuery.IncludeBinaryData instead", true)]
        public ObjectQuerySettings BinaryData(bool binary)
        {
            var result = GetTypedQuery();
            result.IncludeBinaryData = binary;

            return result;
        }
    }
}
