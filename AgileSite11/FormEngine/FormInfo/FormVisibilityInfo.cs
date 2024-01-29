using System;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Ensures management of XML file that represents the form visibility definition.
    /// </summary>
    public class FormVisibilityInfo
    {
        #region "Variables"

        /// <summary>
        /// FormInfo initialized by constructor of this class.
        /// </summary>
        private FormInfo fi;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty FormVisibilityInfo structure.
        /// </summary>
        public FormVisibilityInfo()
        {
        }


        /// <summary>
        /// Constructor, creates the FormVisibilityInfo structure and loads visibility definition.
        /// </summary>
        /// <param name="visibilityDefinition">XML definition in the form [&lt;form name="..."&gt;&lt;field column="..." visibility="..." /&gt;&lt;/form&gt;]*</param>
        /// <param name="formName">Form name</param>
        public FormVisibilityInfo(string visibilityDefinition, string formName)
        {
            fi = LoadXmlDefinition(visibilityDefinition, formName);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns FormInfo created by constructor of this class.
        /// </summary>
        public FormInfo GetFormInfo()
        {
            return fi;
        }


        /// <summary>
        /// Loads XML document from the specified string.
        /// </summary>
        /// <param name="xmlString">String containing XML document to load</param>
        /// <param name="formName">Form name</param>
        private FormInfo LoadXmlDefinition(string xmlString, string formName)
        {
            var doc = LoadVisibilityDocument(xmlString);
            
            XmlNode formNode = SelectFormXmlNode(doc, formName);
            if (formNode != null)
            {
                return new FormInfo(formNode.OuterXml);
            }

            return null;
        }


        /// <summary>
        /// Inserts or updates specified field node.
        /// </summary>
        /// <param name="fieldName">Name of a field to update</param>
        /// <param name="visibility">Visibility value</param>
        /// <param name="formName">Form name, used if load definition is null.</param>
        public void SetVisibilityField(string fieldName, string visibility, string formName)
        {
            if (String.IsNullOrEmpty(fieldName) || String.IsNullOrEmpty(visibility) || String.IsNullOrEmpty(formName))
            {
                return;
            }

            if (fi == null)
            {
                fi = new FormInfo(null)
                {
                    FormName = formName
                };
            }

            var ffi = EnsureFormField(fi, fieldName);

            ffi.Visibility = visibility.ToEnum<FormFieldVisibilityTypeEnum>();
        }


        /// <summary>
        /// Returns <see cref="FormFieldInfo"/> for a field with given <paramref name="fieldName"/>. Creates a new field in given <paramref name="form"/> if it didn't exist there yet.
        /// </summary>
        /// <param name="form"><see cref="FormInfo"/> that should contain the field.</param>
        /// <param name="fieldName">Field name</param>
        private static FormFieldInfo EnsureFormField(FormInfo form, string fieldName)
        {
            FormFieldInfo ffi = form.GetFormField(fieldName);
            if (ffi == null)
            {
                ffi = new FormFieldInfo
                {
                    Name = fieldName
                };

                form.AddFormItem(ffi);
            }

            return ffi;
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Merges full visibility definition with visibility definition of specific form.
        /// </summary>
        /// <param name="fullDefinition">May contain several form visibility definitions</param>
        /// <param name="formVisibility">Visibility definition of specific form</param>
        /// <param name="formName">Form name</param>
        public static string MergeVisibility(string fullDefinition, FormInfo formVisibility, string formName)
        {
            if ((formVisibility == null) || String.IsNullOrEmpty(formName))
            {
                // Return non-changed definition
                return fullDefinition;
            }

            var xmlDoc = LoadVisibilityDocument(fullDefinition);
            var docElem = xmlDoc.DocumentElement;

            // Get XML node for specific form name
            var formNode = SelectFormXmlNode(xmlDoc, formName);
            if (formNode != null)
            {
                // Remove form visibility definition
                docElem.RemoveChild(formNode);
            }

            // Append entire node which is not present in the original definition
            var newElement = formVisibility.GetVisibilityElement();

            var newNode = xmlDoc.ImportNode(newElement, true);
            docElem.AppendChild(newNode);

            return docElem.OuterXml;
        }


        /// <summary>
        /// Constructs form visibilty document from given XML.
        /// </summary>
        /// <param name="xml">XML containing form visibility definition.</param>
        private static XmlDocument LoadVisibilityDocument(string xml)
        {
            var doc = new XmlDocument();

            if (!String.IsNullOrEmpty(xml))
            {
                doc.LoadXml(xml);
            }

            if (doc.DocumentElement == null)
            {
                doc.AppendChild(doc.CreateElement("forms"));
            }

            return doc;
        }


        /// <summary>
        /// Gets XML node representing specific form visibility definition.
        /// </summary>
        /// <param name="xml">XML document to work with</param>
        /// <param name="formName">Form name</param>
        private static XmlNode SelectFormXmlNode(XmlDocument xml, string formName)
        {
            if (!String.IsNullOrEmpty(formName))
            {
                // Get XML node with form visibility definition
                return xml.SelectSingleNode("forms/form[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') ='" + formName.ToLowerCSafe() + "']");
            }

            return null;
        }

        #endregion
    }
}