using System;
using System.Globalization;
using System.Xml;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute with double values.
    /// </summary>
    public class EntityDoubleAttributeModel : EntityAttributeModel
    {

        #region "Constructors"

        internal EntityDoubleAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) : base(entityModel, source, type)
        {

        }

        #endregion

        #region "Methods"

        internal override object ConvertValue(object value)
        {
            return Convert.ToDouble(value);
        }

        internal override string SerializeValue(object value)
        {
            return ((double)value).ToString("G", CultureInfo.InvariantCulture);
        }

        internal override object DeserializeValue(string value)
        {
            return XmlConvert.ToDouble(value);
        }

        #endregion

    }

}