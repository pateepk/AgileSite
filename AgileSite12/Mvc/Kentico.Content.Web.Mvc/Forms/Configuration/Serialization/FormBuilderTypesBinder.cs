using System;
using Newtonsoft.Json.Serialization;

namespace Kentico.Forms.Web.Mvc
{
    internal class FormBuilderTypesBinder : DefaultSerializationBinder
    {
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType == null)
            {
                throw new ArgumentNullException(nameof(serializedType));
            }

            base.BindToName(serializedType, out assemblyName, out typeName);
        }


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
            return serializedType.IsSubclassOf(typeof(FormComponentProperties))
                || serializedType.IsSubclassOf(typeof(ValidationRule))
                || serializedType.IsSubclassOf(typeof(VisibilityCondition));
        }
    }
}
