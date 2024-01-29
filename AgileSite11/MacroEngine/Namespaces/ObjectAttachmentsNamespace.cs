using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(ObjectAttachmentsNamespace))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide names of the object attachment categories to the MacroEngine.
    /// </summary>
    [Extension(typeof(ObjectAttachmentCategoriesFields))]
    public class ObjectAttachmentsNamespace : MacroNamespace<ObjectAttachmentsNamespace>
    {
    }
}