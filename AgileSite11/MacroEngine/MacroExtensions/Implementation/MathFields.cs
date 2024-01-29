
using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide fields from System.Math namespace in the MacroEngine.
    /// </summary>
    internal class MathFields : MacroFieldContainer
    {
        /// <summary>
        /// Registers the math fields.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            RegisterField(new MacroField("PI", () => Math.PI));
        }
    }
}