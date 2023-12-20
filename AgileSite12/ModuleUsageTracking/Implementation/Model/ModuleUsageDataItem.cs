using System;
using System.Runtime.Serialization;

using CMS.Base;
using CMS.Helpers;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Represents one fragment of module usage data
    /// </summary>
    [Serializable]
    internal class ModuleUsageDataItem : IModuleUsageDataItem, ISerializable
    {
        #region "Properties"

        /// <summary>
        /// Key representing the item. Must be unique within one data source.
        /// </summary>
        public string Key
        {
            get;
            set;
        }


        /// <summary>
        /// Item value
        /// </summary>
        public object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Data type of value
        /// </summary>
        public ModuleUsageDataTypeEnum Type
        {
            get
            {
                return Value == null ? EnumHelper.GetDefaultValue<ModuleUsageDataTypeEnum>() : Value.GetType().ToModuleUsageDataTypeEnum();
            }
        }

        #endregion


        #region "Methods"

        public ModuleUsageDataItem(string key = null, object value = null)
        {
            Key = key;
            Value = value;
        }

        #endregion


        #region "Equalization methods"

        /// <summary>
        /// Indicates whether the current object is equal to another object.
        /// </summary>
        protected bool Equals(ModuleUsageDataItem other)
        {
            return string.Equals(Key, other.Key, StringComparison.Ordinal);
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as ModuleUsageDataItem;
            return other != null && Equals(other);
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #endregion


        #region "Serialization methods"


        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key);
            info.AddValue("Value", Value);
            info.AddValue("Type", Type.ToString());
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        public ModuleUsageDataItem(SerializationInfo info, StreamingContext context)
        {
            Key = info.GetString("Key");

            var type = (ModuleUsageDataTypeEnum)Enum.Parse(typeof(ModuleUsageDataTypeEnum), info.GetString("Type"));
            Value = info.GetValue("Value", type.ToTypeRepresentation());
        }

        #endregion
    }
}
