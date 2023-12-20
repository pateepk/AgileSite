using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Filter excluding object types according to the import/export configuration in each object type info.
    /// </summary>
    public class ImportObjectTypeFilter : IObjectTypeFilter
    {
        /// <summary>
        /// Returns true if the given object type is included in import.
        /// </summary>
        /// <param name="objectType">Object type name</param>
        public bool IsIncludedType(string objectType)
        {
            var objectTypeInfo = GetTypeInfo(objectType);
            if (objectTypeInfo == null)
            {
                var message =
                    $"Cannot instantiate {typeof(ObjectTypeInfo).FullName} from given object type '{objectType}'.\n" +
                    $"The object type is not known in the system.\n";
                throw new ArgumentException(message);
            }

            return objectTypeInfo.ImportExportSettings.SupportsExport;
        }


        /// <summary>
        /// Indicates whether given type info is a child processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="childTypeInfo">Child type info</param>
        public bool IsChildIncludedToParent(ObjectTypeInfo childTypeInfo)
        {
            return !childTypeInfo.ImportExportSettings.SupportsExport
                && (childTypeInfo.ImportExportSettings.IncludeToExportParentDataSet != IncludeToParentEnum.None);
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


        private static ObjectTypeInfo GetTypeInfo(string objectType)
        {
            return ObjectTypeManager.GetTypeInfo(objectType);
        }
    }
}
