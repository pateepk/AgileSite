using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide fields from DateTime class in the MacroEngine.
    /// </summary>
    internal class DateTimeStaticFields : MacroFieldContainer
    {
        /// <summary>
        /// Registers the math fields.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            RegisterField(new MacroField("Now", () => DateTime.Now));
            RegisterField(new MacroField("UtcNow", () => DateTime.UtcNow));
            RegisterField(new MacroField("Today", () => DateTime.Today));
            RegisterField(new MacroField("MinValue", () => DateTime.MinValue));
            RegisterField(new MacroField("MaxValue", () => DateTime.MaxValue));
            RegisterField(new MacroField("CurrentDateTime", (x) => DateTime.Now));
            RegisterField(new MacroField("CurrentTime", (x) => DateTime.Now.ToShortTimeString()));
            RegisterField(new MacroField("CurrentDate", (x) => DateTime.Now.ToShortDateString()));
        }
    }
}