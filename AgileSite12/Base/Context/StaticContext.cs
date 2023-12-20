using System;
using System.Collections.Generic;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Static context properties
    /// </summary>
    internal class StaticContext : AbstractContext<StaticContext>
    {
        internal const int DEFAULT_INDEX = -1;
        private const int UNKNOWN_INDEX = -2;

        // Context mapping [name] -> [index]
        private static SafeDictionary<string, int> mContextMapping;

        // List of all registered static fields
        private static readonly List<ICMSStatic> mFields = new List<ICMSStatic>();

        private static int mLastContextIndex = DEFAULT_INDEX;
        private string mCurrentContextName;

        // Current context index. -1 = Main context. -2 = Unknown
        private int mCurrentContextIndex = UNKNOWN_INDEX;

        // If true, only main static context is used by the application
        internal static bool MainContextOnly = true;


        /// <summary>
        /// Current context index
        /// </summary>
        internal static int CurrentContextIndex
        {
            get
            {
                var c = Current;

                if (c.mCurrentContextIndex >= DEFAULT_INDEX)
                {
                    return c.mCurrentContextIndex;
                }

                return c.mCurrentContextIndex = GetContextIndex(CurrentContextName, true);
            }
        }
      

        /// <summary>
        /// Current context index
        /// </summary>
        internal static string CurrentContextName
        {
            get
            {
                return Current.mCurrentContextName;
            }
            set
            {
                var c = Current;

                c.mCurrentContextName = value;
                c.mCurrentContextIndex = UNKNOWN_INDEX;

                if (value != null)
                {
                    MainContextOnly = false;
                }
            }
        }


        /// <summary>
        /// Gets the context mapping dictionary [name] -> [index] 
        /// </summary>
        private static SafeDictionary<string, int> ContextMapping
        {
            get
            {
                if (mContextMapping == null)
                {
                    Interlocked.CompareExchange(ref mContextMapping, new SafeDictionary<string, int>(), null);
                }
                return mContextMapping;
            }
        }


        /// <summary>
        /// Gets the current context name
        /// </summary>
        /// <param name="contextName">Context name</param>
        /// <param name="createNew">If true, new index is created if context is not yet registered</param>
        private static int GetContextIndex(string contextName, bool createNew)
        {
            if (String.IsNullOrEmpty(contextName))
            {
                return DEFAULT_INDEX;
            }

            var mappings = ContextMapping;
            
            int result;
            if (!mappings.TryGetValue(contextName, out result) && createNew)
            {
                lock (mappings)
                {
                    if (!mappings.TryGetValue(contextName, out result))
                    {
                        result = ++mLastContextIndex;

                        mappings[contextName] = result;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Registers the given static field instance
        /// </summary>
        /// <param name="field">Field instance</param>
        internal static void Register(ICMSStatic field)
        {
            lock (mFields)
            {
                mFields.Add(field);
            }
        }
        

        /// <summary>
        /// Resets all existing static fields
        /// </summary>
        internal static void Reset()
        {
            mContextMapping = null;
            mLastContextIndex = DEFAULT_INDEX;
            MainContextOnly = true;

            CurrentContextName = null;

            lock (mFields)
            {
                foreach (var field in mFields)
                {
                    field.Reset();
                }
            }
        }
    }
}
