using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents the general info object identifier that uses object type to translate info object ID to code name and vice versa.
    /// </summary>
    public class InfoIdentifier
    {
        #region "Variables"

        private int? mObjectID;
        private string mObjectCodeName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the object type.
        /// </summary>
        public string ObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the object ID. If this <see cref="InfoIdentifier"/> instance was initialized from <see cref="ObjectCodeName"/> which does not represent an existing object, returns 0.
        /// </summary>
        public int ObjectID
        {
            get
            {
                if (!mObjectID.HasValue)
                {
                    mObjectID = ProviderHelper.GetId(ObjectType, ObjectCodeName);
                }

                return mObjectID.Value;
            }
            private set
            {
                mObjectID = value;
            }
        }


        /// <summary>
        /// Gets the object code name. If this <see cref="InfoIdentifier"/> was initialized from <see cref="ObjectID"/> which does not represent an existing object, returns null.
        /// </summary>
        public string ObjectCodeName
        {
            get
            {
                if (string.IsNullOrEmpty(mObjectCodeName))
                {
                    mObjectCodeName = ProviderHelper.GetCodeName(ObjectType, ObjectID);
                }

                return mObjectCodeName;
            }
            private set
            {
                mObjectCodeName = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a new info object identifier using the object type and ID.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        public InfoIdentifier(string objectType, int objectId)
        {
            if (string.IsNullOrEmpty(objectType))
            {
                throw new ArgumentNullException("objectType");
            }

            if (objectId == 0)
            {
                throw new ArgumentOutOfRangeException("objectId", "Object ID needs to be greater than 0.");
            }

            ObjectType = objectType;
            ObjectID = objectId;
        }


        /// <summary>
        /// Creates a new info object identifier using the object type and code name.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="codeName">Code name</param>
        public InfoIdentifier(string objectType, string codeName)
        {
            if (string.IsNullOrEmpty(objectType))
            {
                throw new ArgumentNullException("objectType");
            }

            if (string.IsNullOrEmpty(codeName))
            {
                throw new ArgumentNullException("codeName");
            }

            ObjectType = objectType;
            ObjectCodeName = codeName;
        }


        /// <summary>
        /// Creates a new info object identifier for the specified info object.
        /// </summary>
        /// <param name="info">Info</param>
        public InfoIdentifier(BaseInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            ObjectType = info.TypeInfo.ObjectType;
            ObjectID = info.Generalized.ObjectID;
            ObjectCodeName = info.Generalized.ObjectCodeName;
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicitly converts BaseInfo to InfoIdentifier.
        /// </summary>
        /// <param name="info">Info</param>
        public static implicit operator InfoIdentifier(BaseInfo info)
        {
            return new InfoIdentifier(info);
        }


        /// <summary>
        /// Implicitly converts InfoIdentifier to integer.
        /// </summary>
        /// <param name="identifier">Info identifier</param>
        public static implicit operator int(InfoIdentifier identifier)
        {
            return (identifier == null) ? 0 : identifier.ObjectID;
        }


        /// <summary>
        /// Implicitly converts InfoIdentifier to string.
        /// </summary>
        /// <param name="identifier">Info identifier</param>
        public static implicit operator string(InfoIdentifier identifier)
        {
            return (identifier == null) ? null : identifier.ObjectCodeName;
        }

        #endregion
    }
}