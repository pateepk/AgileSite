using System;
using System.Collections;
using System.Runtime.Serialization;

namespace CMS.Base
{
    /// <summary>
    /// Safe dictionary indexed by string
    /// </summary>
    [Serializable]
    public class StringSafeDictionary<TValue> : SafeDictionary<string, TValue>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="d">Source data dictionary</param>
        /// <param name="comparer">Equality comparer for items</param>
        public StringSafeDictionary(IDictionary d, IEqualityComparer comparer)
            : base(d, comparer)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="d">Source data dictionary</param>
        /// <param name="caseSensitive">If true, the access to the dictionary is case sensitive</param>
        public StringSafeDictionary(IDictionary d, bool caseSensitive = false)
            : base(d, GetComparer(caseSensitive))
        {
        }


        /// <summary>
        /// Default constructor, creates case insensitive dictionary
        /// </summary>
        public StringSafeDictionary()
            : this(false)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="caseSensitive">If true, the access to the dictionary is case sensitive</param>
        public StringSafeDictionary(bool caseSensitive)
            : base(GetComparer(caseSensitive))
        {
        }


        /// <summary>
        /// De-serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public StringSafeDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Gets the comparer for the dictionary
        /// </summary>
        /// <param name="caseSensitive">If true, the access to the dictionary is case sensitive</param>
        private static StringComparer GetComparer(bool caseSensitive)
        {
            return caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;
        }


        /// <summary>
        /// Clones the dictionary
        /// </summary>
        public override object Clone()
        {
            var clone = new StringSafeDictionary<TValue>(this, EqualityComparer);
            
            CopyPropertiesTo(clone);

            return clone;
        }
    }
}
