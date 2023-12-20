using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Field data types for document engine - string constants.
    /// </summary>
    public class DocumentFieldDataType
    {
        /// <summary>
        /// Document attachments.
        /// </summary>
        public const string DocAttachments = FieldDataType.DocAttachments;


        /// <summary>
        /// Document relationships
        /// </summary>
        public const string DocRelationships = FieldDataType.DocRelationships;


        /// <summary>
        /// Register the document engine data types
        /// </summary>
        internal static void RegisterDataTypes()
        {
            DataTypeManager.RegisterDataTypes(
                new DataType<string>("nvarchar(1)", DocAttachments, "xs:string", ValidationHelper.GetString)
                {
                    TypeAlias = "string",
                    SqlValueFormat = DataTypeManager.UNICODE,
                    CodeValueFormat = DataTypeManager.CODE_VALUE_FORMAT_STRING,
                    DefaultValueCode = "String.Empty",

                    DefaultValue = "",
                    HasConfigurableDefaultValue = false,
                    AllowAsAliasSource = false,
                    DbType = SqlDbType.NVarChar,
                    TypeGroup = "DocAttachments",
                    AllowedObjectTypes = new []
                    {
                        DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE
                    }
                },
                // V9 new types
                new DataType<string>("nvarchar(1)", DocRelationships, "xs:string", ValidationHelper.GetString)
                {
                    TypeAlias = "string",
                    SqlValueFormat = DataTypeManager.UNICODE,
                    CodeValueFormat = DataTypeManager.CODE_VALUE_FORMAT_STRING,
                    DefaultValueCode = "String.Empty",

                    DefaultValue = "",
                    HasConfigurableDefaultValue = false,
                    AllowAsAliasSource = false,
                    DbType = SqlDbType.NVarChar,
                    TypeGroup = "DocRelationships",
                    AllowedObjectTypes = new []
                    {
                        DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE
                    }
                }
            );
        }
    }
}
