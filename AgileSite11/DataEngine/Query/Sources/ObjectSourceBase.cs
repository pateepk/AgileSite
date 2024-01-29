using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query source
    /// </summary>
    public class ObjectSourceBase<TSource> : QuerySourceBase<TSource> 
        where TSource : ObjectSourceBase<TSource>, new()

    {
        #region "Variables"

        /// <summary>
        /// Object type.
        /// </summary>
        protected string mObjectType = null;

        /// <summary>
        /// Object instance of specified type.
        /// </summary>
        protected BaseInfo mObject = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns the object type of the objects stored within the collection.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return mObjectType;
            }
            set
            {
                mObjectType = value;
                mObject = null;

                TypeUpdated();
                Changed();
            }
        }


        /// <summary>
        /// Object instance of the specified type.
        /// </summary>
        protected BaseInfo Object
        {
            get
            {
                if (mObject == null)
                {
                    mObject = ModuleManager.GetReadOnlyObject(ObjectType);

                    if (mObject == null)
                    {
                        throw new Exception("[ObjectQueryBase.Object]: Object type '" + ObjectType + "' not found.");
                    }
                }

                return mObject;
            }
            set
            {
                mObject = value;
                mObjectType = null;

                // Setup the object type and class name
                if (value != null)
                {
                    mObjectType = value.TypeInfo.ObjectType;
                }

                TypeUpdated();
                Changed();
            }
        }


        /// <summary>
        /// Class name
        /// </summary>
        protected string ClassName
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectSourceBase()
        {
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public ObjectSourceBase(string objectType)
            : base(objectType)
        {
            ObjectType = objectType;
        }


        /// <summary>
        /// Initializes the query from the given type
        /// </summary>
        protected void InitFromType<T>()
            where T : BaseInfo, new()
        {
            Object = new T();
        }


        /// <summary>
        /// Updates the query class name based on the current status
        /// </summary>
        protected virtual void TypeUpdated()
        {
            if (!String.IsNullOrEmpty(ObjectType))
            {
                ClassName = Object.TypeInfo.ObjectClassName;
                
                SourceExpression = GetTableName(ClassName);
            }
        }


        /// <summary>
        /// Translates the source to the final query expression
        /// </summary>
        /// <param name="source">Source table</param>
        protected override void TranslateSource(QuerySourceTable source)
        {
            // Try to translate source expression to the object type
            var obj = ModuleManager.GetReadOnlyObject(source.Expression);
            if (obj != null)
            {
                source.Expression = GetTableName(obj.TypeInfo.ObjectClassName);
            }

            base.TranslateSource(source);
        }
        

        /// <summary>
        /// Gets the table name for the given class name
        /// </summary>
        /// <param name="className">Class name</param>
        private static string GetTableName(string className)
        {
            string tableName = null;
            
            // Get data class
            var dci = DataClassInfoProvider.GetDataClassInfo(className);
            if (dci != null)
            {
                tableName = dci.ClassTableName;
            }

            return tableName;
        }

        #endregion
    }
}
