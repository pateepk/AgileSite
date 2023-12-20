using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.UIControls;

[assembly: RegisterModule(typeof(UIControlsModule))]

namespace CMS.UIControls
{
    /// <summary>
    /// Represents the UI controls module.
    /// </summary>
    internal class UIControlsModule : Module
    {
        /// <summary>
        /// Name of the UI controls module.
        /// </summary>
        public const string MODULE_NAME = "CMS.UIControls";


        /// <summary>
        /// Initializes a new instance of the <see cref="UIControlsModule"/> class.
        /// </summary>
        public UIControlsModule() : base(MODULE_NAME)
        {
        }


        /// <summary>
        /// Pre-initializes the UI controls module.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();
            ApplicationEvents.UpdateData.Execute += (sender, e) => HotfixProcedure.Hotfix();
        }
    }
}
