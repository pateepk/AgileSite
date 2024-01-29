using System.Collections.Generic;
using System.Linq;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Custom classes event handling
    /// Re-stores existing items when data class is changed or deleted 
    /// </summary>
    internal class ContinuousIntegrationCustomClassHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        internal static void Init()
        {
            ObjectEvents.Update.Before += UpdateCustomClassRepository;
        }


        private static void UpdateCustomClassRepository(object sender, ObjectEventArgs objectEventArgs)
        {
            var dataClassInfo = objectEventArgs.Object as DataClassInfo;

            foreach (var typeInfo in GetObjectTypesThatRequireRepositoryChange(dataClassInfo))
            {
                var typeInfoClosure = typeInfo;
                objectEventArgs.CallWhenFinished(() =>
                {
                    // All objects are stored, empty condition is sufficient. No need to delete objects. Dependencies are static, no need to store them.
                    RepositoryBulkOperations.StoreObjects(typeInfoClosure, new WhereCondition(), false);
                });
            }
        }


        private static IEnumerable<ObjectTypeInfo> GetObjectTypesThatRequireRepositoryChange(DataClassInfo dataClassInfo)
        {
            if ((dataClassInfo == null) || !ContinuousIntegrationEventHandling.RequiresFormDefinitionChangeRepositoryRefresh(dataClassInfo))
            {
                return Enumerable.Empty<ObjectTypeInfo>();
            }

            // TypeInfo for custom modules is available once the code is generated and compiled.
            return DataClassInfoProvider.GetClassObjectTypes(dataClassInfo.ClassName)
                .Where(typeInfo => typeInfo.SupportsContinuousIntegration());
        }
    }
}
