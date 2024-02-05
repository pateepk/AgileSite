using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Parameters wrapper for methods of <see cref="TranslationHelper"/>.
    /// </summary>
    public sealed class TranslationParameters : ICloneable
    {
        private readonly string mObjectType;

        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType
        {
            get
            {
                return (TypeInfo != null) ? TypeInfo.ObjectType : mObjectType;
            }
        }


        /// <summary>
        /// Type info object.
        /// </summary>
        /// <remarks>
        /// May be null when <see cref="ObjectType"/> is not registered.
        /// </remarks>
        internal ObjectTypeInfo TypeInfo
        {
            get;
            private set;
        }


        /// <summary>
        /// Code name
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Guid
        /// </summary>
        public Guid Guid
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Parent object ID
        /// </summary>
        public int ParentId
        {
            get;
            set;
        }


        /// <summary>
        /// Group info ID
        /// </summary>
        public int GroupId
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public TranslationParameters(string objectType)
        {
            mObjectType = objectType;
            TypeInfo = ObjectTypeManager.GetTypeInfo(ObjectType);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeInfo">Object type</param>
        public TranslationParameters(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            TypeInfo = typeInfo;
        }


        /// <summary>
        /// Creates a new object that is a copy of the given instance.
        /// </summary>
        public TranslationParameters(TranslationParameters original)
        {
            mObjectType = original.mObjectType;
            TypeInfo = original.TypeInfo;
            CodeName = original.CodeName;
            Guid = original.Guid;
            SiteName = original.SiteName;
            ParentId = original.ParentId;
            GroupId = original.GroupId;
        }


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            return new TranslationParameters(this);
        }


        /// <summary>
        /// Returns text representation
        /// </summary>
        public override string ToString()
        {
            return String.Join(" ", ObjectType, CodeName, Guid, SiteName, ParentId, GroupId);
        }
    }
}
