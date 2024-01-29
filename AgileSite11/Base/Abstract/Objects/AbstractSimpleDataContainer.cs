using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Abstract class for the data container with no functionality
    /// </summary>
    public abstract class AbstractSimpleDataContainer<TObject> : ISimpleDataContainer
        where TObject : AbstractSimpleDataContainer<TObject>
    {
        #region "Variables"

        // Registered Columns
        private static RegisteredProperties<TObject> mRegisteredColumns;

        // Local registered Columns
        private RegisteredProperties<TObject> mLocalRegisteredColumns;

        // Registered properties sync root
        private static object registeredPropSyncRoot = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the object uses local columns
        /// </summary>
        protected bool UseLocalColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Registered Columns object
        /// </summary>
        protected RegisteredProperties<TObject> RegisteredColumnsObject
        {
            get
            {
                if (UseLocalColumns)
                {
                    return mLocalRegisteredColumns;
                }
               
                return mRegisteredColumns;
            }
            set
            {
                if (UseLocalColumns)
                {
                    mLocalRegisteredColumns = value;
                }
                else
                {
                    mRegisteredColumns = value;
                }
            }
        }
        

        /// <summary>
        /// Registered Columns
        /// </summary>
        protected RegisteredProperties<TObject> RegisteredColumns
        {
            get
            {
                if (RegisteredColumnsObject == null)
                {
                    lock (registeredPropSyncRoot)
                    {
                        if (RegisteredColumnsObject == null)
                        {
                            RegisteredColumnsObject = new RegisteredProperties<TObject>(RegisterColumns);
                        }
                    }
                }
                return RegisteredColumnsObject;            
            }
        }

        #endregion


        #region "Registration methods"

        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lamda expression for the getter of the property (object, parameter) => return value</param>
        protected PropertySettings<TObject> RegisterColumn(string columnName, Func<TObject, object> lambdaExpr)
        {
            var result = RegisteredColumns.Add<object>(columnName, lambdaExpr, null);
            result.PropertyType = typeof(string);

            return result;
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lamda expression for the getter of the property (object, parameter) => return value</param>
        protected PropertySettings<TObject> RegisterColumn<ColumnType>(string columnName, Func<TObject, object> lambdaExpr) 
        {
            return RegisteredColumns.Add<ColumnType>(columnName, lambdaExpr, null);
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lamda expression for the getter of the property (object, parameter) => return value</param>
        /// <param name="setLambdaExpr">Lamda expression for the setter of the property (object, parameter, value) => set</param>
        protected PropertySettings<TObject> RegisterColumn(string columnName, Func<TObject, object> lambdaExpr, Func<TObject, object, object> setLambdaExpr)
        {
            var result = RegisteredColumns.Add<object>(columnName, lambdaExpr, setLambdaExpr, null);
            result.PropertyType = typeof(string);

            return result;
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lamda expression for the getter of the property (object, parameter) => return value</param>
        /// <param name="setLambdaExpr">Lamda expression for the setter of the property (object, parameter, value) => set</param>
        protected PropertySettings<TObject> RegisterColumn<TColumn>(string columnName, Func<TObject, object> lambdaExpr, Func<TObject, object, object> setLambdaExpr) where TColumn : new()
        {
            return RegisteredColumns.Add<TColumn>(columnName, lambdaExpr, setLambdaExpr, null);
        }


        /// <summary>
        /// Registers the Columns of this object
        /// </summary>
        protected virtual void RegisterColumns()
        {
            RegisteredColumns.CollectColumns(GetType());
        }

        #endregion
       

        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractSimpleDataContainer()
        {
            TypeManager.RegisterGenericType(typeof(AbstractSimpleDataContainer<TObject>));
        }
        

        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetValue(string columnName, out object value)
        {
            return RegisteredColumns.Evaluate((TObject)this, columnName, out value);
        }


        /// <summary>
        /// Gets the object value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);

            return value;
        }


        /// <summary>
        /// Sets the object value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public virtual bool SetValue(string columnName, object value)
        {
            return RegisteredColumns.Set((TObject)this, columnName, value);
        }

        #endregion
    }
}