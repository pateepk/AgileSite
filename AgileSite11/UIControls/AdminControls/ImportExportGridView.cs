using System;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// ImportExportGridView with checkBoxes.
    /// Used for import and export site objects.
    /// </summary>
    public abstract class ImportExportGridView : ImportExportBase
    {
        #region "Private variables"

        /// <summary>
        /// Preposition for checkBox ID and checkBox name.
        /// </summary>
        private const string PREPOSITION = "echck_";
       
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
        /// Ensure objects preselection.
        /// </summary>
        /// <param name="codeName">Code name</param>
        public abstract bool IsChecked(object codeName);

        #endregion
    }
}
