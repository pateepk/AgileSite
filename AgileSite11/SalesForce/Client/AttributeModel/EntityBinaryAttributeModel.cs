using System;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute with binary values.
    /// </summary>
    public class EntityBinaryAttributeModel : EntityAttributeModel
    {

        #region "Constructors"

        internal EntityBinaryAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) : base(entityModel, source, type)
        {

        }

        #endregion

        #region "Methods"

        internal override object ConvertValue(object value)
        {
            return (byte[])value;
        }

        internal override string SerializeValue(object value)
        {
            return Convert.ToBase64String((byte[])value);
        }

        internal override object DeserializeValue(string value)
        {
            return Convert.FromBase64String(value);
        }

        #endregion

    }

}