using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object property wrapper
    /// </summary>
    public class ObjectProperty
    {
        #region "Variables"

        /// <summary>
        /// Value
        /// </summary>
        protected object mValue = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Property name
        /// </summary>
        public string PropertyName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Parent object
        /// </summary>
        public ISimpleDataContainer Object
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor - Anonymous object property
        /// </summary>
        /// <param name="value">Property value</param>
        public ObjectProperty(object value)
        {
            mValue = value;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="propertyName">Property name</param>
        public ObjectProperty(IDataContainer infoObj, string propertyName)
        {
            if ((infoObj == null) || String.IsNullOrEmpty(propertyName))
            {
                throw new Exception("[ObjectProperty]: This object must be initialized with both object and property name.");
            }

            Object = infoObj;
            PropertyName = propertyName;
        }


        /// <summary>
        /// Property value
        /// </summary>
        public object Value
        {
            get
            {
                if (mValue != null)
                {
                    return mValue;
                }

                return Object.GetValue(PropertyName);
            }
        }


        /// <summary>
        /// Converts the object value to string
        /// </summary>
        public override string ToString()
        {
            return Value?.ToString() ?? String.Empty;
        }


        /// <summary>
        /// Injects the specified value to the property without modifying the object
        /// </summary>
        /// <param name="value">Value to inject</param>
        public void InjectValue(object value)
        {
            mValue = value;
        }

        #endregion
    }
}