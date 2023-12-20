using System;
using System.Runtime.Serialization;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Provides a type to which deserialization is to be bound.
    /// </summary>
    internal class StagingTaskDataBinder : SerializationBinder
    {
        private readonly Type stagingTaskDataType = typeof(StagingTaskData);
        private readonly Type taskGroupInfoType = typeof(TaskGroupInfo);
        private readonly Type synchronizationTypeEnumType = typeof(SynchronizationTypeEnum);
        private readonly Type iDataClassType = typeof(IDataClass);
        private readonly Type objectType = typeof(object);
        private readonly Type guidType = typeof(Guid);

        private readonly string synchronizationAssemblyName;
        private readonly string dataEngineAssemblyName;
        private readonly string systemAssemblyName;

        private static readonly Lazy<StagingTaskDataBinder> mInstance = new Lazy<StagingTaskDataBinder>(() => new StagingTaskDataBinder());


        /// <summary>
        /// Gets the current instance of the <see cref="StagingTaskDataBinder"/> class.
        /// </summary>
        public static StagingTaskDataBinder Instance => mInstance.Value;


        /// <summary>
        /// Initializes a new instance of <see cref="StagingTaskDataBinder"/>.
        /// </summary>
        private StagingTaskDataBinder()
        {
            synchronizationAssemblyName = stagingTaskDataType.Assembly.FullName;
            dataEngineAssemblyName = synchronizationTypeEnumType.Assembly.FullName;
            systemAssemblyName = objectType.Assembly.FullName;
        }


        /// <summary>
        /// Returns a type to be bound to for deserialization of staging data.
        /// </summary>
		/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object. </param>
		/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object. </param>
        /// <exception cref="InvalidOperationException">Thrown when, <paramref name="assemblyName"/> and <paramref name="typeName"/> are not supported by the staging.</exception>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (systemAssemblyName.Equals(assemblyName, StringComparison.Ordinal))
            {
                if (typeName.Equals(objectType.FullName, StringComparison.Ordinal))
                {
                    return objectType;
                }

                if (typeName.Equals(guidType.FullName, StringComparison.Ordinal))
                {
                    return guidType;
                }
            }

            if (dataEngineAssemblyName.Equals(assemblyName, StringComparison.Ordinal))
            {
                if (synchronizationTypeEnumType.FullName.Equals(typeName, StringComparison.Ordinal))
                {
                    return synchronizationTypeEnumType;
                }
                
                if (iDataClassType.IsAssignableFrom(synchronizationTypeEnumType.Assembly.GetType(typeName)))
                {
                    // When object is assignable from IDataClass (SimpleDataClass) then return null thus deserializer will use the type from the input params
                    return null;
                }
            }

            if (synchronizationAssemblyName.Equals(assemblyName, StringComparison.Ordinal))
            {
                if (stagingTaskDataType.FullName.Equals(typeName, StringComparison.Ordinal))
                {
                    return stagingTaskDataType;
                }

                if (taskGroupInfoType.FullName.Equals(typeName, StringComparison.Ordinal))
                {
                    return taskGroupInfoType;
                }
            }

            throw new InvalidOperationException($"Type '{typeName}' from assembly '{assemblyName}' cannot be bound to.");
        }
    }
}
