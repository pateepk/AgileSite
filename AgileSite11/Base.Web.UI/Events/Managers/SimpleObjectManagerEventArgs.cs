using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.WorkflowEngine;
using CMS.DataEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Simple object manager event arguments
    /// </summary>
    public class SimpleObjectManagerEventArgs : ManagerEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Object
        /// </summary>
        public BaseInfo InfoObject
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="mode">Manager mode</param>
        public SimpleObjectManagerEventArgs(BaseInfo infoObj, FormModeEnum mode)
            : base(mode, null)
        {
            InfoObject = infoObj;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="mode">Manager mode</param>
        /// <param name="actionName">Action name</param>
        public SimpleObjectManagerEventArgs(BaseInfo infoObj, FormModeEnum mode, string actionName)
            : base(mode, actionName)
        {
            InfoObject = infoObj;
        }

        #endregion
    }
}
