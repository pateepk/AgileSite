using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Settings object for general properties registering in BaseGenericInfo.
    /// </summary>
    public class PropertySettings<InfoType>
    {
        #region "Variables"

        /// <summary>
        /// Property name
        /// </summary>
        protected string mName = null;

        /// <summary>
        /// Parameter passed to the parametrized.
        /// </summary>
        protected object mParameter = null;


        /// <summary>
        /// Property getter without parameters.
        /// </summary>
        protected Func<InfoType, object> mPropertyFunction = null;

        /// <summary>
        /// Property setter without parameters
        /// </summary>
        protected Action<InfoType, object> mPropertySetFunction = null;

        /// <summary>
        /// Property getter with one parameter.
        /// </summary>
        protected Func<InfoType, object, object> mParametrizedFunction = null;

        /// <summary>
        /// Property setter with one parameter
        /// </summary>
        protected Action<InfoType, object, object> mParametrizedSetFunction = null;

        /// <summary>
        /// Property type
        /// </summary>
        protected Type mPropertyType = null;

        /// <summary>
        /// Empty object
        /// </summary>
        protected object mEmptyObject = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Empty object of the given type
        /// </summary>
        internal object EmptyObject
        {
            get
            {
                if ((mEmptyObject == null) && (EmptyObjectFactory != null))
                {
                    mEmptyObject = EmptyObjectFactory.CreateNewObject();
                }

                return mEmptyObject;
            }
            set
            {
                mEmptyObject = value;
            }
        }

        
        /// <summary>
        /// Object factory that provides an empty object to properly determine dynamic type
        /// </summary>
        public IObjectFactory EmptyObjectFactory 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType
        {
            get
            {
                if ((mPropertyType == null) && (EmptyObject != null))
                {
                    mPropertyType = EmptyObject.GetType();
                }

                return mPropertyType;
            }
            set
            {
                mPropertyType = value;
            }
        }


        /// <summary>
        /// If true, this property is hidden (doesn't report in properties list)
        /// </summary>
        public bool Hidden
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of property
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="lambdaExpr">Lamda expression for the getter of the property</param>
        /// <param name="setLambdaExpr">Lamda expression for the setter of the property</param>
        /// <param name="propertyType">Property type</param>
        public PropertySettings(string name, Func<InfoType, object> lambdaExpr, Action<InfoType, object> setLambdaExpr, Type propertyType)
        {
            mName = name;
            mPropertyFunction = lambdaExpr;
            mPropertySetFunction = setLambdaExpr;

            PropertyType = propertyType;
        }


        /// <summary>
        /// Creates new instance of parametrized property with a setter
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="lambdaExpr">Lamda expression for the getter of the property (object, parameter) => return value</param>
        /// <param name="setLambdaExpr">Lamda expression for the setter of the property (object, parameter, value) => set</param>
        /// <param name="parameter">Parameter to pass to the lambda functions</param>
        /// <param name="propertyType">Property type</param>
        public PropertySettings(string name, Func<InfoType, object, object> lambdaExpr, Action<InfoType, object, object> setLambdaExpr, object parameter, Type propertyType)
        {
            mName = name;
            mParametrizedFunction = lambdaExpr;
            mParametrizedSetFunction = setLambdaExpr;
            mParameter = parameter;

            PropertyType = propertyType;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if given property has a setter
        /// </summary>
        public bool HasSetter()
        {
            return (mPropertySetFunction != null) || (mParametrizedSetFunction != null);
        }


        /// <summary>
        /// Evaluates the property.
        /// </summary>
        /// <param name="info">Info object to evaluate the lambda expression on</param>
        /// <param name="newValue">New value to set to the property</param>
        public void Set(InfoType info, object newValue)
        {
            if (mPropertySetFunction != null)
            {
                mPropertySetFunction(info, newValue);
            }
            else if (mParametrizedSetFunction != null)
            {
                mParametrizedSetFunction(info, mParameter, newValue);
            }
            else
            {
                throw new Exception("Property '" + mName + "' does not have the setter registered.");
            }
        }


        /// <summary>
        /// Evaluates the property.
        /// </summary>
        /// <param name="info">Info object to evaluate the lambda expression on</param>
        public object Evaluate(InfoType info)
        {
            if (mPropertyFunction != null)
            {
                return mPropertyFunction(info);
            }
            else if (mParametrizedFunction != null)
            {
                return mParametrizedFunction(info, mParameter);
            }
            return null;
        }


        /// <summary>
        /// Sets the empty object to the factory
        /// </summary>
        public void SetEmptyObject(object obj)
        {
            EmptyObject = obj;
        }

        #endregion
    }
}