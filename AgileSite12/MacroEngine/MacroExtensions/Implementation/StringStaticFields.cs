namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide string static fields.
    /// </summary>
    internal class StringStaticFields : MacroFieldContainer
    {
        /// <summary>
        /// Registers the string static fields.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            // Register special values
            RegisterField(new MacroField("Empty", () => string.Empty));
        }
    }
}