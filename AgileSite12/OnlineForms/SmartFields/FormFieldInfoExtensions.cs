using System.Linq;

using CMS.Base;
using CMS.FormEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Extension methods for <see cref="FormFieldInfo"/> class.
    /// </summary>
    public static class FormFieldInfoExtensions
    {
        /// <summary>
        /// Returns true when given <paramref name="fieldInfo"/> is defined as smart one.
        /// </summary>
        public static bool IsSmartField(this FormFieldInfo fieldInfo)
        {
            return fieldInfo.GetPropertyValue(FormFieldPropertyEnum.Smart).ToBoolean(false);
        }


        /// <summary>
        /// Returns true when given <paramref name="formInfo"/> contains at least one field defined as smart one.
        /// </summary>
        public static bool ContainsSmartField(this FormInfo formInfo)
        {
            return formInfo.GetFields(true, true, false).Any(i => i.IsSmartField());
        }
    }
}
