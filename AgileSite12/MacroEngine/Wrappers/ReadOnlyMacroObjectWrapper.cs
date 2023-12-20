using System;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper for read-only version of the hierarchical object.
    /// </summary>
    public class ReadOnlyMacroObjectWrapper<TObject> : ReadOnlyAbstractHierarchicalObject<ReadOnlyMacroObjectWrapper<TObject>>
    {
        #region "Variables"

        private readonly TObject mObjectInstance;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectInstance">Object instance</param>
        public ReadOnlyMacroObjectWrapper(TObject objectInstance)
            : this()
        {
            mObjectInstance = objectInstance;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ReadOnlyMacroObjectWrapper()
        {
        }

        #endregion

        
        #region "Methods"

        /// <summary>
        /// Registers the given parameterized property to the object.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="parameter">Parameter for the lambda expression</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterProperty(string propertyName, object parameter, Func<TObject, object, object> ex)
        {
            return RegisterProperty(propertyName, parameter, (w, p) => ex(w.mObjectInstance, p));
        }


        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterProperty(string propertyName, Func<TObject, object> ex)
        {
            return RegisterProperty(propertyName, w => ex(w.mObjectInstance));
        }


        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterProperty<PropertyType>(string propertyName, Func<TObject, object> ex) where PropertyType : new()
        {
            return RegisterProperty<PropertyType>(propertyName, w => ex(w.mObjectInstance));
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lambda expression for the getter of the property (object, parameter) => return value</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterColumn(string columnName, Func<TObject, object> lambdaExpr)
        {
            return RegisterColumn(columnName, w => lambdaExpr(w.mObjectInstance));
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lambda expression for the getter of the property (object, parameter) => return value</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterColumn<ColumnType>(string columnName, Func<TObject, object> lambdaExpr) where ColumnType : new()
        {
            return RegisterColumn<ColumnType>(columnName, w => lambdaExpr(w.mObjectInstance));
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lambda expression for the getter of the property (object, parameter) => return value</param>
        /// <param name="setLambdaExpr">Lambda expression for the setter of the property (object, parameter, value) => set</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterColumn(string columnName, Func<TObject, object> lambdaExpr, Func<TObject, object, object> setLambdaExpr)
        {
            return RegisterColumn(columnName, w => lambdaExpr(w.mObjectInstance), (w, p) => setLambdaExpr(w.mObjectInstance, p));
        }


        /// <summary>
        /// Registers the given Column to the object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="lambdaExpr">Lambda expression for the getter of the property (object, parameter) => return value</param>
        /// <param name="setLambdaExpr">Lambda expression for the setter of the property (object, parameter, value) => set</param>
        protected PropertySettings<ReadOnlyMacroObjectWrapper<TObject>> RegisterColumn<ColumnType>(string columnName, Func<TObject, object> lambdaExpr, Func<TObject, object, object> setLambdaExpr) where ColumnType : new()
        {
            return RegisterColumn<ColumnType>(columnName, w => lambdaExpr(w.mObjectInstance), (w, p) => setLambdaExpr(w.mObjectInstance, p));
        }

        #endregion
    }
}