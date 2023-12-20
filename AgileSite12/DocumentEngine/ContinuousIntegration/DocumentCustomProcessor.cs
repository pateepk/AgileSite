using System;
using System.Xml;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    internal class DocumentCustomProcessor : ICustomProcessor
    {
        /// <summary>
        /// Amends <paramref name="document"/> loaded from file system before it is processed by <see cref="InfoDeserializer"/>.
        /// </summary>
        /// <param name="processorResult">Object carrying information of ongoing deserialization.</param>
        /// <param name="document">XML file loaded from the file system.</param>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        public void PreprocessDeserializedDocument(CustomProcessorResult processorResult, XmlDocument document, StructuredLocation structuredLocation)
        {
            // There is no customization needed.
        }


        /// <summary>
        /// Amends <paramref name="serializedObject"/> after it was processed by <see cref="InfoSerializer"/>.
        /// </summary>
        /// <param name="infoObject">Object that was serialized into <paramref name="serializedObject"/>.</param>
        /// <param name="infoRelativePath">(Main) Path the <paramref name="serializedObject"/> will be stored in.</param>
        /// <param name="serializedObject">Serialization form of the <paramref name="infoObject"/>.</param>
        public void PostprocessSerializedObject(BaseInfo infoObject, string infoRelativePath, XmlElement serializedObject)
        {
            if (serializedObject.OwnerDocument == null)
            {
                return;
            }

            string fileIdentification = null;
            switch (infoObject.TypeInfo.ObjectType)
            {
                case DocumentNodeDataInfo.OBJECT_TYPE:
                    fileIdentification = ((DocumentNodeDataInfo)infoObject).NodeAliasPath;
                    break;

                case DocumentCultureDataInfo.OBJECT_TYPE:
                    fileIdentification = ((DocumentCultureDataInfo)infoObject).DocumentNamePath;       
                    break;
            }

            if (!String.IsNullOrWhiteSpace(fileIdentification))
            {
                var comment = serializedObject.OwnerDocument.CreateComment(String.Format(" This is a CI serialization file for {0} ", fileIdentification));
                serializedObject.OwnerDocument.PrependChild(comment);
            }
        }
    }
}
