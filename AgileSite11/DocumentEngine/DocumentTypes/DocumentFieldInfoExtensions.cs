using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Extension methods for document related form field.
    /// </summary>
    public static class DocumentFormFieldInfoExtensions
    {
        /// <summary>
        /// Checks if form field can be used as source for document node name.
        /// </summary>
        public static bool IsNodeNameSourceCandidate(this FormFieldInfo field)
        {
            return !field.PrimaryKey && !field.AllowEmpty && ((field.DataType == FieldDataType.Text) || (field.DataType == FieldDataType.LongText));
        }
    }
}
