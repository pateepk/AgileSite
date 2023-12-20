using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms.Web.UI;

[assembly: RegisterModule(typeof(OnlineFormsWebUIModule))]

namespace CMS.OnlineForms.Web.UI
{
    /// <summary>
    /// Represents the Form web UI module.
    /// </summary>
    public class OnlineFormsWebUIModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlineFormsWebUIModule"/> class.
        /// </summary>
        public OnlineFormsWebUIModule()
            : base(ModuleName.ONLINEFORMSWEBUI)
        {
        }


        /// <summary>
        /// Initializes the Form web UI module.
        /// </summary>
        protected override void OnInit()
        {
            TempUploadFilesCleanup.RegisterUploadedFilesCleanupAction();

            base.OnInit();
        }
    }
}
