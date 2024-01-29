using System;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute with picklist values.
    /// </summary>
    public class EntityMultiPicklistAttributeModel : EntityAttributeModel
    {

        #region "Constants"

        private const char TOKEN_SEPARATOR = ';';

        #endregion

        #region "Constructors"

        internal EntityMultiPicklistAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) : base(entityModel, source, type)
        {

        }

        #endregion

        #region "Methods"

        internal override object ConvertValue(object value)
        {
            string text = value as string;
            if (text != null)
            {
                return text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return (string[])value;
        }

        internal override string SerializeValue(object value)
        {
            return String.Join(TOKEN_SEPARATOR.ToString(), (string[])value);
        }

        internal override object DeserializeValue(string value)
        {
            return value.Split(TOKEN_SEPARATOR);
        }

        #endregion

    }

}