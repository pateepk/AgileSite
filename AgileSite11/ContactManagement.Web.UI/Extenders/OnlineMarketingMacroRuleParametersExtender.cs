using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.MacroEngine;
using CMS.ContactManagement.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.UIControls;

[assembly: RegisterCustomClass("OnlineMarketingMacroRuleParametersExtender", typeof(OnlineMarketingMacroRuleParametersExtender))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Extender for on-line marketing macro rule parameters editor
    /// </summary>
    public class OnlineMarketingMacroRuleParametersExtender : ControlExtender<BaseFieldEditor>
    {
        /// <summary>
        /// OnInit event handler
        /// </summary>
        public override void OnInit()
        {
            Control.Load += EditForm_OnAfterDataLoad;
        }


        private void EditForm_OnAfterDataLoad(object sender, EventArgs e)
        {
            MacroRuleInfo info = Control.EditedObject as MacroRuleInfo;
            if (info != null)
            {
                string macroName = info.MacroRuleName;
                if (!MacroRuleMetadataContainer.IsTranslatorAvailable(macroName))
                {
                    var text = string.Format(Control.GetString("om.configuration.macro.slow"), DocumentationHelper.GetDocumentationTopicUrl("om_macro_performance"));
                    Control.ShowWarning(text);
                }
            }
        }
    }
}