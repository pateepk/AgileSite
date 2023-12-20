using System;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Macro methods for e-products.
    /// </summary>
    internal sealed class EProductMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns e-product download URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns permanent URL to the specified e-product file.", 3)]
        [MacroMethodParam(0, "fileToken", typeof(Guid), "File unique download token")]
        [MacroMethodParam(1, "fileName", typeof(string), "File name")]
        [MacroMethodParam(2, "siteId", typeof(int), "Site ID of the order in which the e-product is included")]
        public static string GetEproductUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    var token = ValidationHelper.GetGuid(parameters[0], Guid.Empty);
                    var fileName = ValidationHelper.GetString(parameters[1], String.Empty);
                    var siteId = ValidationHelper.GetInteger(parameters[2], 0);

                    return OrderItemSKUFileInfoProvider.GetOrderItemSKUFileUrl(token, fileName, siteId);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
