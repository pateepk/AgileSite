using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CMS.Base
{
    #region "Safe dictionary"

    /// <summary>
    /// Represents a thread-safe collection of key/value pairs that can be modified by multiple threads concurrently.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <remarks>
    /// <para>
    /// Keep in mind that the operations of this class are thread-safe. However, a sequence of operations inherently cannot be thread-safe.
    /// If you want to perform multiple operations as one atomic operation, use the <see cref="SyncRoot"/> object.
    /// </para>
    /// <para>
    /// In current implementation some read operations do not require locking (i.e. multiple threads can read even though some other thread has the lock).
    /// If your situation requires all the readers to wait when the dictionary is locked by some other thread, consider some other collection suitable for readers-writers synchronization problem.
    /// </para>
    /// </remarks>
    [Serializable]
    public class SafeDictionary<TKey, TValue> : Hashtable, ICloneThreadItem, IXmlSerializable, IGeneralIndexable<TKey, TValue>
    {
        #region "Variables"

        /// <summary>
        /// Default value.
        /// </summary>
        protected TValue mDefaultValue;

        /// <summary>
        /// Null value.
        /// </summary>
        protected object mNullValue = DBNull.Value;

        /// <summary>
        /// If true, the weak references are used for the items so the memory can be cleared upon request
        /// </summary>
        protected bool mUseWeakReferences = false;

        /// <summary>
        /// If true, the dictionary is copied (not cloned) to a new thread
        /// </summary>
        private bool mCopyToNewThread = true;


        /// <summary>
        /// Object used for underlying Hashtable access synchronization.
        /// </summary>
        private readonly object mSyncRoot = new object(); 

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if dictionary is synchronized.
        /// </summary>
        /// <remarks>
        /// Current implementation ensures thread-safety for read/write operations (read operations are safe to be performed without the need for locking).
        /// To synchronize enumeration or multiple operations use <see cref="SyncRoot"/>.
        /// </remarks>
        public override bool IsSynchronized
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="SafeDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="SafeDictionary{TKey,TValue}"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Utilize this property when you need to perform multiple dictionary operations as a single atomic operation,
        /// or when you need to enumerate the entries.
        /// </para>
        /// <para>
        /// Obtaining a lock does not prevent all other threads from read operations, since most read operations do not
        /// need to use locking.
        /// </para>
        /// </remarks>
        /// <example>
        /// Use the following snippet to enumerate the dictionary entries in a thread-safe way.
        /// <code>
        /// lock (dictionary.SyncRoot)
        /// {
        ///     foreach (var entry in dictionary)
        ///     {
        ///         // Perform some operation
        ///         // Try to avoid time consuming operations to release the lock as soon as possible
        ///         // No other thread can modify the dictionary when it is locked using the SyncRoot property
        ///         // However, read access does not require locking, therefore any other thread can still read the dictionary's content
        ///     }
        /// }
        /// </code>
        /// </example>
        public override object SyncRoot
        {
            get
            {
                // Original SyncRoot is overridden because its lazy loading feature is not desired.
                // It's assumed that SafeDictionary will always need the synchronization object.
                return mSyncRoot;
            }
        }


        /// <summary>
        /// Default value.
        /// </summary>
        public TValue DefaultValue
        {
            get
            {
                return mDefaultValue;
            }
            set
            {
                mDefaultValue = value;
            }
        }


        /// <summary>
        /// Gets a typed collection of keys in this dictionary
        /// </summary>
        public IEnumerable<TKey> TypedKeys
        {
            get
            {
                // The Keys property is already synchronized
                return Keys.Cast<TKey>();
            }
        }


        /// <summary>
        /// Gets a typed collection of values in this dictionary
        /// </summary>
        public IEnumerable<TValue> TypedValues
        {
            get
            {
                if (UseWeakReferences)
                {
                    throw new NotSupportedException("[SafeDictionary.TypedValues]: Dictionary using weak references does not support this query.");
                }

                // The Values property is already synchronized
                return Values.Cast<TValue>();
            }
        }

        
        /// <summary>
        /// Items indexer. Gets or sets the value in the dictionary.
        /// </summary>
        /// <param name="key">Value key</param>
        /// <remarks>
        /// The <see cref="get_Item(object)"/> operation does not require locking for concurrent access.
        /// </remarks>
        public override object this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }


        /// <summary>
        /// Items indexer. Gets or sets the value in the dictionary.
        /// </summary>
        /// <param name="key">Value key</param>
        /// <remarks>
        /// The <see cref="get_Item(TKey)"/> operation does not require locking for concurrent access.
        /// </remarks>
        public virtual TValue this[TKey key]
        {
            get
            {
                object value = GetInternalValue(key);
                if ((value == null) && (typeof(TValue).IsValueType) || (AllowNulls && (value == NullValue)))
                {
                    return default(TValue);
                }

                return (TValue)value;
            }
            set
            {
                if ((value == null) && AllowNulls)
                {
                    SetInternalValue(key, NullValue);
                }
                else
                {
                    SetInternalValue(key, value);
                }
            }
        }


        /// <summary>
        /// If true, the dictionary allows null values as valid.
        /// </summary>
        public bool AllowNulls
        {
            get;
            set;
        }


        /// <summary>
        /// Null value.
        /// </summary>
        public object NullValue
        {
            get
            {
                return mNullValue;
            }
            protected set
            {
                if (value is TValue)
                {
                    throw new Exception("[SafeDictionary.NullValue]: Null value must be different type than the value type of this dictionary.");
                }

                mNullValue = value;
            }
        }


        /// <summary>
        /// If true, the weak references are used for the items so the memory can be cleared upon request.
        /// The property can be set only when the dictionary is empty. To ensure thread-safety, you have
        /// to perform the check for emptiness and property assignment in a critical section (use <see cref="SyncRoot"/> for that purpose).
        /// </summary>
        /// <example>
        /// Use the following code snippet to ensure thread-safety while changing this property.
        /// <code>
        /// lock (dictionary.SyncRoot)
        /// {
        ///     dictionary.Clear();
        ///     dictionary.UseWeakReferences = true;
        /// }
        /// </code>
        /// </example>
        public bool UseWeakReferences
        {
            get
            {
                return mUseWeakReferences;
            }
            set
            {
                lock (SyncRoot)
                {
                    // The Count property can be read without locking, the lock ensures atomicity of condition evaluation and assignment
                    if (Count > 0)
                    {
                        throw new Exception("[SafeDictionary.UseWeakReferences]: This setting can be changed only when the dictionary is empty.");
                    }

                    mUseWeakReferences = value;
                }
            }
        }
        

        /// <summary>
        /// If true, the dictionary is copied (not cloned) to a new thread
        /// </summary>
        public bool CopyToNewThread
        {
            get
            {
                return mCopyToNewThread;
            }
            set
            {
                mCopyToNewThread = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// SafeDictionary constructor.
        /// </summary>
        public SafeDictionary()
        {
        }


        /// <summary>
        /// SafeDictionary constructor.
        /// </summary>
        /// <param name="comparer">Equality comparer for the items</param>
        public SafeDictionary(IEqualityComparer comparer)
            : base(comparer)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="d">Source data dictionary</param>
        /// <param name="comparer">Equality comparer for the items</param>
        public SafeDictionary(IDictionary d, IEqualityComparer comparer = null)
            : base(d, comparer)
        {
        }


        /// <summary>
        /// Clones the dictionary
        /// </summary>
        public override object Clone()
        {
            var clone = new SafeDictionary<TKey, TValue>(this, EqualityComparer);

            CopyPropertiesTo(clone);

            return clone;
        }


        /// <summary>
        /// Copies the dictionary properties to the target dictionary
        /// </summary>
        /// <param name="target">Target dictionary</param>
        protected void CopyPropertiesTo(SafeDictionary<TKey, TValue> target)
        {
            target.mDefaultValue = mDefaultValue;
            target.mNullValue = mNullValue;
            target.mUseWeakReferences = mUseWeakReferences;
            target.mCopyToNewThread = mCopyToNewThread;
            target.mUseWeakReferences = mUseWeakReferences;
            target.AllowNulls = AllowNulls;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// De-serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public SafeDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AllowNulls = (bool)info.GetValue("AllowNulls", typeof(bool));
            mDefaultValue = (TValue)info.GetValue("DefaultValue", typeof(TValue));
            mUseWeakReferences = (bool)info.GetValue("UseWeakReferences", typeof(bool));
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("AllowNulls", AllowNulls);
            info.AddValue("DefaultValue", mDefaultValue);
            info.AddValue("UseWeakReferences", mUseWeakReferences);
        }


        /// <summary>
        /// Gets the real count of the objects in the dictionary.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The dictionary's content can be changed by other threads after computing
        /// the result. If you want to make sure the dictionary's content does
        /// not change until the real count is used, use the <see cref="SyncRoot"/>.
        /// </para>
        /// <para>
        /// Important: When the dictionary uses weak references, no lock can guarantee you
        /// that no item has been released from the memory while computing the real count.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// lock (dictionary.SyncRoot)
        /// {
        ///     int realCount = dictionary.GetRealCount();
        /// 
        ///     // The realCount corresponds to current dictionary's state only when weak references are NOT used
        ///     // Otherwise, some entries might have been released from the memory regardless of the lock
        /// }
        /// </code>
        /// </example>
        public int GetRealCount()
        {
            int count = 0;

            if (UseWeakReferences)
            {
                lock (SyncRoot)
                {
                    // Count live weak references with non-null values
                    foreach (WeakReference item in Values)
                    {
                        if ((item != null) && (item.Target != null))
                        {
                            count++;
                        }
                    }
                }
            }
            else
            {
                lock (SyncRoot)
                {
                    // Count non-null values
                    foreach (TValue item in Values)
                    {
                        if (item != null)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }


        /// <summary>
        /// Gets the value from the internal dictionary
        /// </summary>
        /// <param name="key">Object key</param>
        protected object GetInternalValue(TKey key)
        {
            // The get_Item operation can be performed without locking
            object obj = base[key];

            if (UseWeakReferences && (obj != null))
            {
                // Convert the inner value to the inner object
                obj = ((WeakReference)obj).Target;
            }

            return obj;
        }


        /// <summary>
        /// Sets the value in the internal dictionary
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="value">Object value</param>
        protected void SetInternalValue(TKey key, object value)
        {
            if (UseWeakReferences)
            {
                // Convert the inner value to the inner object
                value = new WeakReference(value);
            }

            lock (SyncRoot)
            {
                // The set_Item operation needs a lock since Hashtable does not support concurrent write
                base[key] = value;
            }
        }


        /// <summary>
        /// Tries to get the value, returns true if the retrieval was successful.
        /// </summary>
        /// <param name="key">Value key</param>
        /// <param name="value">Returning value</param>
        /// <remarks>
        /// This operation does not require locking for concurrent access.
        /// </remarks>
        public bool TryGetValue(TKey key, out TValue value)
        {
            // Get the data
            bool result = true;

            object obj = GetInternalValue(key);

            if (AllowNulls && (obj == NullValue))
            {
                value = default(TValue);
            }
            else if (obj != null)
            {
                value = (TValue)obj;
            }
            else
            {
                value = mDefaultValue;
                result = false;
            }

            return result;
        }


        /// <summary>
        /// Adds the value to the dictionary if it does not exist.
        /// Updates existing, if it does exist.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <example>
        /// If you want to add some key-value pair only if it does not exist within the dictionary, use the following
        /// snippet to do so in a thread-safe way.
        /// <code>
        /// if (!dictionary.ContainsKey(myKey))
        /// {
        ///     lock (dictionary.SyncRoot)
        ///     {
        ///         if (!dictionary.ContainsKey(myKey))
        ///         {
        ///             dictionary.Add(myKey, myValue);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public override void Add(object key, object value)
        {
            SetInternalValue((TKey)key, value);
        }


        /// <summary>
        /// Returns true if the dictionary contains the given key
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks>
        /// This operation does not require locking for concurrent access.
        /// </remarks>
        public override bool ContainsKey(object key)
        {
            if (UseWeakReferences)
            {
                return (GetInternalValue((TKey)key) != null);
            }

            // The ContainsKey can be performed without locking
            return base.ContainsKey(key);
        }


        /// <summary>
        /// Returns true if the dictionary contains the given key
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks>
        /// This operation does not require locking for concurrent access.
        /// </remarks>
        public override bool Contains(object key)
        {
            return ContainsKey(key);
        }


        /// <summary>
        /// Returns true if the dictionary contains the given value
        /// </summary>
        /// <param name="value">Value</param>
        public override bool ContainsValue(object value)
        {
            if (UseWeakReferences)
            {
                throw new NotSupportedException("[SafeDictionary.ContainsValue]: Dictionary using weak references does not support this query.");
            }

            lock (SyncRoot) 
            {
                // The ContainsValue needs a lock
                return base.ContainsValue(value);
            }
        }


        /// <summary>
        /// Adds multiple items with same value to the dictionary
        /// </summary>
        /// <param name="items">Items to add</param>
        /// <param name="value">Items value</param>
        /// /// <remarks>
        /// The operation is not performed in a critical section. If you want to make sure
        /// no other thread modifies the dictionary (e.g. adds/removes an item) while the dictionary
        /// is being filled with items, use the <see cref="SyncRoot"/>.
        /// </remarks>
        /// <example>
        /// Use the following snippet to make sure no other thread modifies the dictionary while
        /// it is being populated by <paramref name="items"/>.
        /// <code>
        /// string[] items = GetItems(...);
        /// lock (dictionary.SyncRoot)
        /// {
        ///     dictionary.AddMultiple(items, true);
        /// 
        ///     // All the items are set to true now
        /// }
        /// </code>
        /// 
        /// The following approach is also thread-safe, but the result may differ from the previous one if another thread performs any write operation
        /// while the items are being added.
        /// <code>
        /// string[] items = GetItems(...);
        /// dictionary.AddMultiple(items, true);
        /// 
        /// // Any other thread might have modified some entry (i.e. the items are not guaranteed to be set to true)
        /// </code>
        /// </example>
        public void AddMultiple(string[] items, bool value)
        {
            // Add all items
            foreach (var item in items)
            {
                this[item] = value;
            }
        }


        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        public object CloneForNewThread()
        {
            if (CopyToNewThread)
            {
                return this;
            }

            return null;
        }

        #endregion


        #region "Thread-safe overrides"

        /// <summary>
        ///  Removes all elements from the System.Collections.Hashtable.
        /// </summary>
        public override void Clear()
        {
            lock (SyncRoot)
            {
                base.Clear();
            }
        }


        /// <summary>
        /// Copies the System.Collections.Hashtable elements to a one-dimensional System.Array instance at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the System.Collections.DictionaryEntry objects copied from System.Collections.Hashtable. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <remarks>
        /// The <paramref name="array"/> has to be large enough to accommodate all the dictionary entries. Unless the dictionary has a fixed number of entries
        /// (or the entries count is limited somehow and the limit is a known number), you should perform the array allocation in a thread-safe way.
        /// </remarks>
        /// <example>
        /// Use the following code snippet to ensure the proper array size.
        /// <code>
        /// lock (dictionary.SyncRoot)
        /// {
        ///     // The lock makes sure the entries count does not change between the array allocation and the copy operation
        ///     // The GetArrayOfSize is only an illustrational method
        ///     Array destinationArray = GetArrayOfSize(dictionary.Count);
        ///     dictionary.CopyTo(destinationArray, 0);
        /// }
        /// </code>
        /// </example>
        public override void CopyTo(Array array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                base.CopyTo(array, arrayIndex);
            }
        }


        /// <summary>
        /// Gets an Collection containing the keys in the System.Collections.Hashtable.
        /// </summary>
        public override ICollection Keys
        {
            get
            {
                lock (SyncRoot)
                {
                    return base.Keys;
                }
            }
        }


        /// <summary>
        /// Gets a Collection containing the values in the System.Collections.Hashtable.
        /// </summary>
        public override ICollection Values
        {
            get
            {
                lock (SyncRoot)
                {
                    return base.Values;
                }
            }
        }


        /// <summary>
        /// Removes the element with the specified key from the System.Collections.Hashtable.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public override void Remove(object key)
        {
            lock (SyncRoot)
            {
                base.Remove(key);
            }
        }

        #endregion


        #region "IXmlSerializable Members"

        /// <summary>
        /// Get XML schema
        /// </summary>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }


        /// <summary>
        /// Reads XML serialized data and fills the dictionary with them (the current dictionary content is cleared).
        /// </summary>
        /// <param name="reader">XML reader</param>
        /// <remarks>
        /// The operation is not performed in a critical section. If you want to make sure
        /// no other thread modifies the dictionary (e.g. adds/removes an item) while the dictionary
        /// is being filled with values from the XML, use the <see cref="SyncRoot"/>.
        /// </remarks>
        /// <example>
        /// Use the following snippet to make sure no other thread modifies the dictionary while
        /// it is being populated from the XML.
        /// <code>
        /// XmlReader xmlReader = GetReader(...);
        /// lock (dictionary.SyncRoot)
        /// {
        ///     dictionary.ReadXml(xmlReader);
        /// 
        ///     // The dictionary contains only entries from the XML
        /// }
        /// </code>
        /// 
        /// The following approach is also thread-safe, but the result may differ from the previous one if another thread performs any write operation
        /// while the XML is being read.
        /// <code>
        /// XmlReader xmlReader = GetReader(...);
        /// dictionary.ReadXml(xmlReader);
        /// 
        /// // Any other thread might have added or modified some entry from the XML, the dictionary's entries may differ from the XML content
        /// </code>
        /// </example>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Clear();

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }


        /// <summary>
        /// XML serialization method.
        /// </summary>
        /// <param name="writer">XML writer</param>
        /// <remarks>
        /// The operation writes a snapshot of the dictionary entries to XML.
        /// </remarks>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            lock (SyncRoot)
            {
                // The lock is necessary to make the snapshot consistent - no key should be removed
                // while enumerating
                foreach (TKey key in Keys)
                {
                    writer.WriteStartElement("item");

                    writer.WriteStartElement("key");
                    keySerializer.Serialize(writer, key);
                    writer.WriteEndElement();

                    writer.WriteStartElement("value");
                    TValue value = this[key];
                    valueSerializer.Serialize(writer, value);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
            }
        }

        #endregion
    }

    #endregion
}