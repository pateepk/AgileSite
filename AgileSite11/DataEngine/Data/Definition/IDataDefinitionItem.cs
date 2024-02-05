using System;
using System.Data;
using System.Xml;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for data definition items.
    /// </summary>
    public interface IDataDefinitionItem
    {
        #region "Methods"

        /// <summary>
        /// Clones current object and returns copy of it.
        /// </summary>
        IDataDefinitionItem Clone();


        /// <summary>
        /// Returns XML representation of current object.
        /// </summary>
        /// <param name="doc">XML document with other items</param>
        XmlNode GetXmlNode(XmlDocument doc);


        /// <summary>
        /// Loads the field info from XML node.
        /// </summary>
        /// <param name="node">Field node</param>
        void LoadFromXmlNode(XmlNode node);


        /// <summary>
        /// Loads the field info from plain database structure data.
        /// </summary>
        /// <param name="row">Data row with structure information</param>
        /// <param name="isPrimary">Indicates if field represents primary key</param>
        /// <param name="isSystem">Indicates if field is system field</param>
        /// <remarks>Database structure data can be obtained via <see cref="CMS.DataEngine.TableManager.GetColumnInformation(string, string)"/>.</remarks>
        void LoadFromTableData(DataRow row, bool isPrimary, bool isSystem);

        #endregion
    }
}