using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Class structure info for testing purposes
    /// </summary>
    public class FakeClassStructureInfo<T> : FakeClassStructureInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FakeClassStructureInfo()
            : base(new InfoFakeSettings(typeof(T)))
        {
        }
    }


    /// <summary>
    /// Class structure info for testing purposes
    /// </summary>
    public class FakeClassStructureInfo : ClassStructureInfo
    {
        #region "Properties"

        /// <summary>
        /// Nested class structures
        /// </summary>
        public List<ClassStructureInfo> NestedClasses = null;


        /// <summary>
        /// Class type
        /// </summary>
        public Type ClassType
        {
            get;
            protected set;
        }


        /// <summary>
        /// 
        /// </summary>
        public bool GetFromParentType
        {
            get;
            set;
        }


        /// <summary>
        /// Class name.
        /// </summary>
        public new string ClassName
        {
            get
            {
                return base.ClassName;
            }
            set
            {
                base.ClassName = value;
            }
        }


        /// <summary>
        /// ID column name.
        /// </summary>
        public new string IDColumn
        {
            get
            {
                return base.IDColumn;
            }
            set
            {
                base.IDColumn = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Fake settings</param>
        public FakeClassStructureInfo(InfoFakeSettings settings)
        {
            ClassType = settings.Type;
            TableName = "[Unknown]";
            GetFromParentType = settings.IncludeInheritedFields;

            InitCollections();

            // Include columns from the type
            IncludeColumns(settings.Type);
        }


        /// <summary>
        /// Adds the nested class to this class structure info
        /// </summary>
        /// <param name="nested">Nested class</param>
        public void AddNestedClass(ClassStructureInfo nested)
        {
            if (NestedClasses == null)
            {
                NestedClasses = new List<ClassStructureInfo>();
            }

            NestedClasses.Add(nested);
        }


        /// <summary>
        /// Includes the columns from the given type
        /// </summary>
        /// <param name="type">Type from which the columns should be collected</param>
        public void IncludeColumns(Type type)
        {
            // Try to collect marked columns
            if (!RegisterColumns(type, true))
            {
                // If no marked fields found, register all fields
                RegisterColumns(type, false);
            }
        }


        /// <summary>
        /// Registers the columns from the given type. Returns true if some columns were registered
        /// </summary>
        /// <param name="type">Type from which the columns should be collected</param>
        /// <param name="onlyMarked">If true, only marked columns are registered</param>
        private bool RegisterColumns(Type type, bool onlyMarked)
        {
            // Register all properties
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            
            if (!GetFromParentType)
            {
                // Take only fields declared on type if not interested in inherited ones
                bindingFlags |= BindingFlags.DeclaredOnly;
            }

            if (onlyMarked)
            {
                // Marked fields are also allowed to be non-public
                bindingFlags |= BindingFlags.NonPublic;
            }

            var props = type.GetProperties(bindingFlags);
            bool registered = false;

            foreach (var p in props)
            {
                var colName = p.Name;
                var valueType = p.PropertyType;
                var register = !onlyMarked;

                // Register property with explicit flag RegisterProperty
                var attributes = Attribute.GetCustomAttributes(p, typeof(DatabaseFieldAttribute), true);
                if (attributes.Length == 1)
                {
                    var attr = (DatabaseFieldAttribute)attributes[0];

                    colName = attr.ColumnName ?? colName;
                    valueType = attr.ValueType ?? valueType;

                    // Set ID column
                    if (attr is DatabaseIDFieldAttribute)
                    {
                        IDColumn = colName;
                    }

                    register = true;
                }

                var underlyingType = Nullable.GetUnderlyingType(valueType);
                if (underlyingType != null)
                {
                    valueType = underlyingType;
                }
                
                if (register &&  DataTypeManager.IsKnownType(valueType))
                {
                    // Register column
                    RegisterColumn(colName, valueType);
                    registered = true;
                }
            }

            return registered;
        }


        /// <summary>
        /// Gets new data structure for class data as a DataSet.
        /// </summary>
        public override DataSet GetNewDataSet()
        {
            var ds = base.GetNewDataSet();
            var dt = ds.Tables[0];

            dt.TableName = ClassType.Name;

            // Add columns from nested classes
            if (NestedClasses != null)
            {
                foreach (var nested in NestedClasses)
                {
                    var nestedDs = nested.GetNewDataSet();
                    var nestedDt = nestedDs.Tables[0];

                    dt.Merge(nestedDt);
                }
            }

            return ds;
        }

        #endregion
    }
}
