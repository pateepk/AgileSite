using System;
using System.Globalization;
using System.Xml;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute with date and time values.
    /// </summary>
    public class EntityDateTimeAttributeModel : EntityAttributeModel
    {

        #region "Constructors"

        internal EntityDateTimeAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) : base(entityModel, source, type)
        {

        }

        #endregion

        #region "Methods"

        internal override object ConvertValue(object value)
        {
            return Convert.ToDateTime(value).ToUniversalTime();
        }

        internal override string SerializeValue(object value)
        {
            return ((DateTime)value).ToString("s", CultureInfo.InvariantCulture);
        }

        internal override object DeserializeValue(string value)
        {
            return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
        }

        #endregion

    }

}