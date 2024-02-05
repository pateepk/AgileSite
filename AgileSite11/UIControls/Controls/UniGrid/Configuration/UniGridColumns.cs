using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid Columns definition.
    /// </summary>
    [ParseChildren(typeof(Column), DefaultProperty = "Columns", ChildrenAsProperties = true)]
    public class UniGridColumns
    {
        #region "Properties"

        /// <summary>
        /// List of the Columns.
        /// </summary>
        public List<Column> Columns
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public UniGridColumns()
        {
            Columns = new List<Column>();
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">Columns XML element</param>
        public UniGridColumns(XElement element)
            : this()
        {
            foreach (var child in element.GetElements("column"))
            {
                Columns.Add(new Column(child));
            }
        }

        #endregion
    }
}