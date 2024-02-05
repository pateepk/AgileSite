using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Localization
{
    /// <summary>
    /// ResourceStringInfo data container class.
    /// </summary>
    public class ResourceStringInfoBase<TInfo> : AbstractInfo<TInfo>
        where TInfo : ResourceStringInfoBase<TInfo>, new()
    {
        #region "Properties"

        /// <summary>
        /// The GUID of the resource string.
        /// </summary>
        [DatabaseField]
        public Guid StringGUID
        {
            get
            {
                return GetValue("StringGUID", Guid.Empty);
            }
            set
            {
                SetValue("StringGUID", value);
            }
        }


        /// <summary>
        /// String ID
        /// </summary>
        [DatabaseField]
        public virtual int StringID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("StringID"), 0);
            }
            set
            {
                SetValue("StringID", value);
            }
        }


        /// <summary>
        /// String key
        /// </summary>
        [DatabaseField]
        public virtual string StringKey
        {
            get
            {
                return ValidationHelper.GetString(GetValue("StringKey"), "");
            }
            set
            {
                SetValue("StringKey", value);
            }
        }


        /// <summary>
        /// String is custom
        /// </summary>
        [DatabaseField]
        public virtual bool StringIsCustom
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("StringIsCustom"), false);
            }
            set
            {
                SetValue("StringIsCustom", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ResourceStringInfo object.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        public ResourceStringInfoBase(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ResourceStringInfo object from the given DataRow.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dr">DataRow with the object data</param>
        public ResourceStringInfoBase(ObjectTypeInfo typeInfo, DataRow dr)
            : base(typeInfo, dr)
        {
        }

        #endregion
    }
}