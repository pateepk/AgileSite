using System;
using System.Collections.Concurrent;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Static object wrapper which provides static variables based on the current context name
    /// </summary>
    public class CMSStatic<TValue> : ICMSStatic
    {
        private readonly TValue mDefaultValue;
        private readonly Func<TValue> mInitMethod;
        private TValue mMainValue;
        private readonly ConcurrentDictionary<int, TValue> values = new ConcurrentDictionary<int, TValue>();


        /// <summary>
        /// Property value
        /// </summary>
        public TValue Value
        {
            get
            {
                // Optimization for a single context
                if (StaticContext.MainContextOnly)
                {
                    return mMainValue;
                }

                // Get context index if multiple context used
                var contextIndex = StaticContext.CurrentContextIndex;

                // Optimization for the main context
                if (contextIndex < 0)
                {
                    return mMainValue;
                }

                return values.GetOrAdd(contextIndex, GetDefaultValue());
            }
            set
            {
                // Optimization for a single context
                if (StaticContext.MainContextOnly)
                {
                    mMainValue = value;
                }
                else
                {
                    // Get context index if multiple context used
                    var contextIndex = StaticContext.CurrentContextIndex;
                    if (contextIndex < 0)
                    {
                        // Optimization for the main context
                        mMainValue = value;
                    }
                    else
                    {
                        values[contextIndex] = value;
                    }
                }
            }
        }


        /// <summary>
        /// Static constructor
        /// </summary>
        static CMSStatic()
        {
            TypeManager.RegisterGenericType(typeof(CMSStatic<TValue>));
        }


        /// <summary>
        /// Empty constructor
        /// </summary>
        public CMSStatic()
        {
            StaticContext.Register(this);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Initial value</param>
        [Obsolete("Use CMSStatic constructor with initializer.")]
        public CMSStatic(TValue value) 
            : this()
        {
            mDefaultValue = value;

            InitializeMainValue();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initMethod">Initializing method for a new instance of the value</param>
        public CMSStatic(Func<TValue> initMethod)
            : this()
        {
            mInitMethod = initMethod;

            InitializeMainValue();
        }


        /// <summary>
        /// Initializes the main value
        /// </summary>
        private void InitializeMainValue()
        {
            mMainValue = GetDefaultValue();
        }


        /// <summary>
        /// Resets the static field to its original state
        /// </summary>
        public void Reset()
        {
            InitializeMainValue();

            values.Clear();
        }


        /// <summary>
        /// Gets the default value for a new context
        /// </summary>
        protected TValue GetDefaultValue()
        {
            if (mInitMethod != null)
            {
                return mInitMethod();
            }

            return mDefaultValue;
        }


        /// <summary>
        /// Implicit conversion from the property to its value
        /// </summary>
        /// <param name="property">Property to convert</param>
        public static implicit operator TValue(CMSStatic<TValue> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property), "Failed to access the static object, the main CMSStatic object is initialized with null or set to null which is not permitted.");
            }

            return property.Value;
        }
    }
}
