using System;
using System.Collections.Generic;
using System.Xml;

namespace CMS.SalesForce
{

    internal sealed class EntitySerializer
    {

        #region "Private members"

        private XmlDocument mDocument;

        #endregion

        #region "Constructors"

        public EntitySerializer()
        {
            mDocument = new XmlDocument();
        }

        #endregion

        #region "Public methods"

        public WebServiceClient.sObject Serialize(Entity entity)
        {
            WebServiceClient.sObject result = new WebServiceClient.sObject();
            result.type = entity.Model.Name;
            List<XmlElement> elements = new List<XmlElement>();
            List<string> emptyAttributeNames = new List<string>();
            foreach (EntityAttribute attribute in entity.Attributes)
            {
                if (attribute.Model.Name == "Id")
                {
                    if (attribute.Value != null)
                    {
                        result.Id = (string)attribute.Value;
                    }
                }
                else
                {
                    if ((attribute.Value != null) && (!attribute.Value.Equals(string.Empty)))
                    {
                        XmlElement element = mDocument.CreateElement(attribute.Model.Name);
                        element.InnerText = attribute.Model.SerializeValue(attribute.Value);
                        elements.Add(element);
                    }
                    else
                    {
                        emptyAttributeNames.Add(attribute.Model.Name);
                    }
                }
            }
            result.fieldsToNull = emptyAttributeNames.ToArray();
            result.Any = elements.ToArray();

            return result;
        }

        public Entity Deserialize(WebServiceClient.sObject entitySource, EntityModel entityModel)
        {
            Entity result = entityModel.CreateEntity();
            if (entitySource.Any != null)
            {
                foreach (XmlElement element in entitySource.Any)
                {
                    EntityAttributeModel entityAttributeModel = entityModel.GetAttributeModel(element.LocalName);
                    if (entityAttributeModel == null)
                    {
                        throw new Exception("[EntitySerializer.Deserialize]: Invalid attribute name.");
                    }
                    result[entityAttributeModel.Name] = entityAttributeModel.DeserializeValue(element.InnerText);
                }
            }
            if (entitySource.fieldsToNull != null)
            {
                foreach (string attributeName in entitySource.fieldsToNull)
                {
                    EntityAttributeModel entityAttributeModel = entityModel.GetAttributeModel(attributeName);
                    if (entityAttributeModel == null)
                    {
                        throw new Exception("[EntitySerializer.Deserialize]: Invalid attribute name.");
                    }
                    result[entityAttributeModel.Name] = null;
                }
            }
            if (!String.IsNullOrEmpty(entitySource.Id))
            {
                result["Id"] = entitySource.Id;
            }

            return result;
        }

        #endregion

    }

}