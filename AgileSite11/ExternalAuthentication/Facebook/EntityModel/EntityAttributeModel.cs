using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Represents a model of a Facebook API entity attribute.
    /// </summary>
    public sealed class EntityAttributeModel
    {
        #region "Variables"

        /// <summary>
        /// The corresponding property info.
        /// </summary>
        private PropertyInfo mPropertyInfo;


        /// <summary>
        /// The display name.
        /// </summary>
        private string mDisplayName = String.Empty;


        /// <summary>
        /// The description.
        /// </summary>
        private string mDescription = String.Empty;


        /// <summary>
        /// The name of a permission scope required by external application to obtain a value of this attribute.
        /// </summary>
        private string mFacebookPermissionScopeName;


        /// <summary>
        /// The type of the required entity attribute value converter.
        /// </summary>
        private Type mAttributeValueConverterType;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the attribute name.
        /// </summary>
        public string Name
        {
            get
            {
                return mPropertyInfo.Name;
            }
        }


        /// <summary>
        /// Gets the attribute display name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
        }


        /// <summary>
        /// Gets the attribute description.
        /// </summary>
        public string Description
        {
            get
            {
                return mDescription;
            }
        }


        /// <summary>
        /// Gets the name of a permission scope required by external application to obtain a value of this attribute.
        /// </summary>
        public string FacebookPermissionScopeName
        {
            get
            {
                return mFacebookPermissionScopeName;
            }
        }


        /// <summary>
        /// Gets the type of the entity attribute value converter.
        /// </summary>
        public Type AttributeValueConverterType
        {
            get
            {
                return mAttributeValueConverterType;
            }
        }


        /// <summary>
        /// Gets the corresponding property info.
        /// </summary>
        internal PropertyInfo PropertyInfo
        {
            get
            {
                return mPropertyInfo;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityAttributeInfo class.
        /// </summary>
        /// <param name="propertyInfo">The corresponding entity property info.</param>
        internal EntityAttributeModel(PropertyInfo propertyInfo)
        {
            mPropertyInfo = propertyInfo;
            object[] attributes = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true);
            if (attributes.Length > 0)
            {
                DisplayAttribute attribute = attributes[0] as DisplayAttribute;
                if (!String.IsNullOrEmpty(attribute.Name))
                {
                    mDisplayName = attribute.Name;
                }
                if (!String.IsNullOrEmpty(attribute.Description))
                {
                    mDescription = attribute.Description;
                }
            }
            object[] facebookAttributes = propertyInfo.GetCustomAttributes(typeof(FacebookAttribute), true);
            if (facebookAttributes.Length > 0)
            {
                FacebookAttribute facebookAttribute = facebookAttributes[0] as FacebookAttribute;
                if (!String.IsNullOrEmpty(facebookAttribute.PermissionScopeName))
                {
                    mFacebookPermissionScopeName = facebookAttribute.PermissionScopeName;
                }
                mAttributeValueConverterType = facebookAttribute.ValueConverterType;
            }
        }

        #endregion
    }

}