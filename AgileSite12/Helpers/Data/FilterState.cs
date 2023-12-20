using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Helpers
{
    /// <summary>
    /// Represents a state of the filter control.
    /// </summary>
    [Serializable]
    public sealed class FilterState : IEnumerable<KeyValuePair<string, object>>, ISerializable
    {
        #region "Variables"

        /// <summary>
        /// The dictionary that holds name/value pairs.
        /// </summary>
        private readonly Dictionary<string, object> mValues;


        /// <summary>
        /// Returns stored state names
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return mValues.Keys;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FilterState class.
        /// </summary>
        public FilterState()
        {
            mValues = new Dictionary<string, object>();
        }

        
        /// <summary>
        /// Initializes a new instance of the FilterState class with the specified serialization context.
        /// </summary>
        /// <param name="info">The SerializationInfo with data.</param>
        /// <param name="context">The source of this deserialization.</param>
        private FilterState(SerializationInfo info, StreamingContext context) : this()
        {
            foreach (SerializationEntry entry in info)
            {
                mValues.Add(entry.Name, entry.Value);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Adds a value into the filter state.
        /// </summary>
        /// <param name="name">The name to associate with the value.</param>
        /// <param name="value">The value to serialize.</param>
        public void AddValue(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            mValues[name] = value;
        }

        
        /// <summary>
        /// Retrieves a boolean value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The boolean value associated with the specified name.</returns>
        public bool GetBoolean(string name, bool defaultValue = false)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves an 8-bit unsigned integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 8-bit unsigned integer value associated with the specified name.</returns>
        public byte GetByte(string name, byte defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a Unicode character from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The Unicode character associated with the specified name.</returns>
        public char GetChar(string name, char defaultValue = '\0')
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a DateTime value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The DateTime value associated with the specified name.</returns>
        public DateTime GetDateTime(string name, DateTime defaultValue = new DateTime())
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a decimal value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The decimal value associated with the specified name.</returns>
        public decimal GetDecimal(string name, decimal defaultValue = 0.0M)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a double-precision floating-point value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The double-precision floating-point value associated with the specified name.</returns>
        public double GetDouble(string name, double defaultValue = 0.0D)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a 16-bit signed integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 16-bit signed integer value associated with the specified name.</returns>
        public short GetInt16(string name, short defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a 32-bit signed integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 32-bit signed integer value associated with the specified name.</returns>
        public int GetInt32(string name, int defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a 64-bit signed integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 64-bit signed integer value associated with the specified name.</returns>
        public long GetInt64(string name, long defaultValue = 0L)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves an 8-bit signed integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 8-bit signed integer value associated with the specified name.</returns>
        public sbyte GetSByte(string name, sbyte defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a single-precision floating-point value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The single-precision floating-point value associated with the specified name.</returns>
        public float GetSingle(string name, float defaultValue = 0.0F)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a String value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The String value associated with the specified name.</returns>
        public string GetString(string name, string defaultValue = null)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a 16-bit unsigned integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 16-bit unsigned integer value associated with the specified name.</returns>
        public ushort GetUInt16(string name, ushort defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a 32-bit unsigned integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 32-bit unsigned integer value associated with the specified name.</returns>
        public uint GetUInt32(string name, uint defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves a 64-bit unsigned integer value from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The 64-bit unsigned integer value associated with the specified name.</returns>
        public ulong GetUInt64(string name, ulong defaultValue = 0)
        {
            return GetValue(name, defaultValue);
        }


        /// <summary>
        /// Retrieves an object from the filter state.
        /// </summary>
        /// <param name="name">The name associated with the object to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The object associated with the specified name.</returns>
        public object GetObject(string name, object defaultValue = null)
        {
            return GetValue(name, defaultValue);
        }

        
        /// <summary>
        /// Retrieves a value from the filter state.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="defaultValue">The default value that will be returned if state doesn't contain value for specified name.</param>
        /// <returns>The value associated with the specified name, if found, otherwise the default value.</returns>
        public T GetValue<T>(string name, T defaultValue = default(T))
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object value;
            if (!mValues.TryGetValue(name, out value))
            {
                return defaultValue;
            }

            return (T)value;
        }

        #endregion


        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection of name/value pairs.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection of name/value pairs.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return mValues.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection of name/value pairs.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection of name/value pairs.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region ISerializable Members

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (KeyValuePair<string, object> pair in mValues)
            {
                info.AddValue(pair.Key, pair.Value);
            }
        }

        #endregion
    }
}