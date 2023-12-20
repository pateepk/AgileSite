using CMS.Base;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide fields from DenugHelper in the MacroEngine.
    /// </summary>
    internal class DebugSettingsFields : MacroFieldContainer
    {
        protected override void RegisterFields()
        {
            base.RegisterFields();

            RegisterField(new MacroField("AnyDebugLogToFileEnabled", () => DebugHelper.AnyDebugLogToFileEnabled));
            RegisterField(new MacroField("AnyDebugEnabled", () => DebugHelper.AnyDebugEnabled));
        }
    }
}
