using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Provides information on deserialization process result and partially or fully deserialized <see cref="BaseInfo"/>.
    /// </summary>
    public class DeserializationResult : DeserializationResultBase
    {
        /// <summary>
        /// Deserialized info object
        /// </summary>
        /// <remarks>
        /// See <see cref="DeserializationResultBase.IsValid"/> to identify whether deserialization was fully or just partially successful.
        /// </remarks>
        public BaseInfo DeserializedInfo
        {
            private set;
            get;
        }


        /// <summary>
        /// Creates <see cref="DeserializationResult"/> instance.
        /// </summary>
        /// <param name="deserializedInfo">Info object representing deserialized data.</param>
        public DeserializationResult(BaseInfo deserializedInfo)
        {
            if (deserializedInfo == null)
            {
                throw new ArgumentNullException("deserializedInfo");
            }

            DeserializedInfo = deserializedInfo;
        }


        /// <summary>
        /// Creates <see cref="DeserializationResult"/> instance copying provided
        /// <paramref name="failedFields"/> into <see cref="DeserializationResultBase.FailedFields"/>.
        /// </summary>
        /// <param name="deserializedInfo">Info object representing deserialized data.</param>
        /// <param name="failedFields">Collection of <see cref="DeserializationResultBase.FailedFields"/>.</param>
        internal DeserializationResult(BaseInfo deserializedInfo, IEnumerable<KeyValuePair<string, string>> failedFields)
            : this(deserializedInfo)
        {
            if (failedFields == null)
            {
                throw new ArgumentNullException("failedFields");
            }

            foreach (var failedField in failedFields)
            {
                FailedFields.Add(failedField);
            }
        }


        /// <summary>
        /// Implicit conversion to <see cref="BaseInfo"/> returns value of <see cref="DeserializedInfo"/>.
        /// </summary>
        public static implicit operator BaseInfo(DeserializationResult deserializationResult)
        {
            return deserializationResult.DeserializedInfo;
        }
    }
}
