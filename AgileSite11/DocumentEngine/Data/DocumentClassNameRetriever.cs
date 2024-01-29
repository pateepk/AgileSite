using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Retrieves page type class name from given <see cref="IDataContainer"/> data.
    /// </summary>
    public class DocumentClassNameRetriever
    {
        private IDataContainer Container
        {
            get;
            set;
        }


        private bool RequireClassName
        {
            get;
            set;
        }


        /// <summary>
        /// Creates an instance of <see cref="DocumentClassNameRetriever"/> class.
        /// </summary>
        /// <param name="container">Container with document data to retrieve the class name from.</param>
        /// <param name="requireClassName">Indicates if the class name is required when retrieved from the container.</param>
        /// <remarks>If the class name is required and the value is not retrieved from the data container <see cref="DocumentTypeNotExistsException"/> exception is thrown.</remarks>
        public DocumentClassNameRetriever(IDataContainer container, bool requireClassName)
        {
            Container = container;
            RequireClassName = requireClassName;
        }


        /// <summary>
        /// Gets the class name from the given data container.
        /// </summary>
        /// <remarks>
        /// Retrieves class name by probing these source columns in the container with priority given by the list order: ClassName, NodeClassID, <see cref="SystemColumns.SOURCE_TYPE"/>.
        /// </remarks>
        /// <exception cref="DocumentTypeNotExistsException">Thrown when class name is required and not found in any source column of the container data.</exception>
        public string Retrieve()
        {
            if (Container != null)
            {
                var probes = new List<Func<string>>
                {
                    GetClassNameFromName,
                    GetClassNameFromClassId,
                    GetClassNameFromObjectType
                };

                foreach (var probe in probes)
                {
                    var className = probe();
                    if (!string.IsNullOrEmpty(className))
                    {
                        return className;
                    }
                }
            }

            if (RequireClassName)
            {
                throw new DocumentTypeNotExistsException("Page type class not found in the data.");
            }

            return null;
        }


        private string GetClassNameFromClassId()
        {
            var classId = ValidationHelper.GetInteger(Container.GetValue("NodeClassID"), 0);
            if (classId <= 0)
            {
                return null;
            }

            return DataClassInfoProvider.GetClassName(classId);
        }


        private string GetClassNameFromName()
        {
            return ValidationHelper.GetString(Container.GetValue("ClassName"), null);
        }


        private string GetClassNameFromObjectType()
        {
            var objectType = ValidationHelper.GetString(Container.GetValue(SystemColumns.SOURCE_TYPE), null);
            return TreeNodeProvider.GetClassName(objectType);
        }
    }
}
