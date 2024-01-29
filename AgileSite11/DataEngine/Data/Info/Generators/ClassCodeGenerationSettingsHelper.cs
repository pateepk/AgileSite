using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    internal sealed class ClassCodeGenerationSettingsHelper
    {
        private const string LAST_MODIFIED = "lastmodified";
        private const string DISPLAY_NAME = "displayname";
        private const string NAME = "name";
        private const string SITE_ID = "siteid";


        /// <summary>
        /// Pre-fills settings from given class fields.
        /// Settings value is set only when original value is null er empty string.
        /// </summary>
        internal void PreFillSettings(IEnumerable<FieldInfo> fields, ref ClassCodeGenerationSettings settings)
        {
            foreach (var fieldInfo in fields)
            {
                // Get the first GUID column
                if (string.IsNullOrEmpty(settings.GuidColumn) && (fieldInfo.DataType == FieldDataType.Guid))
                {
                    settings.GuidColumn = fieldInfo.Name;
                }
                // Get the first binary column
                if (string.IsNullOrEmpty(settings.BinaryColumn) && (fieldInfo.DataType == FieldDataType.Binary))
                {
                    settings.BinaryColumn = fieldInfo.Name;
                }
                // Get the LastModified column
                if (string.IsNullOrEmpty(settings.LastModifiedColumn) && fieldInfo.Name.EndsWithCSafe(LAST_MODIFIED, true))
                {
                    settings.LastModifiedColumn = fieldInfo.Name;
                }
                // Get the display name column
                if (string.IsNullOrEmpty(settings.DisplayNameColumn) && fieldInfo.Name.EndsWithCSafe(DISPLAY_NAME, true) && (fieldInfo.DataType == FieldDataType.Text))
                {
                    settings.DisplayNameColumn = fieldInfo.Name;
                }
                // Get the code name column
                if (string.IsNullOrEmpty(settings.CodeNameColumn) && !fieldInfo.Name.EndsWithCSafe(DISPLAY_NAME, true) && fieldInfo.Name.EndsWithCSafe(NAME, true) && (fieldInfo.DataType == FieldDataType.Text))
                {
                    settings.CodeNameColumn = fieldInfo.Name;
                }
                // Get the display name column
                if (string.IsNullOrEmpty(settings.SiteIdColumn) && fieldInfo.Name.EndsWithCSafe(SITE_ID, true) && (fieldInfo.DataType == FieldDataType.Integer))
                {
                    settings.SiteIdColumn = fieldInfo.Name;
                }
            }
        }
    }
}
