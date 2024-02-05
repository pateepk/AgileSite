using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Arguments for the evaluation of data column in transformation event.
    /// </summary>
    public class TransformationEvalEventArgs : CMSEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Source data.
        /// </summary>
        public object Data
        {
            get;
            internal set;
        }


        /// <summary>
        /// Name of the column.
        /// </summary>
        public string ColumnName
        {
            get;
            internal set;
        }


        /// <summary>
        /// Output value.
        /// </summary>
        public object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that the value is provided by the handler.
        /// </summary>
        public bool HasValue
        {
            get;
            set;
        }

        #endregion
    }
}