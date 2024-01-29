using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(OptionCategoryMethods), typeof(OptionCategoryInfo))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Option category methods - wrapping methods for macro resolver.
    /// </summary>
    internal class OptionCategoryMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if product is global.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if product is global.", 2)]
        [MacroMethodParam(0, "optionCategoryInfo", typeof(OptionCategoryInfo), "Option category info")]
        [MacroMethodParam(1, "productID", typeof(int), "Product ID")]
        public static object IsProductGlobal(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    var productObj = SKUInfoProvider.GetSKUInfo(ValidationHelper.GetInteger(parameters[1], 0));
                    return (productObj != null) && productObj.IsGlobal;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
