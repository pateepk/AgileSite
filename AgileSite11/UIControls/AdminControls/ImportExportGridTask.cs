using System;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// ImportExportGridTask with checkBoxes.
    /// Used for tasks import and export.
    /// </summary>
    public abstract class ImportExportGridTask : ImportExportBase
    {
        #region "Private variables"

        /// <summary>
        /// Preposition for checkBox ID and checkBox name.
        /// </summary>
        private const string PREPOSITION = "etchck_";
        

        /// <summary>
        /// Used for selecting tasks from grid.
        /// </summary>
        protected const string TASK_ID = "TaskId";
        
        #endregion


        #region "Public methods"
        
        /// <summary>
        /// Returns CMSCheckBox with specified ID and name.
        /// </summary>
        /// <param name="codeNameColumnName">CodeName</param>
        /// <returns>CMSCheckBox</returns>
        public CMSCheckBox GetCheckBox(string codeNameColumnName)
        {
            var check = GetCheckBox(codeNameColumnName, PREPOSITION);
            check.Checked = IsChecked(codeNameColumnName);
            return check;
        }


        /// <summary>
        /// Returns the name for the task checkBox.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        public string GetCheckBoxName(object taskId)
        {
            return GetCheckBoxName(taskId, PREPOSITION);
        }

        #endregion
        

        #region "Abstract methods"

        /// <summary>
        /// Ensure tasks preselection.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        public abstract bool IsChecked(object taskId);

        #endregion
    }
}
