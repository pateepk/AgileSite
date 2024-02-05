using System;

using CMS.Core;

namespace CMS.CustomTables
{
    /// <summary>
    /// Factory that provides custom table item objects
    /// </summary>
    public class CustomTableItemFactory : ObjectFactory<CustomTableItem>
    {
        /// <summary>
        /// Custom table item type
        /// </summary>
        public Type Type 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the custom table item class</param>
        public CustomTableItemFactory(Type type)
        {
            Type = type;
        }


        /// <summary>
        /// Creates new custom table item object
        /// </summary>
        public override object CreateNewObject()
        {
            return Activator.CreateInstance(Type);
        }
    }
}
