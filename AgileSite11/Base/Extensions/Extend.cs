using System;

namespace CMS.Base
{
    /// <summary>
    /// Extension storage
    /// </summary>
    public class Extend<ObjectType>
    {
        /// <summary>
        /// Extends the object with the given static extension
        /// </summary>
        public static void WithMetadata<MetadataType>()
            where MetadataType : IMetadata
        {
            var ext = new GenericExtension<MetadataContainer>();
            ext.Instance = new MetadataContainer(typeof (MetadataType));

            Extension<MetadataContainer>.AddTo(typeof(ObjectType), ext);
        }


        /// <summary>
        /// Extends the object with the given static extension
        /// </summary>
        public static ExtensionType With<ExtensionType>()
            where ExtensionType : new()
        {
            return Extension<ExtensionType>.AddTo(typeof(ObjectType)).Instance;
        }


        /// <summary>
        /// Extends the type with a new generic property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public static GenericExtension<PropertyType> WithProperty<PropertyType>(string propertyName)
            where PropertyType : new()
        {
            return Extension<PropertyType>.AddAsProperty(typeof(ObjectType), propertyName);
        }


        /// <summary>
        /// Extends the type with a new generic static property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public static GenericProperty<PropertyType> WithStaticProperty<PropertyType>(string propertyName)
        {
            return Extension<PropertyType>.AddAsStaticProperty(typeof(ObjectType), propertyName);
        }


        /// <summary>
        /// Extends the type with a new generic static property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="getter">Getter function</param>
        /// <param name="setter">Setter function</param>
        public static GenericProperty<PropertyType> WithStaticProperty<PropertyType>(string propertyName, Func<PropertyType> getter, Action<PropertyType> setter = null)
        {
            var prop = new DynamicProperty<PropertyType>(propertyName, getter, setter);
            prop.RegisterAsStaticPropertyTo(typeof(ObjectType), propertyName);

            return prop;
        }
    }
}
