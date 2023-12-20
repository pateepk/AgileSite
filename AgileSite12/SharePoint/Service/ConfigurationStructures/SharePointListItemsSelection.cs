using System;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// <para>
    /// Holds a triplet for list items selection specification. The triplet is semantically equivalent to following SharePoint Query expression<br />
    /// &lt;FieldRef Name="FieldName" />&lt;Value Type="FieldType">FieldValue&lt;/Value><br />
    /// </para>
    /// <para>
    /// In order for the selection to be valid the <see cref="FieldName"/>, <see cref="FieldType"/> and <see cref="FieldValue"/> should be provided.
    /// </para>
    /// </summary>
    public class SharePointListItemsSelection
    {
        #region "Properties"
        
        /// <summary>
        /// Field name (internal name) on which to perform selection.
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Field type on which the selection is performed.
        /// </summary>
        public string FieldType
        {
            get;
            set;
        }


        /// <summary>
        /// Field value which is looked up in the list.
        /// </summary>
        public string FieldValue
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new list items selection with no parameters.
        /// </summary>
        public SharePointListItemsSelection()
        {
        }


        /// <summary>
        /// Creates a new list items selection with selection parameters specified.
        /// </summary>
        /// <param name="fieldName">Field name (internal name) on which to perform selection.</param>
        /// <param name="fieldType">Field type on which the selection is performed.</param>
        /// <param name="fieldValue">Field value which is looked up in the list.</param>
        public SharePointListItemsSelection(string fieldName, string fieldType, string fieldValue)
        {
            FieldName = fieldName;
            FieldType = fieldType;
            FieldValue = fieldValue;
        }

        #endregion
    }
}
