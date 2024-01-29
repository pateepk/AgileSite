using System;
using System.Collections;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for field items.
    /// </summary>
    public interface IField : IDataDefinitionItem
    {
        #region "Properties"

        /// <summary>
        /// Column caption.
        /// </summary>
        string Caption
        {
            get;
            set;
        }


        /// <summary>
        /// Column name.
        /// </summary>
        string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field allow empty values.
        /// </summary>
        bool AllowEmpty
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field is a primary key.
        /// </summary>
        bool PrimaryKey
        {
            get;
            set;
        }


        /// <summary>
        /// Data type.
        /// </summary>
        string DataType
        {
            get;
            set;
        }


        /// <summary>
        /// Size of the field.
        /// </summary>
        int Size
        {
            get;
            set;
        }


        /// <summary>
        /// Precision of the field
        /// </summary>
        int Precision
        {
            get;
            set;
        }


        /// <summary>
        /// Field default value.
        /// </summary>
        string DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Field unique identifier.
        /// </summary>
        Guid Guid
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field is a system field.
        /// </summary>
        bool System
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field is unique.
        /// </summary>
        bool IsUnique
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field is external, if so it represents column from another table which is included in CMS_Tree_View_Joined (CMS_Document, CMS_Node, ...).
        /// </summary>
        bool External
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the field is inherited from parent class.
        /// </summary>
        bool IsInherited
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that field has no representation in database.
        /// </summary>
        bool IsDummyField
        {
            get;
            set;
        }


        /// <summary>
        /// If true the field was added into the main form else it was added into the alt.form (expects <see cref="IsDummyField"/> property to be true).
        /// </summary>
        bool IsDummyFieldFromMainForm
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that field is extra field (field is not in original form definition).
        /// </summary>
        bool IsExtraField
        {
            get;
            set;
        }


        /// <summary>
        /// Properties of the field
        /// </summary>
        Hashtable Properties
        {
            get;
            set;
        }


        /// <summary>
        /// Macro table for the field properties.
        /// </summary>
        Hashtable PropertiesMacroTable
        {
            get;
            set;
        }


        /// <summary>
        /// ObjectType to which the given field refers (for example as a foreign key).
        /// </summary>
        string ReferenceToObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the reference (used only when ReferenceToObjectType is set).
        /// </summary>
        ObjectDependencyEnum ReferenceType
        {
            get;
            set;
        }

        #endregion
    }
}