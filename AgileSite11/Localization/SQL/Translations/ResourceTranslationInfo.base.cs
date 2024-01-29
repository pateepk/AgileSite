using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Localization
{
    /// <summary>
    /// ResourceTranslationInfo data container class.
    /// </summary>
    public class ResourceTranslationInfoBase<TInfo> : AbstractInfo<TInfo>
        where TInfo : ResourceTranslationInfoBase<TInfo>, new()
    {
        #region "Properties"

        /// <summary>
        /// Translation ID
        /// </summary>
        [DatabaseField]
        public virtual int TranslationID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TranslationID"), 0);
            }
            set
            {
                SetValue("TranslationID", value);
            }
        }


        /// <summary>
        /// Translation string ID
        /// </summary>
        [DatabaseField]
        public virtual int TranslationStringID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TranslationStringID"), 0);
            }
            set
            {
                SetValue("TranslationStringID", value);
            }
        }


        /// <summary>
        /// Translation text
        /// </summary>
        [DatabaseField]
        public virtual string TranslationText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TranslationText"), "");
            }
            set
            {
                SetValue("TranslationText", value);
            }
        }


        /// <summary>
        /// Translation culture ID
        /// </summary>
        [DatabaseField]
        public virtual int TranslationCultureID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TranslationCultureID"), 0);
            }
            set
            {
                SetValue("TranslationCultureID", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ResourceTranslationInfo object.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        public ResourceTranslationInfoBase(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ResourceTranslationInfo object from the given DataRow.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dr">DataRow with the object data</param>
        public ResourceTranslationInfoBase(ObjectTypeInfo typeInfo, DataRow dr)
            : base(typeInfo, dr)
        {
        }

        #endregion
    }
}