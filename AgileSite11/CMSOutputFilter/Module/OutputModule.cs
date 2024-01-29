using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Base;
using CMS.OutputFilter;

[assembly: RegisterModule(typeof(OutputModule))]

namespace CMS.OutputFilter
{
    /// <summary>
    /// Represents the Output filter module.
    /// </summary>
    public class OutputModule: Module
    {
         /// <summary>
        /// Default constructor
        /// </summary>
        public OutputModule()
            : base(new OutputModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            if (!SystemContext.IsCMSRunningAsMainApplication)
            {
                return;
            }

            DebugHelper.RegisterDebug(OutputDebug.Settings);
        }


        /// <summary>
        /// Module init
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            if (!SystemContext.IsCMSRunningAsMainApplication)
            {
                return;
            }

            OutputFilterHandlers.Init();
        }

        #endregion
    }
}
