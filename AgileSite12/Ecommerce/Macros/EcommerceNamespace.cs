using CMS.Base;
using CMS.Ecommerce;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(EcommerceNamespace), AllowAnonymous = true)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Macro namespace for ecommerce macro methods.
    /// </summary>
    [Extension(typeof(EProductMethods))]
    internal sealed class EcommerceNamespace : MacroNamespace<EcommerceNamespace>
    {
    }
}