using System;
using System.Globalization;
using System.Xml;

namespace CMS.SalesForce
{
    /// <summary>
    /// Represents a model of the Sales force entity attribute with integer values.
    /// </summary>
    public class EntityIntegerAttributeModel : EntityAttributeModel
    {
        internal EntityIntegerAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) 
            : base(entityModel, source, type)
        {
        }


        internal override object ConvertValue(object value)
        {
            return Convert.ToInt32(value);
        }


        internal override string SerializeValue(object value)
        {
            return ((int)value).ToString("D", CultureInfo.InvariantCulture);
        }


        internal override object DeserializeValue(string value)
        {
            if (String.IsNullOrEmpty(value) && IsNullable)
            {
                return null;
            }

            return XmlConvert.ToInt32(value);
        }
    }
}