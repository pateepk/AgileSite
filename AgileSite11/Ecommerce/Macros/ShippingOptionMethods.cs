using System;

using CMS;
using CMS.Ecommerce;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(ShippingOptionMethods), typeof(ShippingOptionInfo))]
namespace CMS.Ecommerce
{
    internal class ShippingOptionMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns where condition to select tax class which can be assigned to edited shipping option.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns where condition to select tax class which can be assigned to current shipping option", 1)]
        [MacroMethodParam(0, "shippingOption", typeof(ShippingOptionInfo), "Shipping option info")]
        public static object GetShippingOptionTaxClassSelectorWhereCondition(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new NotSupportedException();
            }
            
            var shippingOption = parameters[0] as ShippingOptionInfo;
            if (shippingOption == null)
            {
                return "";
            }

            // Use taxes suitable for edited shipping
            var query = TaxClassInfoProvider.GetTaxClasses(shippingOption.ShippingOptionSiteID);
           
            // Add selected tax
            if (shippingOption.ShippingOptionTaxClassID > 0)
            {
                query.Or().WhereEquals("TaxClassID", shippingOption.ShippingOptionTaxClassID);
            }

            return query.Expand(query.WhereCondition);
        }
    }
}
