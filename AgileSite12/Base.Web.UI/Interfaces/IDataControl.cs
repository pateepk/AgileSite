using System;

using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Data control interface.
    /// </summary>
    public interface IDataControl : IRelatedData
    {
        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        bool SetValue(string propertyName, object value);


        /// <summary>
        /// Returns the value of the given web part property property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        object GetValue(string propertyName);


        /// <summary>
        /// Logs the evaluation of the given column to the debug
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="colsCount">Total number of available columns</param>
        void LogEval(string columnName, int colsCount);
    }
}