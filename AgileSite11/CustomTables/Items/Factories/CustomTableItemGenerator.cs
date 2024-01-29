using CMS.Core;
using CMS.Base;

namespace CMS.CustomTables
{
    /// <summary>
    /// Generator of the specific custom table items
    /// </summary>
    public static class CustomTableItemGenerator
    {
        /// <summary>
        /// Object generator
        /// </summary>
        private static readonly ObjectGenerator<CustomTableItem> mGenerator = new ObjectGenerator<CustomTableItem>(new ObjectFactory<CustomTableItem>());


        /// <summary>
        /// Creates new instance of the given type
        /// </summary>
        /// <param name="className">Class name</param>
        public static ItemType NewInstance<ItemType>(string className) where ItemType : CustomTableItem, new()
        {
            // Create item with updated type info
            var item = (ItemType)mGenerator.CreateNewObject(className.ToLowerCSafe());
            item.UpdateTypeInfo(CustomTableItemProvider.GetTypeInfo(className));

            return item;
        }


        /// <summary>
        /// Registers the given custom table item class
        /// </summary>
        /// <param name="className">Class name to register</param>
        /// <param name="factory">Factory to use for new items</param>
        public static void RegisterCustomTable(string className, CustomTableItemFactory factory)
        {
            mGenerator.RegisterObjectType(className.ToLowerCSafe(), factory);
        }


        /// <summary>
        /// Registers the given custom table item class
        /// </summary>
        /// <param name="className">Class name to register</param>
        public static void RegisterCustomTable<ItemType>(string className) where ItemType : CustomTableItem, new()
        {
            mGenerator.RegisterObjectType<ItemType>(className.ToLowerCSafe());
        }


        /// <summary>
        /// Registers the default factory for the custom table items, which overlaps the default one
        /// </summary>
        /// <param name="factory">Factory to register</param>
        public static void RegisterDefaultFactory(CustomTableItemFactory factory)
        {
            mGenerator.RegisterDefaultFactory(factory, true);
        }
    }
}
