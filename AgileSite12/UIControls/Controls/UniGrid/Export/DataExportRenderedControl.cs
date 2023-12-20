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
        }


        /// <summary>
        /// Target column name
        /// </summary>
        public string ColumnName
        {
            get;
        }


        /// <summary>
        /// DataRow with the target data
        /// </summary>
        public DataRow DataRow
        {
            get;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Control to render</param>
        /// <param name="dataRow">DataRow with the target data</param>
        /// <param name="columnName">Target column name</param>
        public DataExportRenderedControl(Control control, DataRow dataRow, string columnName)
        {
            Control = control;
            DataRow = dataRow;
            ColumnName = columnName;
        }
    }
}
