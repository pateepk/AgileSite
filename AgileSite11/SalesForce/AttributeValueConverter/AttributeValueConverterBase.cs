using System;
using System.Collections.Generic;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides a base implementation for the extensible SalesForce entity attribute value converter model.
    /// </summary>
    public abstract class AttributeValueConverterBase
    {

        #region "Protected members"

        /// <summary>
        /// The entity attribute model to convert value to.
        /// </summary>
        protected EntityAttributeModel mAttributeModel;

        /// <summary>
        /// The form field info to convert value from.
        /// </summary>
        protected FormFieldInfo mFieldInfo;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the entity attribute value converter using the specified attribute model and field info.
        /// </summary>
        /// <param name="attributeModel">The entity attribute model to convert value to.</param>
        /// <param name="fieldInfo">The form field info to convert value from.</param>
        public AttributeValueConverterBase(EntityAttributeModel attributeModel, FormFieldInfo fieldInfo)
        {
            mAttributeModel = attributeModel;
            mFieldInfo = fieldInfo;
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Creates a list of compatibility problems that might occur during the value conversion, and returns it.
        /// </summary>
        /// <returns>A list of compatibility problems that might occur during the value conversion.</returns>
        public virtual List<string> GetCompatibilityWarnings()
        {
            List<string> warnings = new List<string>();
            if (!mAttributeModel.IsCreatable)
            {
                warnings.Add(ResHelper.GetString("sf.attributecompatibility.notcreatable"));
            }
            if (!mAttributeModel.IsUpdateable)
            {
                warnings.Add(ResHelper.GetString("sf.attributecompatibility.notupdateable"));
            }
            if (mFieldInfo.AllowEmpty)
            {
                if (!mAttributeModel.IsNullable && !mAttributeModel.HasDefaultValue && !mAttributeModel.IsCalculated)
                {
                    warnings.Add(ResHelper.GetString("sf.attributecompatibility.required"));
                }
            }

            return warnings;
        }

        /// <summary>
        /// Assigns the value from the specified CMS object field to the specified SalesForce entity attribute.
        /// </summary>
        /// <param name="entity">A SalesForce entity.</param>
        /// <param name="baseInfo">A CMS object.</param>
        public abstract void Convert(Entity entity, BaseInfo baseInfo);

        #endregion

        #region "Protected methods"

        /// <summary>
        /// Gets the custom exception message when the form field type is not copmatible with this converter.
        /// </summary>
        /// <param name="fieldType">The form field type.</param>
        /// <returns>The exception message.</returns>
        protected string GetUnsupportedFieldTypeMessage(string fieldType)
        {
            return String.Format("[{0}.Convert]: Unsupported form field type ({1}).", GetType().Name, fieldType);
        }

        #endregion

    }

}