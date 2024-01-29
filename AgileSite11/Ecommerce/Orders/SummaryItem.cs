using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class used for displaying order summaries (discounts, taxes, other payments) in Invoice, Email template and Shopping cart.
    /// </summary>
    [Serializable]
    [XmlType("Item")]
    [DebuggerDisplay("{" + nameof(Value) + "}", Name = "{" + nameof(Name) + "}")]
    public sealed class SummaryItem : IDataContainer
    {
        #region "Properties"

        /// <summary>
        /// Summary item name used for displaying in Invoice, Email template and Shopping cart.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Summary item value in shopping cart currency used for displaying in Invoice, Email template and Shopping cart.
        /// </summary>
        public decimal Value
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryItem"/> class.
        /// </summary>
        public SummaryItem()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryItem"/> class with the specified <paramref name="name"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="name">Name of the summary item.</param>
        /// <param name="value">Value of the summary item.</param>
        public SummaryItem(string name, decimal value)
        {
            Name = name;
            Value = value;
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
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
        /// Gets the value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);
            return value;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// DiscountSummaryItem object property names.
        /// </summary>
        [XmlIgnore]
        public List<string> ColumnNames => new List<string>
        {
            "Value",
            "Name"
        };


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            switch (columnName.ToLowerInvariant())
            {
                case "name":
                    value = Name;
                    return true;

                case "value":
                    value = Value;
                    return true;
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName);
        }

        #endregion
    }
}
