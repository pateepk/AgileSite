using CMS;
using CMS.MacroEngine;
using CMS.Base;
using CMS.Newsletters;

[assembly: RegisterExtension(typeof(NewsletterEnums), typeof(EnumsNamespace))]

namespace CMS.Newsletters
{
    /// <summary>
    /// Wrapper class to provide newsletter enumerations.
    /// </summary>
    internal class NewsletterEnums : MacroFieldContainer
    {
        /// <summary>
        /// Registers the newsletter enumerations.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            // Register enumerations
            RegisterField(new MacroField("EmailTemplateTypeEnum", () => new EnumDataContainer(typeof(EmailTemplateTypeEnum))));
        }
    }
}