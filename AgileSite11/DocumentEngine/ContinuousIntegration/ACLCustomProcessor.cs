using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.Helpers;
using CMS.ContinuousIntegration.Internal;

namespace CMS.DocumentEngine
{
    internal class ACLCustomProcessor : CustomProcessorBase
    {
        #region "Constants"

        internal const string INHERITED_ACLS_COLUMN = "ACLInheritedACLs";
        internal const string SINGLE_REFERENCE_ELEMENT_NAME = "ACLID";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Should be only used for registration in <see cref="CustomProcessorFactory"/>.
        /// </summary>
        /// <param name="job">Instance of a job that requested this processor.</param>
        public ACLCustomProcessor(AbstractFileSystemJob job)
            : base(job)
        {
        }


        /// <summary>
        /// Loads ACL references stored within <paramref name="document"/>'s <see cref="INHERITED_ACLS_COLUMN"/> element
        /// and tries to translate them into IDs. Successfully translated IDs are joined into comma-separated string that
        /// is placed into the element. References which translation failed are stored in <see cref="DeserializationResultBase.FailedMappings"/>.
        /// </summary>
        /// <param name="processorResult">Result of operation.</param>
        /// <param name="document">XML file with serialized object.</param>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        public override void PreprocessDeserializedDocument(CustomProcessorResult processorResult, XmlDocument document, StructuredLocation structuredLocation)
        {
            var rootElement = document.DocumentElement;
            if (rootElement == null)
            {
                // No document, no processing
                return;
            }

            var inheritedElement = rootElement[INHERITED_ACLS_COLUMN];
            if (inheritedElement == null)
            {
                // ALC list element was not found, but field has not-null constraint, so it has to be present
                processorResult.FailedFields.Add(INHERITED_ACLS_COLUMN, null);
                return;
            }

            IEnumerable<XmlElement> innerElements;
            if (!GetValidElements(inheritedElement, out innerElements))
            {
                // ACL list element is leaf or contains invalid nodes, references (if present) cannot be verified
                processorResult.FailedFields.Add(INHERITED_ACLS_COLUMN, inheritedElement.InnerText);
                return;
            }

            var ids = DeserializeReferences(innerElements, processorResult).ToArray();
            RestoreIdsToXml(inheritedElement, ids);
        }


        /// <summary>
        /// Translates ACL stored within <paramref name="serializedObject"/>'s <see cref="INHERITED_ACLS_COLUMN"/> element
        /// into references and replaces content of the element with these references (in XML format).
        /// </summary>
        /// <param name="infoObject">Object that was serialized into <paramref name="serializedObject"/>.</param>
        /// <param name="infoRelativePath">(Main) Path the <paramref name="serializedObject"/> will be stored in.</param>
        /// <param name="serializedObject">Serialization form of the <paramref name="infoObject"/>.</param>
        public override void PostprocessSerializedObject(BaseInfo infoObject, string infoRelativePath, XmlElement serializedObject)
        {
            var inheritedElement = serializedObject[INHERITED_ACLS_COLUMN];
            if (inheritedElement == null)
            {
                // ALC list element was not found, cannot be further processed
                return;
            }

            if (inheritedElement.IsEmpty)
            {
                // ALC list element was empty (empty string stored in DB), cannot be further processed
                return;
            }

            var references = SerializeIds(inheritedElement);
            StoreReferencesToXml(inheritedElement, references);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns <see langword="true"/> if and only if the <paramref name="inheritedElement"/> contains such <see cref="XmlNode"/>s
        /// that are of type <see cref="XmlElement"/> and have the name of <see cref="SINGLE_REFERENCE_ELEMENT_NAME"/>.
        /// Elements fulfilling the condition above are returned in <paramref name="referenceElements"/>.
        /// </summary>
        private bool GetValidElements(XmlElement inheritedElement, out IEnumerable<XmlElement> referenceElements)
        {
            // All inner documents in the element
            var innerElements = inheritedElement
                .ChildNodes
                .Cast<XmlNode>()
                .ToArray();

            // Only ACL reference elements
            var validInnerElements = innerElements
                .OfType<XmlElement>()
                .Where(element => element.Name == SINGLE_REFERENCE_ELEMENT_NAME)
                .ToArray();

            referenceElements = validInnerElements;
            return validInnerElements.Length == innerElements.Length;
        }


        /// <summary>
        /// Returns collection of successfully translated IDs belonging to objects which references are stored in <paramref name="elements"/>.
        /// <para>References that failed to translate (ACL were not processed and stored yet) are added into <paramref name="processorResult"/>
        /// (under <see cref="DeserializationResultBase.FailedMappings"/>).
        /// </para>
        /// </summary>
        private IEnumerable<int> DeserializeReferences(IEnumerable<XmlElement> elements, CustomProcessorResult processorResult)
        {
            // Load individual references
            var aclReferences = elements
                .Select(TranslationReferenceLoader.LoadFromElement)
                .Where(aclReference => aclReference != null);

            // Try translate references
            foreach (var aclReference in aclReferences)
            {
                var id = TranslationHelper.GetForeignID(aclReference);
                if (id > 0)
                {
                    // Valid ID of existing object obtained
                    yield return id;
                }
                else
                {
                    // Invalid ID obtained, store it for later processing
                    processorResult.FailedMappings.Add(new FailedMapping(INHERITED_ACLS_COLUMN, aclReference));
                }
            }
        }


        /// <summary>
        /// Returns collection of <see cref="TranslationReference"/>s extracted from comma-separated
        /// list of IDs stored in <paramref name="element"/>'s inner text (non-existing IDs are skipped.).
        /// </summary>
        private IEnumerable<TranslationReference> SerializeIds(XmlElement element)
        {
            const int ID_NOT_FOUND = -1;
            var aclReferences = AclInfoProvider
                .SplitAclIdsList(element.InnerText)
                .Select(textId => ValidationHelper.GetInteger(textId, ID_NOT_FOUND))
                .Where(id => id != ID_NOT_FOUND)
                .Select(aclId => TranslationReferenceLoader.LoadFromDatabase(AclInfo.OBJECT_TYPE, aclId))
                .Where(aclReference => aclReference != null);

            return aclReferences;
        }


        /// <summary>
        /// Writes given <paramref name="ids"/> into provided <paramref name="element"/>
        /// (replacing all its existing content) in form of string list of comma-separated values.
        /// </summary>
        /// <remarks>
        /// If there are no <paramref name="ids"/> provided, <see cref="String.Empty"/>
        /// will be set into the <paramref name="element"/>.
        /// </remarks>
        private void RestoreIdsToXml(XmlElement element, IEnumerable<int> ids)
        {
            element.InnerText = AclInfoProvider.JoinAclIdsToList(ids);
        }


        /// <summary>
        /// Writes given <paramref name="references"/> into provided <paramref name="element"/> (replacing all its existing content).
        /// </summary>
        private void StoreReferencesToXml(XmlElement element, IEnumerable<TranslationReference> references)
        {
            // Clear original element's content
            element.IsEmpty = true;

            // Store references in original element
            var document = element.OwnerDocument;
            foreach (var aclReference in references)
            {
                var referenceElement = document.CreateElement(SINGLE_REFERENCE_ELEMENT_NAME);
                element.AppendChild(referenceElement);
                aclReference.WriteToXmlElement(referenceElement);
            }
        }

        #endregion
    }
}
