using System;

namespace CMS.Base
{
    /// <summary>
    /// Represents a generic property
    /// </summary>
    public class DynamicProperty<PropertyType> : GenericProperty<PropertyType>
    {
        /// <summary>
        /// Property value
        /// </summary>
        public override PropertyType Value 
        {
            get
            {
                if (Getter != null)
                {
                    return Getter();
                }
                else
                {
                    throw new NotSupportedException("Getter is not supported for this property.");
                }
            }
            set
            {
                if (Setter != null)
                {
                    Setter(value);
                }
                else
                {
                    throw new NotSupportedException("Setter is not supported for this property.");
                }
            }
        }


        /// <summary>
        /// Gets or sets the getter for the property
        /// </summary>
        protected Func<PropertyType> Getter
        {
            get;
            set;
        }

    
        /// <summary>
        /// Gets or sets the setter for the property
        /// </summary>
        protected Action<PropertyType> Setter
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="getter">Getter function</param>
        /// <param name="setter">Setter function</param>
        public DynamicProperty(string name, Func<PropertyType> getter, Action<PropertyType> setter = null)
            : base(name)
        {
            Getter = getter;
            Setter = setter;
        }
    }
}
