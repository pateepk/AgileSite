namespace CMS.Base
{
    /// <summary>
    /// Defines an object extensible by other objects as properties
    /// </summary>
    public interface IExtensible
    {
        /// <summary>
        /// Returns the property of the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        GenericProperty<PropertyType> Property<PropertyType>(string propertyName) where PropertyType : new();
    }
}
