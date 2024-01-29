using System.Data;
using System.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Container for the rendered control
    /// </summary>
    internal class DataExportRenderedControl
    {
        /// <summary>
        /// Control to render
        /// </summary>
        public Control Control
        {
            get;
            protected set;
        }


        /// <summary>
        /// Target column name
        /// </summary>
        public string ColumnName
        {
            get;
            protected set;
        }


        /// <summary>
        /// DataRow with the target data
        /// </summary>
        public DataRow DataRow
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c">Control to render</param>
        /// <param name="dr">DataRow with the target data</param>
        /// <param name="colName">Target column name</param>
        public DataExportRenderedControl(Control c, DataRow dr, string colName)
        {
            Control = c;
            DataRow = dr;
            ColumnName = colName;
        }
    }
}
