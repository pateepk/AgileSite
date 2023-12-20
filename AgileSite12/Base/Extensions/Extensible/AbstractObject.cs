namespace CMS.Base
{
    /// <summary>
    /// Abstract class for the data container with no functionality
    /// </summary>
    public abstract class AbstractObject : IExtensible
    {
        private CMSLazy<ExtensionProperties> mExtensionProperties = new CMSLazy<ExtensionProperties>(() => new ExtensionProperties());


        /// <summary>
        /// Returns the extension property for the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public GenericProperty<PropertyType> Property<PropertyType>(string propertyName)
            where PropertyType : new()
        {
            return mExtensionProperties.Value.EnsureProperty<PropertyType>(this, propertyName);
        }
    }
}