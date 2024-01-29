using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace CMS.SharePoint
{
    /// <summary>
    /// Helps building Collaborative Application Markup Language (CAML) queries.
    /// </summary>
    internal class CamlQueryBuilder
    {
        #region "Fields"

        private readonly XmlDocument queryDocument = new XmlDocument();
        private List<string> mViewFields = new List<string>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the inner XML of CAML Query element.
        /// </summary>
        /// <seealso cref="ItemSelectionFieldName"/>
        /// <seealso cref="ItemSelectionFieldType"/>
        /// <seealso cref="ItemSelectionFieldValue"/>
        public string QueryInnerXml
        {
            get
            {
                return Query.InnerXml;
            }
            set
            {
                Query.InnerXml = value;
            }
        }


        /// <summary>
        /// Gets the CAML Query root element.
        /// </summary>
        public XmlElement Query
        {
            get
            {
                return queryDocument.DocumentElement;
            }
        }


        /// <summary>
        /// Specifies the limit of rows retrieved from SharePoint server.
        /// Non-positive values mean no limit.
        /// </summary>
        public int RowLimit
        {
            get;
            set;
        }


        /// <summary>
        /// List containing field names to be retrieved from SharePoint server.
        /// The SharePoint internal names must be used.
        /// Keeping null causes all fields to be retrieved.
        /// </summary>
        public List<string> ViewFields
        {
            get
            {
                return mViewFields;
            }
            set
            {
                mViewFields = value;
            }
        }


        /// <summary>
        /// Field name (internal name) for selection of certain item(s) in which the <see cref="ItemSelectionFieldValue"/> is being searched (using equality operator).
        /// The field is usually some unique field but it is not a must.
        /// The selection is made from elements satisfying the <see cref="Query"/>.
        /// The selection part of query will look like the following<br />
        /// &lt;Eq>&lt;FieldRef Name="ItemSelectionFieldName" />&lt;Value Type="ItemSelectionFieldType">ItemSelectionFieldValue&lt;/Value>&lt;/Eq>
        /// </summary>
        public string ItemSelectionFieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Type of field specified in <see cref="ItemSelectionFieldName"/> for selection of certain item(s).
        /// </summary>
        public string ItemSelectionFieldType
        {
            get;
            set;
        }


        /// <summary>
        /// Searched value of <see cref="ItemSelectionFieldName"/> for selection of certain item(s).
        /// The selection is made from elements satisfying the <see cref="Query"/>.
        /// The selection part of query will look like the following<br />
        /// &lt;Eq>&lt;FieldRef Name="ItemSelectionFieldName" />&lt;Value Type="ItemSelectionFieldType">ItemSelectionFieldValue&lt;/Value>&lt;/Eq>
        /// </summary>
        public string ItemSelectionFieldValue
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets CAML suitable for <see cref="Microsoft.SharePoint.Client.CamlQuery.ViewXml"/> property (including &lt;View> element).
        /// </summary>
        /// <param name="viewAttributes">Collection of view element attributes.</param>
        /// <returns>View XML.</returns>
        /// <seealso cref="Microsoft.SharePoint.Client.CamlQuery.ViewXml"/>
        public string GetViewXmlAsString(Dictionary<string, string> viewAttributes = null)
        {
            // Note: The creation of resulting View element must have no side effect on class properties
            XmlDocument viewDocument = CreateViewDocument(viewAttributes);
            XmlElement viewElement = viewDocument.DocumentElement;

            // Merge the Where clause from Query and Where clause coming from item selection.
            string queryInnerXml = Query.InnerXml;
            string itemSelectionXml = GetItemSelectionXml();
            XmlElement queryElement = viewDocument.CreateElement("Query");
            if (!String.IsNullOrEmpty(queryInnerXml))
            {
                queryElement.InnerXml = queryInnerXml;
            }
            if (!String.IsNullOrEmpty(itemSelectionXml))
            {
                var whereNode = queryElement.SelectSingleNode("Where");
                if (whereNode == null)
                {
                    whereNode = viewDocument.CreateElement("Where");
                    queryElement.AppendChild(whereNode);
                }

                string whereNodeInnerXml = whereNode.InnerXml;
                whereNode.InnerXml = (String.IsNullOrEmpty(whereNodeInnerXml)) ? itemSelectionXml : "<And>" + itemSelectionXml + whereNodeInnerXml + "</And>";
            }

            // Include the Query element only if needed
            if (!String.IsNullOrEmpty(queryElement.InnerXml))
            {
                viewElement.AppendChild(queryElement);
            }

            // Include the RowLimit element only if needed
            if (RowLimit > 0)
            {
                XmlElement rowLimitElement = viewDocument.CreateElement("RowLimit");
                rowLimitElement.InnerText = RowLimit.ToString();
                viewElement.AppendChild(rowLimitElement);
            }

            // Limit the retrieved fields only if needed
            if ((ViewFields != null) && (ViewFields.Count > 0))
            {
                XmlElement viewFieldsElement = viewDocument.CreateElement("ViewFields");
                viewElement.AppendChild(viewFieldsElement);

                foreach (string fieldRef in ViewFields)
                {
                    XmlElement fieldRefElement = viewDocument.CreateElement("FieldRef");
                    fieldRefElement.SetAttribute("Name", fieldRef.Trim());
                    viewFieldsElement.AppendChild(fieldRefElement);
                }
            }

            return viewElement.OuterXml;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets CAML for item selection. Something like
        /// &lt;Eq>&lt;FieldRef Name="ItemSelectionFieldName" />&lt;Value Type="ItemSelectionFieldType">ItemSelectionFieldValue&lt;/Value>&lt;/Eq>
        /// </summary>
        /// <returns>CAML for item selection, or null if <see cref="ItemSelectionFieldName"/>, <see cref="ItemSelectionFieldType"/> or <see cref="ItemSelectionFieldValue"/> are not provided.</returns>
        private string GetItemSelectionXml()
        {
            if (String.IsNullOrEmpty(ItemSelectionFieldName) || String.IsNullOrEmpty(ItemSelectionFieldType) || String.IsNullOrEmpty(ItemSelectionFieldValue))
            {
                return null;
            }

            XmlDocument doc = new XmlDocument();
            var eqElement = doc.CreateElement("Eq");
            doc.AppendChild(eqElement);

            var fieldRefElement = doc.CreateElement("FieldRef");
            fieldRefElement.SetAttribute("Name", ItemSelectionFieldName);
            eqElement.AppendChild(fieldRefElement);

            var valueElement = doc.CreateElement("Value");
            valueElement.SetAttribute("Type", ItemSelectionFieldType);
            valueElement.InnerText = ItemSelectionFieldValue;
            eqElement.AppendChild(valueElement);

            return doc.InnerXml;
        }


        /// <summary>
        /// Creates a new document with &lt;View> as a root and optionally with attributes.
        /// </summary>
        /// <param name="viewAttributes">Attributes for &lt;View> element</param>
        /// <returns>Document with View element and its attributes</returns>
        private XmlDocument CreateViewDocument(Dictionary<string, string> viewAttributes = null)
        {
            XmlDocument viewElement = new XmlDocument();
            viewElement.LoadXml("<View></View>");
            XmlElement documentElement = viewElement.DocumentElement;
            if (viewAttributes != null)
            {
                foreach (KeyValuePair<string, string> attribute in viewAttributes)
                {
                    documentElement.SetAttribute(attribute.Key, attribute.Value);
                }
            }

            return viewElement;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new CAML query builder.
        /// </summary>
        public CamlQueryBuilder()
        {
            // Prepare the root element
            queryDocument.LoadXml("<Query></Query>");
        }

        #endregion
    }
}
