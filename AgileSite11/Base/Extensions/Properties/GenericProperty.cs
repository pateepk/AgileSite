using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Represents a generic property
    /// </summary>
    public class GenericProperty<PropertyType> : GenericExtension<PropertyType>, IGenericProperty
    {
        #region "Variables"

        private PropertyType mValue = default(PropertyType);
        private string mName = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the property ensures an object instance in it's value if null
        /// </summary>
        public bool ValueIsSingleton
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Property initializer
        /// </summary>
        public Func<PropertyType> Initializer
        {
            get;
            protected set;
        }


        /// <summary>
        /// Property value
        /// </summary>
        public virtual PropertyType Value 
        {
            get
            {
                // Get from parent if parent set
                if (ParentProperty != null)
                {
                    return (PropertyType)ParentProperty.GetValue();
                }

                // Ensure the instance if configured
                if (mValue == null)
                {
                    if (Initializer != null)
                    {
                        // Use initializer
                        mValue = Initializer();
                    }
                    else if (ValueIsSingleton)
                    {
                        // Use singleton
                        mValue = ObjectFactory<PropertyType>.StaticSingleton();
                    }
                }

                return mValue;
            }
            set
            {
                if (ParentProperty != null)
                {
                    throw new NotSupportedException("[GenericProperty.Value]: Property must be set through it's original type.");
                }

                mValue = value;
            }
        }


        /// <summary>
        /// Property name
        /// </summary>
        public override string Name 
        {
            get
            {
                if (ParentProperty != null)
                {
                    return ParentProperty.Name;
                }

                return mName;
            }
            internal set
            {
                mName = value;
            }
        }


        /// <summary>
        /// Type of the property
        /// </summary>
        public Type Type
        {
            get
            {
                if (ParentProperty != null)
                {
                    return ParentProperty.Type;
                }

                return typeof(PropertyType);
            }
        }


        /// <summary>
        /// Parent property. If set, provides value to current extension
        /// </summary>
        public IGenericProperty ParentProperty
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor, creates a property without a name, which expects that its parent property will be set
        /// </summary>
        public GenericProperty()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Property name</param>
        public GenericProperty(string name)
        {
            Name = name;
        }


        /// <summary>
        /// Registers the extension as a property to the given type
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="propertyName">Property name</param>
        public void RegisterAsStaticPropertyTo(Type type, string propertyName)
        {
            Extension<PropertyType>.AddAsStaticProperty(type, propertyName, this);
        }


        /// <summary>
        /// Gets the property value for internal purposes
        /// </summary>
        public object GetValue()
        {
            return Value;
        }


        /// <summary>
        /// If set, the value of the object points to a singleton of this object
        /// </summary>
        public void AsSingleton()
        {
            ValueIsSingleton = true;
        }


        /// <summary>
        /// Sets up the lazy initialization over this property
        /// </summary>
        /// <param name="initializer">Property initializer</param>
        public void WithLazyInitialization(Func<PropertyType> initializer)
        {
            Initializer = initializer;
        }


        /// <summary>
        /// Implicit conversion to extension type
        /// </summary>
        /// <param name="ext">Extension to convert</param>
        public static implicit operator PropertyType(GenericProperty<PropertyType> ext)
        {
            return ext.Value;
        }

        #endregion
    }
}
