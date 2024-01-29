using System;
using System.Linq;
using System.Text;

namespace CMS.UIControls
{
    /// <summary>
    /// Mode of SQL edit control.
    /// </summary>
    public enum SqlEditModeEnum
    {
        /// <summary>
        /// Nothing will happen.
        /// </summary>
        None = 0,

        /// <summary>
        /// Existing view will be modified
        /// </summary>
        AlterView = 1,

        /// <summary>
        /// New view will be created
        /// </summary>
        CreateView = 2,

        /// <summary>
        /// Existing stored procedure will be modified
        /// </summary>
        AlterProcedure = 3,

        /// <summary>
        /// New stored procedure will be created
        /// </summary>
        CreateProcedure = 4
    }
}
