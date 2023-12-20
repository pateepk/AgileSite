using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Filter child object types according to the import/export configuration in each object type info.
    /// </summary>
    internal class ImportChildObjectTypeFilter : IObjectTypeFilter
    {
        /// <summary>
        /// Returns true if the given object type is included in import.
        /// </summary>
        /// <param name="objectType">Object type name</param>
        public bool IsIncludedType(string objectType)
        {
            // Include all child types even if not included to the parent to ensure backward compatibility. 
            // Customers could rely on event being raised for all child object types.
            return true;
        }


        /// <summary>
        /// Indicates whether given type info is a child processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="childTypeInfo">Child type info</param>
        public bool IsChildIncludedToParent(ObjectTypeInfo childTypeInfo)
        {
            return childTypeInfo.ImportExportSettings.IncludeToExportParentDataSet != IncludeToParentEnum.None;
        }


        /// <summary>
        /// Indicates whether given type info is a binding processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="bindingTypeInfo">Binding type info</param>
        public bool IsBindingIncludedToParent(ObjectTypeInfo bindingTypeInfo)
        {
            return !bindingTypeInfo.IsSiteBinding
                && !bindingTypeInfo.IsSelfBinding
                && IsChildIncludedToParent(bindingTypeInfo);
        }
    }
}
