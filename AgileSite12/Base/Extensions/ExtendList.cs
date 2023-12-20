namespace CMS.Base
{
    /// <summary>
    /// List extensions
    /// </summary>
    public static class ExtendList<ListType, ExtensionType>
    {
        /// <summary>
        /// Extends the object with the given static extension
        /// </summary>
        /// <param name="itemName">Item name</param>
        public static GenericProperty<ExtensionType> With(string itemName)
        {
            return Extend<ListType>.WithStaticProperty<ExtensionType>(itemName);
        }
    }
}
