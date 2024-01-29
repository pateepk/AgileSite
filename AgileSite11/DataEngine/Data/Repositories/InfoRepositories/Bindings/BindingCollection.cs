using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Info collection of object bindings
    /// </summary>
    public class BindingCollection : InfoObjectCollection
    {
        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public BindingCollection(string objectType)
            : base(objectType)
        {
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<BaseInfo> Clone()
        {
            // Create new instance and copy over the properties
            var result = new BindingCollection(ObjectType);
            CopyPropertiesTo(result);

            return result;
        }


        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Objects to add</param>
        public override void Add(params BaseInfo[] objects)
        {
            if (objects == null)
            {
                return;
            }

            // Add all objects
            foreach (var infoObj in objects)
            {
                if (infoObj != null)
                {
                    // If valid type, add directly
                    if (ValidateItem(infoObj, false))
                    {
                        base.Add(infoObj);
                    }
                    else
                    {
                        // Try to add binding through ID
                        var addId = GetTargetObjectId(infoObj);

                        Add(addId);
                    }
                }
            }
        }


        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Objects to add</param>
        public override void Remove(params BaseInfo[] objects)
        {
            if (objects == null)
            {
                return;
            }

            // Add all objects
            foreach (var infoObj in objects)
            {
                if (infoObj != null)
                {
                    // If valid type, add directly
                    if (ValidateItem(infoObj, false))
                    {
                        base.Remove(infoObj);
                    }
                    else
                    {
                        // Try to add binding through ID
                        var removeId = GetTargetObjectId(infoObj);

                        Remove(removeId);
                    }
                }
            }
        }


        /// <summary>
        /// Gets the target object ID for this collection
        /// </summary>
        /// <param name="infoObj">Object to process</param>
        protected virtual int GetTargetObjectId(BaseInfo infoObj)
        {
            string targetObjectType;
            GetBindingColumn(out targetObjectType);

            var ti = infoObj.TypeInfo;

            int objId = (ti.ObjectType != targetObjectType) ? 0 : infoObj.Generalized.ObjectID;
            if (objId <= 0)
            {
                throw new Exception(String.Format("[BindingCollection.Add]: Given object '{0}' of type '{1}' does not match neither the item type of the collection '{2}' nor target object type '{3}'.", infoObj.GetType().Name, ti.ObjectType, ObjectType, targetObjectType));
            }

            return objId;
        }


        /// <summary>
        /// Adds given bindings to the collection. Supported only for collections with parent defined. Returns newly created binding objects.
        /// </summary>
        /// <param name="bindingIds">Binding IDs to add</param>
        public virtual List<BaseInfo> Add(params int[] bindingIds)
        {
            // Get the binding column
            string targetObjectType;
            var bindingColumn = GetBindingColumn(out targetObjectType);
            
            var newObjects = new List<BaseInfo>();

            // Process all bindings
            foreach (var bindingId in bindingIds)
            {
                if (bindingId > 0)
                {
                    // Add the binding
                    var newObj = AddBinding(bindingColumn, bindingId);

                    newObjects.Add(newObj);
                }
            }

            return newObjects;
        }


        /// <summary>
        /// Removes given bindings from the collection. Supported only for collections with parent defined. Returns removed binding objects.
        /// </summary>
        /// <param name="bindingIds">Binding IDs to remove</param>
        public virtual List<BaseInfo> Remove(params int[] bindingIds)
        {
            // Get the binding column
            string targetObjectType;
            var bindingColumn = GetBindingColumn(out targetObjectType);

            var newObjects = new List<BaseInfo>();

            // Process all bindings
            foreach (var bindingId in bindingIds)
            {
                if (bindingId > 0)
                {
                    // Add the binding
                    var newObj = RemoveBinding(bindingColumn, bindingId);

                    newObjects.Add(newObj);
                }
            }

            return newObjects;
        }


        /// <summary>
        /// Adds given bindings to the collection. Supported only for collections with parent defined, and target object having code name column. Returns newly created binding objects.
        /// </summary>
        /// <param name="bindingCodeNames">Binding code names to add</param>
        public virtual List<BaseInfo> Add(params string[] bindingCodeNames)
        {
            // Get the binding column
            string targetObjectType;
            var bindingColumn = GetBindingColumn(out targetObjectType);

            var newObjects = new List<BaseInfo>();

            // Process all bindings
            foreach (var bindingCodeName in bindingCodeNames)
            {
                if (!String.IsNullOrEmpty(bindingCodeName))
                {
                    // Get target by code name
                    var targetObj = ProviderHelper.GetInfoByName(targetObjectType, bindingCodeName);
                    if (targetObj != null)
                    {
                        var bindingId = targetObj.Generalized.ObjectID;
                        
                        // Add the binding
                        var newObj = AddBinding(bindingColumn, bindingId);

                        newObjects.Add(newObj);
                    }
                }
            }

            return newObjects;
        }


        /// <summary>
        /// Removes given bindings from the collection. Supported only for collections with parent defined, and target object having code name column. Returns removed binding objects.
        /// </summary>
        /// <param name="bindingCodeNames">Binding code names to remove</param>
        public virtual List<BaseInfo> Remove(params string[] bindingCodeNames)
        {
            // Get the binding column
            string targetObjectType;
            var bindingColumn = GetBindingColumn(out targetObjectType);

            var newObjects = new List<BaseInfo>();

            // Process all bindings
            foreach (var bindingCodeName in bindingCodeNames)
            {
                if (!String.IsNullOrEmpty(bindingCodeName))
                {
                    // Get target by code name
                    var targetObj = ProviderHelper.GetInfoByName(targetObjectType, bindingCodeName);
                    if (targetObj != null)
                    {
                        var bindingId = targetObj.Generalized.ObjectID;

                        // Remove the binding
                        var newObj = RemoveBinding(bindingColumn, bindingId);

                        newObjects.Add(newObj);
                    }
                }
            }

            return newObjects;
        }


        /// <summary>
        /// Adds a new binding
        /// </summary>
        /// <param name="bindingColumn">Binding column</param>
        /// <param name="bindingId">Binding ID</param>
        protected virtual BaseInfo AddBinding(string bindingColumn, int bindingId)
        {
            // Create and add new binding object
            var newObj = CreateNewObject(null);
            newObj.SetValue(bindingColumn, bindingId);

            Add(newObj);
            return newObj;
        }


        /// <summary>
        /// Removes the binding
        /// </summary>
        /// <param name="bindingColumn">Binding column</param>
        /// <param name="bindingId">Binding ID</param>
        protected virtual BaseInfo RemoveBinding(string bindingColumn, int bindingId)
        {
            // Create and add new binding object
            var newObj = CreateNewObject(null);
            newObj.SetValue(bindingColumn, bindingId);

            Remove(newObj);
            return newObj;
        }


        /// <summary>
        /// Gets the binding column and object type
        /// </summary>
        /// <param name="targetObjectType">Returns target object type</param>
        protected virtual string GetBindingColumn(out string targetObjectType)
        {
            var ti = Object.TypeInfo;

            if (!ti.IsBinding)
            {
                throw new NotSupportedException("[BindingCollection.GetBindingColumn]: Object type '" + ObjectType + "' is not binding, cannot add binding to this collection through this method.");
            }

            string bindingColumn;

            // Locate the binding column and target type
            if (ti.IsSiteBinding)
            {
                bindingColumn = ti.SiteIDColumn;
                targetObjectType = PredefinedObjectType.SITE;
            }
            else
            {
                var bindingColumns = ti.ObjectDependencies.Where(d => (d.DependencyType == ObjectDependencyEnum.Binding) && !String.IsNullOrEmpty(d.DependencyColumn)).ToList();
                if (bindingColumns.Count != 1)
                {
                    throw new NotSupportedException("[BindingCollection.GetBindingColumn]: Object type '" + ObjectType + "' is not binding with single target column, cannot add binding to this collection through this method.");
                }

                bindingColumn = bindingColumns[0].DependencyColumn;
                targetObjectType = bindingColumns[0].DependencyObjectType;
            }

            return bindingColumn;
        }

        #endregion
    }
}
