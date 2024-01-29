using System;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.Base;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Generator of the specific BizForm items
    /// </summary>
    public static class BizFormItemGenerator
    {
        /// <summary>
        /// Object generator
        /// </summary>
        private static readonly ObjectGenerator<BizFormItem> mGenerator = new ObjectGenerator<BizFormItem>(new ObjectFactory<BizFormItem>());


        /// <summary>
        /// Creates new instance of the given type
        /// </summary>
        /// <param name="className">Class name</param>
        public static ItemType NewInstance<ItemType>(string className) where ItemType : BizFormItem, new()
        {
            // Create item with updated type info
            var item = (ItemType)mGenerator.CreateNewObject(className.ToLowerCSafe());
            item.UpdateTypeInfo(BizFormItemProvider.GetTypeInfo(className));

            return item;
        }


        /// <summary>
        /// Registers the given BizForm item class
        /// </summary>
        /// <param name="className">Class name to register</param>
        /// <param name="factory">Factory to use for new items</param>
        public static void RegisterBizForm(string className, BizFormItemFactory factory)
        {
            mGenerator.RegisterObjectType(className.ToLowerCSafe(), factory);
        }


        /// <summary>
        /// Registers the given BizForm item class
        /// </summary>
        /// <param name="className">Class name to register</param>
        public static void RegisterBizForm<ItemType>(string className) where ItemType : BizFormItem, new()
        {
            mGenerator.RegisterObjectType<ItemType>(className.ToLowerCSafe());
        }


        /// <summary>
        /// Registers the default factory for the BizForm items, which overlaps the default one
        /// </summary>
        /// <param name="factory">Factory to register</param>
        public static void RegisterDefaultFactory(BizFormItemFactory factory)
        {
            mGenerator.RegisterDefaultFactory(factory, true);
        }
    }
}
