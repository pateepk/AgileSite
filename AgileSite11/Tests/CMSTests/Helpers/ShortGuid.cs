using System;

namespace CMS.Tests
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) with a shorter string representation.
    /// </summary>
    public class ShortGuid
    {
        private readonly Guid mGuid;
        private readonly string mStringValue;


        /// <summary>
        /// Creates a ShortGuid from a base64 encoded string.
        /// </summary>
        /// <param name="value">The encoded guid as a base64 string.</param>
        public ShortGuid(string value)
        {
            mStringValue = value;
            mGuid = Decode(value);
        }


        /// <summary>
        /// Creates a ShortGuid from a Guid.
        /// </summary>
        /// <param name="guid">The Guid to encode.</param>
        public ShortGuid(Guid guid)
        {
            mStringValue = Encode(guid);
            mGuid = guid;
        }


        /// <summary>
        /// Initialises a new instance of the ShortGuid class.
        /// </summary>
        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }


        private static string Encode(Guid guid)
        {
            string encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded.Replace('/', '_').Replace('+', '-');
            
            // Remove '==' at the end of encoded string
            return encoded.Substring(0, 22);
        }


        private static Guid Decode(string value)
        {
            value = value.Replace('_', '/').Replace('-', '+');
            byte[] buffer = Convert.FromBase64String(value + "==");

            return new Guid(buffer);
        }


        /// <summary>
        /// Returns the base64 encoded guid as a string.
        /// </summary>
        public override string ToString()
        {
            return mStringValue;
        }


        /// <summary>
        /// Returns a value indicating whether this instance and a specified object represent the same type and value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        protected bool Equals(ShortGuid other)
        {
            return mGuid.Equals(other.mGuid);
        }


        /// <summary>
        /// Returns a value indicating whether this instance and a specified object represent the same type and value.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ShortGuid)obj);
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return mGuid.GetHashCode();
        }
    }
}
