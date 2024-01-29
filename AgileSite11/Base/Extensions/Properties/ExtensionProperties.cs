using System;

namespace CMS.Base
{
    /// <summary>
    /// Extension properties
    /// </summary>
    public class ExtensionProperties
    {
        private TwoLevelDictionary<Type, string, IGenericProperty> mProperties = new TwoLevelDictionary<Type, string, IGenericProperty>();


        /// <summary>
        /// Ensures that the given property is properly initialized
        /// </summary>
        /// <param name="obj">Parent object</param>
        /// <param name="propertyName">Property name</param>
        /// <exception cref="InvalidOperationException">Thrown when property of name from <paramref name="propertyName"/> is not found in <paramref name="obj"/>.</exception>
        public GenericProperty<PropertyType> EnsureProperty<PropertyType>(object obj, string propertyName)
        {
            var type = typeof(PropertyType);

            var prop = mProperties[type, propertyName];
            if (prop == null)
            {
                var ext = Extension<PropertyType>.GetPropertyForObject(obj, propertyName);
                if (ext == null)
                {
                    string typeName = obj.GetType().Name;

                    throw new InvalidOperationException("Property '" + propertyName + "' not found in the object of type '" + typeName + "', call Extend<" + typeName + ">.WithProperty<" + type.Name + ">(\"" + propertyName + "\") to register the property.");
                }

                if (ext is DynamicProperty<PropertyType>)
                {
                    // Dynamic property
                    prop = (IGenericProperty)ext;
                }
                else
                {
                    // Simple property
                    prop = ext.NewProperty(obj);
                }
                
                mProperties[type, propertyName] = prop;
            }

            return (GenericProperty<PropertyType>)prop;
        }
    }
}
