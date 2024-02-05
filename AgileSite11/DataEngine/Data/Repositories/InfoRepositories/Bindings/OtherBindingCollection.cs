using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Info collection of object other bindings
    /// </summary>
    public class OtherBindingCollection : BindingCollection
    {
        #region "Properties"

        /// <summary>
        /// Bound object.
        /// </summary>
        public BaseInfo BoundObject
        {
            get;
            protected set;
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="boundObject">Bound object</param>
        public OtherBindingCollection(string objectType, BaseInfo boundObject)
            : base(objectType)
        {
            BoundObject = boundObject;
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<BaseInfo> Clone()
        {
            // Create new instance and copy over the properties
            var result = new OtherBindingCollection(ObjectType, BoundObject);
            CopyPropertiesTo(result);

            return result;
        }


        /// <summary>
        /// Ensures the proper values for the given object
        /// </summary>
        /// <param name="item">Item in which ensure the values</param>
        protected override void EnsureObjectValues(BaseInfo item)
        {
            base.EnsureObjectValues(item);

            // Get the binding column
            string targetObjectType;
            var bindingColumn = base.GetBindingColumn(out targetObjectType);

            // Set the bound object ID column
            item.SetValue(bindingColumn, BoundObject.Generalized.ObjectID);
        }
        

        /// <summary>
        /// Gets the binding column and object type
        /// </summary>
        /// <param name="targetObjectType">Returns target object type</param>
        protected override string GetBindingColumn(out string targetObjectType)
        {
            var ti = Object.TypeInfo;

            if (!ti.IsBinding)
            {
                throw new NotSupportedException("[BindingCollection.GetBindingColumn]: Object type '" + ObjectType + "' is not binding, cannot add binding to this collection through this method.");
            }

            // Get parent column as the binding column for other binding
            targetObjectType = ti.ParentObjectType;
            return ti.ParentIDColumn;
        }

        #endregion
    }
}
