using System;

namespace CMS.Base
{
    /// <summary>
    /// Defines a property implementation
    /// </summary>
    public class Property<T>
    {
        #region "Properties"

        /// <summary>
        /// Property setter
        /// </summary>
        public Action<T> Setter
        {
            get;
            set;
        }


        /// <summary>
        /// Property getter
        /// </summary>
        public Func<T> Getter
        {
            get;
            set;
        }


        /// <summary>
        /// Inner property value
        /// </summary>
        protected T InnerValue
        {
            get;
            set;
        }


        /// <summary>
        /// Property value
        /// </summary>
        public T Value
        {
            get
            {
                if (Getter != null)
                {
                    return Getter();
                }

                return InnerValue;
            }
            set
            {
                Setter(value);
                InnerValue = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public Property()
        {
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getter">Property getter</param>
        /// <param name="setter">Property setter</param>
        public Property(Func<T> getter, Action<T> setter)
        {
            Getter = getter;
            Setter = setter;
        }


        /// <summary>
        /// Implicit conversion operator to property value
        /// </summary>
        /// <param name="prop">Property</param>
        public static implicit operator T(Property<T> prop)
        {
            if (prop == null)
            {
                return default(T);
            }

            return prop.Value;
        }

        #endregion
    }
}
