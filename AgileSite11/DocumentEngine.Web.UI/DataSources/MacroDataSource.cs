using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.EventLog;
using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Users data source server control.
    /// </summary>
    [ToolboxData("<{0}:MacroDataSource runat=server />"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class MacroDataSource : CMSBaseDataSource
    {
        #region "Properties"

        /// <summary>
        /// Macro expression
        /// </summary>
        public string Expression
        {
            get;
            set;
        }       

        #endregion


        #region "Methods, events, handlers"
        
        /// <summary>
        /// Gets datasource from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (StopProcessing)
            {
                return null;
            }

            try
            {
                if (!String.IsNullOrEmpty(Expression))
                {
                    var res = MacroContext.CurrentResolver.ResolveMacroExpression(MacroProcessor.RemoveDataMacroBrackets(Expression), true);
                    if (res != null)
                    {
                       return res.Result;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MacroDataSource", "GetData", ex);
            }

            return null;
        }

        #endregion
    }
}