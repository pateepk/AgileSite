using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.Base;
using CMS.DataEngine;

namespace CMS
{
    /// <summary>
    /// This attribute finds the static TypeInfo fields (all of them) and registers object types those TypeInfos define to the system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterObjectTypeAttribute : Attribute, IPreInitAttribute
    {
        #region "Variables"

        private ObjectTypeInfo mTypeInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Type marked with this attribute
        /// </summary>
        public Type MarkedType
        {
            get;
            set;
        }


        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Type info representing this type
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get
            {
                return mTypeInfo ?? (mTypeInfo = GetTypeInfo());
            }
            set
            {
                mTypeInfo = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoType">Info type</param>
        /// <param name="objectType">Object type</param>
        public RegisterObjectTypeAttribute(Type infoType, string objectType)
        {
            MarkedType = infoType;
            ObjectType = objectType;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void PreInit()
        {
            ObjectTypeManager.RegisterObjectType(ObjectType, TypeInfo, MarkedType);
        }


        /// <summary>
        /// Gets list of fields of type ObjectTypeInfo for this attribute.
        /// </summary>
        /// <param name="flags">Binding flags</param>
        private  List<System.Reflection.FieldInfo> GetTypeInfoFields(BindingFlags flags)
        {
            return MarkedType.GetFields(flags)
                             .Where(f => f.FieldType == typeof (ObjectTypeInfo))
                             .ToList();
        }


        /// <summary>
        /// Gets the type info for this attribute.
        /// </summary>
        private ObjectTypeInfo GetTypeInfo()
        {
            // Find the static TypeInfo fields and register appropriate object type
            var fields = GetTypeInfoFields(BindingFlags.Public | BindingFlags.Static);

            // TypeInfo property not found, try to find it among inherited fields
            if (fields.Count == 0)
            {
                fields = GetTypeInfoFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            }

            foreach (var fieldInfo in fields)
            {
                var typeInfo = fieldInfo.GetValue(null) as ObjectTypeInfo;
                if ((typeInfo != null) && typeInfo.ObjectType.EqualsCSafe(ObjectType, true))
                {
                    return typeInfo;
                }
            }

            return null;
        }

        #endregion
    }
}
