using System;

using Newtonsoft.Json.Serialization;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Serialization binder for <see cref="ABTestConfigurationSerializer"/>.
    /// </summary>
    internal class ABTestConfigurationTypesBinder : DefaultSerializationBinder
    {
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType == null)
            {
                throw new ArgumentNullException(nameof(serializedType));
            }

            base.BindToName(serializedType, out assemblyName, out typeName);
        }


        /// <summary>
        /// Binds given <paramref name="typeName"/> in <paramref name="assemblyName"/> only if this type is valid for <see cref="ABTestConfiguration"/>.
        /// </summary>
        public override Type BindToType(string assemblyName, string typeName)
        {
            var deserializedType = base.BindToType(assemblyName, typeName);

            if (!IsValidFormBuilderSerializedType(deserializedType))
            {
                return null;
            }

            return deserializedType;
        }


        private static bool IsValidFormBuilderSerializedType(Type serializedType)
        {
            return (serializedType == typeof(ABTestConfiguration)) || (serializedType == typeof(ABTestVariant));
        }
    }
}
