using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Generic extension container
    /// </summary>
    public class GenericExtension<ExtensionType> : IGenericExtension
    {
        #region "Variables"

        private ExtensionType mInstance;
        private Lazy<ExtensionType> mLazyInstance;
        private bool mInitialized;

        #endregion

        
        #region "Properties"

        /// <summary>
        /// Property value. Initializes the extension object if not available
        /// </summary>
        public ExtensionType Instance
        {
            get
            {
                // If parent extension set, provide its instance
                if (ParentExtension != null)
                {
                    return (ExtensionType)ParentExtension.GetInstance();
                }

                // Initialize the value
                Initialize();

                return mInstance;
            }
            internal set
            {
                // If parent extension set, provide its instance
                if (ParentExtension != null)
                {
                    ParentExtension.SetInstance(value);
                }
                else
                {
                    mInstance = value;
                    mInitialized = true;
                }
            }
        }


        /// <summary>
        /// Lazy instance of the extension
        /// </summary>
        public Lazy<ExtensionType> LazyInstance
        {
            get
            {
                return mLazyInstance ?? (mLazyInstance = new Lazy<ExtensionType>(() => Instance));
            }
        }


        /// <summary>
        /// Returns true if the extension is initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return mInitialized;
            }
        }


        /// <summary>
        /// Property initializer
        /// </summary>
        protected Func<object, ExtensionType> PropertyInitializer
        {
            get;
            set;
        }


        /// <summary>
        /// Parent extension. If set, provides instance object to current extension
        /// </summary>
        public IGenericExtension ParentExtension
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the extension
        /// </summary>
        public virtual string Name
        {
            get;
            internal set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the extension object instance
        /// </summary>
        public object GetInstance()
        {
            return Instance;
        }


        /// <summary>
        /// Sets the extension object instance
        /// </summary>
        /// <param name="value">New instance value</param>
        public void SetInstance(object value)
        {
            Instance = (ExtensionType)value;
        }
        

        /// <summary>
        /// Initializes the extension
        /// </summary>
        public ExtensionType Initialize()
        {
            if (!mInitialized)
            {
                mInstance = NewValue();

                mInitialized = true;
            }

            return mInstance;
        }


        /// <summary>
        /// Creates a new value of the given type
        /// </summary>
        public ExtensionType NewValue()
        {
            return ObjectFactory<ExtensionType>.New();
        }


        /// <summary>
        /// Creates a new property of the given type, initialized to the extension value
        /// </summary>
        /// <param name="obj">Parent object for the property</param>
        public IGenericProperty NewGenericProperty(object obj)
        {
            return NewProperty(obj);
        }


        /// <summary>
        /// Creates a new property of the given type, initialized to the extension value
        /// </summary>
        /// <param name="obj">Parent object for the property</param>
        public GenericProperty<ExtensionType> NewProperty(object obj)
        {
            var result = new GenericProperty<ExtensionType>();
            result.Name = Name;

            // Create link to the property provided by the parent extension
            if (ParentExtension != null)
            {
                result.ParentProperty = ParentExtension.NewGenericProperty(obj);
            }
            else if (PropertyInitializer != null)
            {
                // Setup the property initializer
                result.WithLazyInitialization(() => PropertyInitializer(obj));
            }
            else
            {
                // Ensure the property value
                result.Value = NewValue();
            }

            return result;
        }


        /// <summary>
        /// Sets up the lazy initialization over this property
        /// </summary>
        /// <param name="initializer">Property initializer</param>
        public void WithLazyInitialization(Func<object, ExtensionType> initializer)
        {
            if (typeof (ExtensionType).IsValueType)
            {
                throw new NotSupportedException("[GenericExtension.WithLazyInitialization]: Value type extensions does not support lazy initialization.");
            }

            PropertyInitializer = initializer;
        }


        /// <summary>
        /// Registers the extension as a property to the given type
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="propertyName">Property name</param>
        public void RegisterAsPropertyTo(Type type, string propertyName)
        {
            Extension<ExtensionType>.AddAsProperty(type, propertyName, this);
        }


        /// <summary>
        /// Registers the extension as an extension to the given type
        /// </summary>
        /// <param name="type">Target type</param>
        public void RegisterAsExtensionTo(Type type)
        {
            Extension<ExtensionType>.AddTo(type, this);
        }

        #endregion
    }
}
