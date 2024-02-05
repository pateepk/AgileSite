using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.Helpers;
using CMS.IO;
using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Provides methods for working with bindings in the file system repository.
    /// </summary>
    internal class FileSystemBindingsProcessor
    {
        #region "Constants"

        private const string BINDING_ID_ATTRIBUTE = "id";
        private const string BINDING_IDENTIFIER_SEPARATOR = "-";
        private const string BINDING_ELEMENT = "Binding";
        private const string BINDING_PARENT_ELEMENT = "Parent";
        private const string BINDINGS_COLLECTION_ELEMENT = "Bindings";


        /// <summary>
        /// Constant indicating that no bindings are present in the returned document.
        /// </summary>
        public static readonly XmlDocument NO_BINDINGS = new XmlDocument();

        #endregion


        #region "Variables and Properties"

        private readonly TranslationHelper mTranslationHelper;
        private readonly FileSystemRepositoryConfiguration mConfiguration;
        private readonly IFileSystemWriter mWriter;


        /// <summary>
        /// If true (default), before a binding is added into existing file, the content is checked if a binding with same child reference is already present in the 
        /// </summary>
        public bool CheckBindingExistanceBeforeAddition
        {
            get;
            set;
        } = true;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor with required <paramref name="translationHelper"/>, <paramref name="configuration"/> and <paramref name="writer"/> parameter.
        /// </summary>
        /// <param name="translationHelper"><see cref="TranslationHelper"/>Object that is used to optimize database calls when translating IDs to <see cref="TranslationReference"/>.</param>
        /// <param name="configuration">Object containing repository configuration.</param>
        /// <param name="writer">Object for writing to file system and storing hash of written file in hash manager.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="translationHelper"/> or <paramref name="configuration"/> or <paramref name="writer"/> not provided.</exception>
        public FileSystemBindingsProcessor(TranslationHelper translationHelper, FileSystemRepositoryConfiguration configuration, IFileSystemWriter writer)
        {
            if (translationHelper == null)
            {
                throw new ArgumentNullException(nameof(translationHelper));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            mTranslationHelper = translationHelper;
            mConfiguration = configuration;
            mWriter = writer;
        }


        /// <summary>
        /// Appends binding to the document retrieved from provided <paramref name="relativePath"/>.
        /// If binding record already exists within the file, it is rewritten in the resulting document.
        /// If the file does not exist, completely new document is created with single <paramref name="binding"/> (the provided one) inside.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the file in the repository that holds all bindings for the <paramref name="binding"/>'s parent.</param>
        /// <returns>
        /// An <see cref="XmlDocument"/> that contains all already written bindings and the new/edited one
        /// or <see cref="NO_BINDINGS"/> when there was an error in the document loaded from <paramref name="relativePath"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When no <paramref name="binding"/> provided.</exception>
        public XmlDocument AppendBinding(BaseInfo binding, string relativePath)
        {
            return AppendBindingInternal(binding, relativePath);
        }


        /// <summary>
        /// Reads all bindings within the provided <paramref name="document"/>. Bindings are supposed to be of <paramref name="typeInfo"/> type.
        /// </summary>
        /// <remarks>
        /// If there is a problem with XML structure (e.g. bindings collection is missing or there is no root element), empty collection is returned,
        /// otherwise, a collection of <see cref="DeserializationResult"/> is returned where each binding needs to be examined for its potential deserialization problems.
        /// </remarks>
        /// <param name="document"><see cref="XmlDocument"/> read containing representation of all bindings of <paramref name="typeInfo"/> type with single parent.</param>
        /// <param name="typeInfo"><see cref="ObjectTypeInfo"/> of the bindings that are stored within the <paramref name="document"/>.</param>
        /// <returns>Collection of <see cref="DeserializationResult"/>s that were read from provided <paramref name="document"/>.</returns>
        /// <exception cref="ArgumentNullException">When either <paramref name="document"/> or <paramref name="typeInfo"/> is not provided.</exception>
        public IEnumerable<DeserializationResult> ReadBindings(XmlDocument document, ObjectTypeInfo typeInfo)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            // Lookup root element
            var root = document.DocumentElement;
            if (!IsRootElementValid(root))
            {
                // No root, no inner elements, no results
                yield break;
            }

            // Lookup bindings (children) collection
            var bindings = SelectBindingsCollection(root);
            if (bindings == null)
            {
                // No bindings collection, no complete binding present
                yield break;
            }

            // Read bindings' parent
            var parentIdColumn = document.CreateElement(typeInfo.ParentIDColumn);
            var parentElement = root.SelectSingleNode(BINDING_PARENT_ELEMENT);
            if (parentElement != null)
            {
                parentIdColumn.InnerXml = parentElement.InnerXml;
            }

            foreach (XmlNode bindingElement in bindings)
            {
                // Create complete binding element from (common) parent ID and (individual) child ID and (eventually) other fields
                var completeBindingElement = document.CreateElement(root.Name);
                completeBindingElement.InnerXml = bindingElement.InnerXml;
                completeBindingElement.AppendChild(parentIdColumn);

                yield return completeBindingElement.Deserialize(mTranslationHelper);
            }
        }


        /// <summary>
        /// Returns true if given binding is serialized in a file on given path.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the bindings serialized within the repository.</param>
        public bool ContainsBinding(BaseInfo binding, string relativePath)
        {
            var document = LoadDocumentFromFile(relativePath);

            var root = document?.DocumentElement;
            if (root == null)
            {
                return false;
            }

            var id = GetBindingIdentifier(binding);
            var xPath = $"{BINDINGS_COLLECTION_ELEMENT}/{GetBindingElementXPath(id)}";

            var element = root.SelectSingleNode(xPath);

            return element != null;
        }


        /// <summary>
        /// Removes the <paramref name="binding"/> from the document retrieved from provided <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the file in the repository that holds all bindings for the <paramref name="binding"/>'s parent.</param>
        /// <returns>
        /// Document loaded from <paramref name="relativePath"/> without provided <paramref name="binding"/>'s record.
        /// <para>If binding record does not exist within the file, unchanged document is returned.</para>
        /// <para>If no binding record exist within the file after the <paramref name="binding"/> removal, <see cref="NO_BINDINGS"/> is returned.</para>
        /// <para>If the file does not exist, <see cref="NO_BINDINGS"/> is returned.</para>
        /// </returns>
        public XmlDocument RemoveBinding(BaseInfo binding, string relativePath)
        {
            return RemoveBindingInternal(binding, relativePath);
        }


        /// <summary>
        /// Writes bindings contained in <paramref name="document" /> to a file on <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Target file path</param>
        /// <param name="document">XML document containing bindings.</param>
        public void WriteBindings(string relativePath, XmlDocument document)
        {
            WriteBindingsInternal(relativePath, document);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Appends binding to the document retrieved from provided <paramref name="relativePath"/>.
        /// If binding record already exists within the file, it is rewritten in the resulting document.
        /// If the file does not exist, completely new document is created with single <paramref name="binding"/> (the provided one) inside.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the file in the repository that holds all bindings for the <paramref name="binding"/>'s parent.</param>
        /// <returns>
        /// An <see cref="XmlDocument"/> that contains all already written bindings and the new/edited one
        /// or <see cref="NO_BINDINGS"/> when there was an error in the document loaded from <paramref name="relativePath"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When no <paramref name="binding"/> provided.</exception>
        protected virtual XmlDocument AppendBindingInternal(BaseInfo binding, string relativePath)
        {
            if (binding == null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            var settingsCopy = new SerializationSettings(binding.TypeInfo.SerializationSettings);

            // Parent ID is the same for the whole file, ignore it during individual binding object serialization
            settingsCopy.ExcludedFieldNames.Add(binding.TypeInfo.PossibleParentIDColumn);

            var bindingElement = binding.Serialize(mTranslationHelper, settingsCopy);

            // The rest of the serialized binding that is unique per binding
            string bindingXml = bindingElement.InnerXml;
            var bindingIdentifier = GetBindingIdentifier(binding);

            // Load existing file or create the XML structure in the new file that will be shared among multiple bindings
            var document = LoadDocumentFromFile(relativePath) ?? PrepareNewDocument(relativePath, binding.Parent, bindingElement.Name);

            // Create or update node for the binding.
            var node = FindOrCreateElement(document, bindingElement.Name, bindingIdentifier);

            if (node == null)
            {
                // Node could not be obtained, no appending can be performed
                return NO_BINDINGS;
            }

            // Update individual binding data
            node.InnerXml = bindingXml;
            return document;
        }


        /// <summary>
        /// Removes the <paramref name="binding"/> from the document retrieved from provided <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the file in the repository that holds all bindings for the <paramref name="binding"/>'s parent.</param>
        /// <returns>
        /// Document loaded from <paramref name="relativePath"/> without provided <paramref name="binding"/>'s record.
        /// <para>If binding record does not exist within the file, unchanged document is returned.</para>
        /// <para>If no binding record exist within the file after the <paramref name="binding"/> removal, <see cref="NO_BINDINGS"/> is returned.</para>
        /// <para>If the file does not exist, <see cref="NO_BINDINGS"/> is returned.</para>
        /// </returns>
        protected virtual XmlDocument RemoveBindingInternal(BaseInfo binding, string relativePath)
        {
            var document = LoadDocumentFromFile(relativePath);
            if (document == null)
            {
                // No file exists, there can be no valid bindings
                return NO_BINDINGS;
            }

            var root = document.DocumentElement;
            if (!IsRootElementValid(root, binding.TypeInfo.ObjectType))
            {
                // Document does not contains expected root element, there can be no valid bindings
                return NO_BINDINGS;
            }

            var collection = SelectBindingsCollection(root);
            if (collection == null)
            {
                // Document does not contain expected bindings collection, there can be no valid bindings
                return NO_BINDINGS;
            }

            var id = GetBindingIdentifier(binding);
            foreach (XmlNode bindingElement in SelectBindingElements(collection, id))
            {
                collection.RemoveChild(bindingElement);
            }

            // If there are some bindings left, return updated document; otherwise there are no bindings.
            return collection.ChildNodes.Count > 0 ? document : NO_BINDINGS;
        }


        /// <summary>
        /// Writes bindings contained in <paramref name="document" /> to a file on <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Target file path</param>
        /// <param name="document">XML document containing bindings.</param>
        protected virtual void WriteBindingsInternal(string relativePath, XmlDocument document)
        {
            SortBindings(document);

            mWriter.WriteToFile(relativePath, document);
        }


        private void SortBindings(XmlDocument document)
        {
            var collection = SelectBindingsCollection(document.DocumentElement);
            if (collection != null)
            {
                var orderedBindings = collection.ChildNodes
                    .OfType<XmlElement>()
                    .OrderBy(binding => binding.GetAttribute(BINDING_ID_ATTRIBUTE), StringComparer.Ordinal)
                    .ToList();

                collection.RemoveAll();

                orderedBindings.ForEach(binding => collection.AppendChild(binding));
            }
        }


        /// <summary>
        /// Creates a new document with binding elements structure and returns it.
        /// </summary>
        /// <param name="relativePath">Path to a file where the bindings should be stored.</param>
        /// <param name="parent">Parent common for all bindings stored within the document (including currently processed only right now).</param>
        /// <param name="rootElementName">Name of binding's object type (serving as root element of the document).</param>
        /// <returns>New element for currently processed binding.</returns>
        protected virtual XmlDocument PrepareNewDocument(string relativePath, BaseInfo parent, string rootElementName)
        {
            var document = new XmlDocument();

            var root = document.CreateElement(rootElementName);
            document.AppendChild(root);

            CreateParentElement(root, parent);

            CreateBindingsCollection(root);

            return document;
        }


        /// <summary>
        /// Finds existing element within the document or creates new element in the existing structure of the document.
        /// </summary>
        /// <remarks>
        /// Returns null when root element is wrong or there is no <see cref="BINDINGS_COLLECTION_ELEMENT"/> present under the root.
        /// </remarks>
        /// <param name="document">Document to work with.</param>
        /// <param name="rootElementName">Name of object type that is expected to occur as root element name.</param>
        /// <param name="bindingIdentifier">Unique binding identifier (see <see cref="GetBindingIdentifier"/>).</param>
        /// <returns>Existing or new element for currently processed binding.</returns>
        private XmlNode FindOrCreateElement(XmlDocument document, string rootElementName, string bindingIdentifier)
        {
            var root = document.DocumentElement;
            if (!IsRootElementValid(root, rootElementName))
            {
                // No root element exists within the document load from provided file, so no binding can be added
                return null;
            }

            var collection = SelectBindingsCollection(root);
            if (collection == null)
            {
                // No bindings collections exists within the document loaded from provided file, so no binding can be added
                return null;
            }

            // Get existing binding record or create a new element
            XmlNode node = null;
            if (CheckBindingExistanceBeforeAddition)
            {
                node = SelectBindingElement(collection, bindingIdentifier);
            }
            return node ?? CreateBindingElement(collection, bindingIdentifier);
        }


        /// <summary>
        /// Loads <paramref name="relativePath"/>'s content into a new XmlDocument.
        /// </summary>
        /// <param name="relativePath">Relative path to a file existing within the repository containing an XML document to be loaded.</param>
        protected virtual XmlDocument LoadDocumentFromFile(string relativePath)
        {
            string absolutePath = mConfiguration.GetAbsolutePath(relativePath);

            if (!File.Exists(absolutePath))
            {
                return null;
            }

            var document = new XmlDocument();
            using (var stream = File.OpenRead(absolutePath))
            {
                document.Load(stream);
            }

            return document;
        }


        /// <summary>
        /// Creates unique identifier for given <paramref name="binding"/>, so it can be distinguish within the file more easily.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object to create identifier of.</param>
        private string GetBindingIdentifier(BaseInfo binding)
        {
            var generalizedInfo = binding.Generalized;
            var typeInfo = generalizedInfo.TypeInfo;

            var identifierMembers = new List<string>();
            if (typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                identifierMembers.Add(generalizedInfo.ObjectSiteName);
            }
            
            // Fetch all info's binding dependencies.
            var dependencies = typeInfo
                .ObjectDependencies
                .Where(dependency => dependency.DependencyType == ObjectDependencyEnum.Binding)
                .Select(dependency => mTranslationHelper.TranslationReferenceLoader.LoadFromDatabase(dependency.DependencyObjectType, binding.GetIntegerValue(dependency.DependencyColumn, 0)))
                .Where(reference => reference != null)
                .Select(reference => reference.ToString());

            identifierMembers.AddRange(dependencies);

            // Generate name by aggregating (site name and) all binding dependencies
            return identifierMembers.Any()
                ? XmlHelper.XMLEncode(String.Join(BINDING_IDENTIFIER_SEPARATOR, identifierMembers))
                : null;
        }


        /// <summary>
        /// Returns true provided root element is not null and its name matches the <paramref name="expectedRootElementName"/>.
        /// </summary>
        /// <remarks>XML elements are case sensitive, so is the check.</remarks>
        /// <param name="rootElement">Root element to check.</param>
        /// <param name="expectedRootElementName">Expected name of the root element. If null, no check performed.</param>
        private static bool IsRootElementValid(XmlNode rootElement, string expectedRootElementName = null)
        {
            return rootElement != null && (expectedRootElementName == null || rootElement.Name == expectedRootElementName);
        }


        /// <summary>
        /// If exists, loads binding collection from provided root element, returns null otherwise.
        /// The collection should contain individual bindings without parent (that they have in common and that is present on the same level as the collection,
        /// under <see cref="BINDING_PARENT_ELEMENT"/>).
        /// </summary>
        /// <param name="rootElement">Root element the bindings collection is supposed to be in.</param>
        private static XmlNode SelectBindingsCollection(XmlNode rootElement)
        {
            return rootElement.SelectSingleNode(BINDINGS_COLLECTION_ELEMENT);
        }


        /// <summary>
        /// Returns string containing XPath identifying binding element(s) with specific <paramref name="bindingId"/> within <see cref="BINDINGS_COLLECTION_ELEMENT"/>.
        /// </summary>
        /// <param name="bindingId">Unique identifier of a binding that marks its individual data section in XML.</param>
        private static string GetBindingElementXPath(string bindingId)
        {
            return $"{BINDING_ELEMENT}[@{BINDING_ID_ATTRIBUTE}='{bindingId}']";
        }


        /// <summary>
        /// Returns single binding element with given <paramref name="bindingIdentifier"/>.
        /// </summary>
        /// <param name="bindingCollection">Collection of bindings (see <see cref="SelectBindingsCollection"/>).</param>
        /// <param name="bindingIdentifier">Unique identifier of a binding that marks its individual data section in XML.</param>
        private static XmlNode SelectBindingElement(XmlNode bindingCollection, string bindingIdentifier)
        {
            return bindingCollection.SelectSingleNode(GetBindingElementXPath(bindingIdentifier));
        }


        /// <summary>
        /// Returns all binding element with given  <paramref name="bindingIdentifier"/>.
        /// </summary>
        /// <param name="bindingCollection">Collection of bindings (see <see cref="SelectBindingsCollection"/>).</param>
        /// <param name="bindingIdentifier">Unique identifier of a binding that marks its individual data section in XML.</param>
        private static XmlNodeList SelectBindingElements(XmlNode bindingCollection, string bindingIdentifier)
        {
            return bindingCollection.SelectNodes(GetBindingElementXPath(bindingIdentifier));
        }


        /// <summary>
        /// Creates parent element right under the <paramref name="rootElement"/> filling it with parent's reference obtained from <paramref name="parent"/> object.
        /// </summary>
        /// <param name="rootElement">Root element of resulting XML.</param>
        /// <param name="parent">Parent object of a binding in question.</param>
        private void CreateParentElement(XmlNode rootElement, BaseInfo parent)
        {
            var parentElement = rootElement.OwnerDocument.CreateElement(BINDING_PARENT_ELEMENT);

            var parentReference = mTranslationHelper.TranslationReferenceLoader.LoadFromInfoObject(parent);
            parentReference.WriteToXmlElement(parentElement);
            rootElement.AppendChild(parentElement);
        }


        /// <summary>
        /// Creates collection of bindings right under the <paramref name="rootElement"/>, so each binding with given parent can be nested within the collection.
        /// </summary>
        /// <param name="rootElement">Root element of resulting XML.</param>
        /// <returns><see cref="XmlNode"/> that represents the bindings collection (the root of all individual bindings).</returns>
        private static void CreateBindingsCollection(XmlNode rootElement)
        {
            var collection = rootElement.OwnerDocument.CreateElement(BINDINGS_COLLECTION_ELEMENT);
            rootElement.AppendChild(collection);
        }


        /// <summary>
        /// Creates data-less element for a single binding object right under the binding collection element (see <seealso cref="CreateBindingsCollection"/>).
        /// </summary>
        /// <param name="bindingsCollection">Collection of individual bindings within a document (see <seealso cref="CreateBindingsCollection"/>).</param>
        /// <param name="bindingIdentifier">Binding's unique identifier within a document (see <seealso cref="GetBindingIdentifier"/>).</param>
        /// <returns><see cref="XmlNode"/> that represents a single binding object.</returns>
        private static XmlNode CreateBindingElement(XmlNode bindingsCollection, string bindingIdentifier)
        {
            var result = bindingsCollection.OwnerDocument.CreateElement(BINDING_ELEMENT);

            bindingsCollection.AppendChild(result);
            result.SetAttribute(BINDING_ID_ATTRIBUTE, bindingIdentifier);

            return result;
        }

        #endregion
    }
}
