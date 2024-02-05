using System;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider dictionary.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class ProviderDictionary<TKey, TValue> : IProviderDictionary<TKey, TValue>
    {
        #region "Variables"

        internal const string WEBFARM_OPERATION_NAME = "DELETE";

        private bool mLogWebFarmTasks = true;
        private bool mDataIsValid = true;
        private bool mEnabled = true;
        private readonly ICacheStorage<TKey, TValue> mCache;
        private IWebFarmService mWebFarmService;

        #endregion


        #region "Properties"

        private IWebFarmService WebFarmService
        {
            get
            {
                return mWebFarmService ?? (mWebFarmService = CoreServices.WebFarm);
            }
            set
            {
                mWebFarmService = value;
            }
        }


        /// <summary>
        /// Dictionary indexer.
        /// </summary>
        /// <param name="key">Object key</param>
        public virtual TValue this[TKey key]
        {
            get
            {
                // Disabled - not not perform any action
                if (!Enabled)
                {
                    return default(TValue);
                }

                // Try to get the value
                TValue result;

                if (TryGetValue(key, out result))
                {
                    return result;
                }

                return default(TValue);
            }
            set
            {
                // Set the value
                Add(key, value);
            }
        }


        /// <summary>
        /// Items count (all keys).
        /// </summary>
        public long Count
        {
            get
            {
                return mCache.Count;
            }
        }


        /// <summary>
        /// Returns true if the Dictionary is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (Count == 0);
            }
        }


        /// <summary>
        /// Returns true if the dictionary content is valid.
        /// </summary>
        public bool DataIsValid
        {
            get
            {
                return mDataIsValid;
            }
        }


        /// <summary>
        /// Dictionary name.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Column names
        /// </summary>
        public string ColumnNames
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets / sets if the dictionary should log web farm tasks
        /// </summary>
        public bool LogWebFarmTasks
        {
            get
            {
                return mLogWebFarmTasks;
            }
            set
            {
                mLogWebFarmTasks = value;
            }
        }


        /// <summary>
        /// If true, the dictionary is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (!AbstractProviderDictionary.EnableHashTables)
                {
                    return false;
                }

                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates new instance of <see cref="ProviderDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        /// <param name="comparer">Equality comparer for the items</param>
        /// <param name="allowNulls">Indicates whether null value will be considered as valid and will be cached.</param>
        /// <param name="useWeakReferences">Indicates whether cache item can be removed from cache in case of insufficient memory.</param>
        public ProviderDictionary(string objectType, string columnNames, IEqualityComparer<TKey> comparer = null, bool allowNulls = false, bool useWeakReferences = false)
            : this(objectType, columnNames, comparer, null, allowNulls, useWeakReferences)
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="ProviderDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        /// <param name="comparer">Equality comparer for the items</param>
        /// <param name="customNameSuffix">Suffix used for name identifier. Suffix is required in cases where <paramref name="objectType"/> and <paramref name="columnNames"/> are used for more instances of <see cref="ProviderDictionary{TKey, TValue}"/>.</param>
        /// <param name="allowNulls">Indicates whether null value will be considered as valid and will be cached.</param>
        /// <param name="webFarmService">Web farm service used for web farm tasks.</param>
        /// <param name="useWeakReferences">Indicates whether cache item can be removed from cache in case of insufficient memory.</param>
        internal ProviderDictionary(string objectType, string columnNames, IEqualityComparer<TKey> comparer, string customNameSuffix, bool allowNulls, bool useWeakReferences = false, IWebFarmService webFarmService = null)
        {
            WebFarmService = webFarmService;

            ObjectType = objectType;
            ColumnNames = columnNames;

            Name = AbstractProviderDictionary.GetDictionaryName(objectType, columnNames, customNameSuffix);

            if (useWeakReferences)
            {
                mCache = new MemoryCacheStorage<TKey, TValue>(Name, allowNulls);
            }
            else
            {
                mCache = new ConcurrentDictionaryCacheStorage<TKey, TValue>(comparer, allowNulls);
            }

            AbstractProviderDictionary.Add(Name, this);
        }


        /// <summary>
        /// Converts the key to a specific type
        /// </summary>
        /// <param name="key">Key to convert</param>
        protected virtual TKey ConvertKey(object key)
        {
            return (TKey)key;
        }


        /// <summary>
        ///	Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Returns the object value if the object is present</param>
        [Obsolete("Use TryGetValue(TKey, out TValue) instead.")]
        public virtual bool Contains(TKey key, out TValue value)
        {
            return TryGetValue(key, out value);
        }


        /// <summary>
        ///	Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Returns the object value if the object is present</param>
        public bool TryGetValue(TKey key, out TValue value)
        {
            // Disabled - do not perform any action
            if (!Enabled)
            {
                value = default(TValue);
                return false;
            }

            // Try to get by original value
            return TryGetInternal(key, out value);
        }


        /// <summary>
        /// Returns true if the table contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        [Obsolete("Use ContainsKey(TKey) instead.")]
        public virtual bool Contains(TKey key)
        {
            return ContainsKey(key);
        }


        /// <summary>
        /// Returns true if the table contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        public bool ContainsKey(TKey key)
        {
            // Disabled - not not perform any action
            if (!Enabled)
            {
                return false;
            }

            return mCache.ContainsKey(key);
        }


        /// <summary>
        /// Removes the specified object.
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void Remove(TKey key)
        {
            Remove(key, false);
        }


        /// <summary>
        /// Removes the specified object and logs the web farm task.
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void Delete(TKey key)
        {
            Remove(key, true);
        }


        /// <summary>
        /// Removes the specified object.
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="logTask">If true, web farm task is logged</param>
        public virtual void Remove(TKey key, bool logTask)
        {
            RemoveInternal(key);

            // Log the web farm task
            if (logTask && LogWebFarmTasks)
            {
                LogWebFarmTask(key, WEBFARM_OPERATION_NAME);
            }
        }


        /// <summary>
        /// Removes the specified object.
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="logTask">If true, web farm task is logged</param>
        void IProviderDictionary.Remove(object key, bool logTask)
        {
            Remove(ConvertKey(key), logTask);
        }


        /// <summary>
        /// Adds the specified object.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value</param>
        public void Add(TKey key, TValue value)
        {
            Add(key, value, false);
        }


        /// <summary>
        /// Updates the specified object and logs the web farm task.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value</param>
        public virtual void Update(TKey key, TValue value)
        {
            Add(key, value, true);
        }


        /// <summary>
        /// Adds the specified object.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value</param>
        /// <param name="logTask">If true, web farm task is logged</param>
        public virtual void Add(TKey key, TValue value, bool logTask)
        {
            mCache.Add(key, value);

            // Log the web farm task
            if (logTask && LogWebFarmTasks)
            {
                LogWebFarmTask(key.ToString(), WEBFARM_OPERATION_NAME);
            }
        }


        /// <summary>
        /// Invalidates the dictionary content.
        /// </summary>
        /// <param name="logTask">If true, web farm task is logged</param>
        public virtual void Invalidate(bool logTask)
        {
            mDataIsValid = false;
            Clear(false);

            // Log invalidate task
            if (logTask && LogWebFarmTasks)
            {
                LogWebFarmTask(AbstractProviderDictionary.COMMAND_INVALIDATE, DataTaskType.DictionaryCommand);
            }
        }


        /// <summary>
        /// Clears all the items.
        /// </summary>
        /// <param name="logTask">If true, web farm task is logged</param>
        public void Clear(bool logTask)
        {
            mCache.Clear();

            // Log clear task
            if (logTask && LogWebFarmTasks)
            {
                LogWebFarmTask(AbstractProviderDictionary.COMMAND_CLEAR, DataTaskType.DictionaryCommand);
            }
        }


        /// <summary>
        /// Logs the web farm task for specified object key.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="taskOperation">Operation to realize with object</param>
        public void LogWebFarmTask(TKey key, string taskOperation)
        {
            LogWebFarmTask(key.ToString(), taskOperation);
        }


        /// <summary>
        /// Logs the web farm task with specified task data.
        /// </summary>
        /// <param name="taskTextData">Task text data</param>
        /// <param name="taskOperation">Operation to realize with object</param>
        public virtual void LogWebFarmTask(string taskTextData, string taskOperation)
        {
            WebFarmService.CreateTask(DataTaskType.DictionaryCommand, "", taskOperation, Name, taskTextData);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Removes the key from the internal dictionary
        /// </summary>
        /// <param name="key">Key to remove</param>
        protected virtual void RemoveInternal(TKey key)
        {
            mCache.Remove(key);
        }


        /// <summary>
        /// Returns true if the internal dictionary contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Returns the object value if the object is present</param>
        protected virtual bool TryGetInternal(TKey key, out TValue value)
        {
            return mCache.TryGet(key, out value);
        }

        #endregion
    }
}