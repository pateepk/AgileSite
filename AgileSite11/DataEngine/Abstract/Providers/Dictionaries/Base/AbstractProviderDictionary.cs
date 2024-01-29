using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Holds the collection of available provider dictionaries and provides basic set of actions.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public static class AbstractProviderDictionary
    {
        private static readonly CMSStatic<IDictionary<string, IProviderDictionary>> mDictionaries = new CMSStatic<IDictionary<string, IProviderDictionary>>(CreateDictionary);
        private static bool? mEnableHashTables;
        private const string DICTIONARY_NAME_SEPARATOR = "_";

        // Clear command.
        internal const string COMMAND_CLEAR = "##CLEAR##";

        // Invalidate command.
        internal const string COMMAND_INVALIDATE = "##INVALIDATE##";


        /// <summary>
        /// Collection of all provider dictionaries.
        /// </summary>
        public static IEnumerable<IProviderDictionary> Dictionaries
        {
            get
            {
                return mDictionaries.Value.Values;
            }
        }


        /// <summary>
        /// Enable hashtables.
        /// </summary>
        public static bool EnableHashTables
        {
            get
            {
                if (mEnableHashTables == null)
                {
                    mEnableHashTables = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEnableHashtables"], true);
                }

                return mEnableHashTables.Value;
            }
            set
            {
                mEnableHashTables = value;
            }
        }


        /// <summary>
        /// Creates new instance of dictionary collection.
        /// </summary>
        private static IDictionary<string, IProviderDictionary> CreateDictionary()
        {
            return new ConcurrentDictionary<string, IProviderDictionary>(StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Adds dictionary to the collection of all dictionaries.
        /// </summary>
        /// <param name="key">Dictionary name.</param>
        /// <param name="value">Dictionary instance.</param>
        internal static void Add(string key, IProviderDictionary value)
        {
            mDictionaries.Value[key] = value;
        }


        /// <summary>
        /// Gets the dictionary associated with the specified key.
        /// </summary>
        /// <param name="key">Dictionary name.</param>
        /// <param name="value">Dictionary instance.</param>
        internal static bool TryGetValue(string key, out IProviderDictionary value)
        {
            return mDictionaries.Value.TryGetValue(key, out value);
        }


        /// <summary>
        /// Gets the dictionary display name
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static string GetDictionaryDisplayName(IProviderDictionary dict)
        {
            var resStringKey = GetDisplayNameResourceKey(dict.Name);

            // Try to get the resource string name
            string name = ResHelper.GetAPIString(resStringKey, null, String.Empty);

            if (String.IsNullOrEmpty(name))
            {
                var obj = ModuleManager.GetReadOnlyObject(dict.ObjectType);
                if (obj != null)
                {
                    string res = null;

                    var objectTypeInfo = obj.TypeInfo;

                    var columnNames = dict.ColumnNames;

                    // Known hashtable columns (ID, GUID, Name)
                    if (columnNames.EqualsCSafe(objectTypeInfo.IDColumn, true))
                    {
                        res = "hashtable.byid";
                    }
                    else if (columnNames.StartsWithCSafe(objectTypeInfo.CodeNameColumn, true))
                    {
                        res = "hashtable.byname";
                    }
                    else if (columnNames.StartsWithCSafe(objectTypeInfo.GUIDColumn, true))
                    {
                        res = "hashtable.byguid";
                    }

                    // Generate the name automatically
                    if (!String.IsNullOrEmpty(res))
                    {
                        name = ResHelper.GetStringFormat(res, ResHelper.GetString(TypeHelper.GetTasksResourceKey(dict.ObjectType)));
                    }
                }
            }

            if (String.IsNullOrEmpty(name))
            {
                name = resStringKey;
            }

            return name;
        }


        /// <summary>
        /// Gets the display name resource key for the dictionary
        /// </summary>
        /// <param name="name">Name</param>
        internal static string GetDisplayNameResourceKey(string name)
        {
            var resStringKey = "HashTableName." + ValidationHelper.GetIdentifier(name);
            if (resStringKey.Length > 100)
            {
                resStringKey = resStringKey.Substring(0, 100);
            }
            return resStringKey;
        }


        /// <summary>
        /// Reloads the dictionaries of specified object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="logTasks">If true, the web farm tasks are logged</param>
        public static void ReloadDictionaries(string objectType, bool logTasks)
        {
            var dicts = GetDictionaries(objectType);
            foreach (var dict in dicts)
            {
                dict.Clear(logTasks);
            }
        }


        /// <summary>
        /// Returns enumerable of provider dictionaries which name starts with defined object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static IEnumerable<IProviderDictionary> GetDictionaries(string objectType)
        {
            string match = objectType + DICTIONARY_NAME_SEPARATOR;

            return Dictionaries
                    .Where(item => item.Name.StartsWith(match, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
        }


        /// <summary>
        /// Gets the dictionary name from provided keys.
        /// </summary>
        /// <param name="keys">Collection of keys used for name.</param>
        internal static string GetDictionaryName(params string[] keys)
        {
            return String.Join(DICTIONARY_NAME_SEPARATOR, keys.Where(key => key != null));
        }


        /// <summary>
        /// Processes the given web farm task.
        /// </summary>
        /// <param name="taskTarget">Task target</param>
        /// <param name="taskTextData">Task text data</param>
        /// <param name="taskBinaryData">Task binary data</param>
        internal static bool ProcessWebFarmTask(string taskTarget, string taskTextData, byte[] taskBinaryData)
        {
            IProviderDictionary dict = null;
            if (TryGetValue(taskTarget, out dict))
            {
                switch (taskTextData)
                {
                    // Clear command
                    case COMMAND_CLEAR:
                        dict.Clear(false);
                        break;

                    // Invalidate command
                    case COMMAND_INVALIDATE:
                        dict.Invalidate(false);
                        break;

                    // Remove the value(s) - default behavior
                    default:
                        dict.Remove(taskTextData, false);
                        break;
                }
                return true;
            }
            return false;
        }
    }
}