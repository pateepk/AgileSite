using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Provides information on deserialization process result.
    /// </summary>
    /// <remarks>Class is intended for internal and testing purposes.</remarks>
    public abstract class DeserializationResultBase
    {
        private IDictionary<string, string> mFailedFields;

        private ICollection<FailedMapping> mFailedMappings;


        /// <summary>
        /// Returns true if deserialized info object contains all fields that were present in its source and all references were successfully mapped to existing DB objects.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !FailedFields.Any() && !FailedMappings.Any();
            }
        }


        /// <summary>
        /// Collection of names of fields and translation references that was impossible to map to existing DB objects.
        /// </summary>
        /// <remarks>
        /// In most cases, there will be at most single failed mapping present for given field.
        /// Occasionally, however, there might be multiple failed mappings for the field.
        /// </remarks>
        public ICollection<FailedMapping> FailedMappings
        {
            get
            {
                return mFailedMappings ?? (mFailedMappings = new List<FailedMapping>());
            }
            set
            {
                mFailedMappings = value;
            }
        }


        /// <summary>
        /// Collection of names of fields and raw values that was impossible to deserialize.
        /// </summary>
        public IDictionary<string, string> FailedFields
        {
            get
            {
                return mFailedFields ?? (mFailedFields = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));
            }
            set
            {
                mFailedFields = value;
            }
        }


        internal void MergeWith(DeserializationResultBase otherResult)
        {
            FailedFields = FailedFields.Union(otherResult.FailedFields).ToDictionary(x => x.Key, x => x.Value);
            FailedMappings = FailedMappings.Union(otherResult.FailedMappings).ToList();
        }
    }
}
