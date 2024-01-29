using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(SKUMethods), typeof(SKUInfo))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Methods for selecting SKUs - wrapping methods for macro resolver.
    /// </summary>
    internal class SKUMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns where condition to select bundle items.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns where condition to select bundle items.", 1)]
        [MacroMethodParam(0, "SKUInfo", typeof(SKUInfo), "SKUInfo object")]
        public static object GetBundleSelectorWhereCondition(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    var sku = (SKUInfo)parameters[0];

                    var where = new WhereCondition();

                    // Exclude product options
                    where.WhereNull("SKUOptionCategoryID");

                    // Exclude bundle products
                    where.WhereNotEquals("SKUProductType", SKUProductTypeEnum.Bundle.ToStringRepresentation());

                    // Exclude edited product itself
                    where.WhereNotEquals("SKUID", sku.SKUID);

                    // Exclude variant parents
                    where.WhereNotIn("SKUID", new IDQuery(SKUListInfo.OBJECT_TYPE).Column("SKUParentSKUID").WhereNotNull("SKUParentSKUID"));

                    // If bundle is global
                    if (sku.SKUSiteID == 0)
                    {
                        // Include global products
                        where.WhereNull("SKUSiteID");
                    }
                    else
                    {
                        // If global products are allowed on this site
                        if (ECommerceSettings.AllowGlobalProducts(SiteContext.CurrentSiteID))
                        {
                            // Include global and site products
                            where.Where(new WhereCondition().WhereNull("SKUSiteID").Or().WhereEquals("SKUSiteID", SiteContext.CurrentSiteID));
                        }
                        else
                        {
                            // Include site products
                            where.WhereEquals("SKUSiteID", SiteContext.CurrentSiteID);
                        }
                    }

                    // Include only enabled products
                    where.WhereTrue("SKUEnabled");

                    // Condition for bundle items already in this bundle (to remove/view also disabled SKUs)
                    var alreadyInBundleCondition = new WhereCondition().WhereIn("SKUID",
                        BundleInfoProvider.GetBundles().Column("SKUID").WhereEquals("BundleID", sku.SKUID));

                    // Clasic where OR items already in bundle
                    return new WhereCondition().Where(where).Or().Where(alreadyInBundleCondition).ToString(true);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns where condition to select tax class for the specified SKU.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns where condition to select tax class.", 1)]
        [MacroMethodParam(0, "SKUInfo", typeof(SKUInfo), "SKUInfo object")]
        public static object GetTaxClassSelectorWhereCondition(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new NotSupportedException();
            }

            var sku = parameters[0] as SKUInfo;
            if (sku == null)
            {
                return "";
            }

            var where = new WhereCondition();

            // If only global taxes are allowed on this site
            if (sku.IsGlobal || ECommerceSettings.UseGlobalTaxClasses(SiteContext.CurrentSiteID))
            {
                // Only global tax classes
                where.Where(new WhereCondition().WhereNull("TaxClassSiteID"));
            }
            else
            {
                // Only site taxes
                where.WhereEquals("TaxClassSiteID", SiteContext.CurrentSiteID);
            }

            // Add selected tax
            if (sku.SKUTaxClassID > 0)
            {
                where.Or().WhereEquals("TaxClassID", sku.SKUTaxClassID);
            }

            return where.ToString(true);
        }


        [MacroMethod(typeof(bool), "Returns whether SKUInfo object belongs to at least one of given sections.", 2)]
        [MacroMethodParam(0, "SKUInfo", typeof(SKUInfo), "SKUInfo object")]
        [MacroMethodParam(1, "sections", typeof(IEnumerable<string>), "GUIDs of given sections")]
        public static object IsInSections(EvaluationContext context, params object[] parameters)
        {
            if ((parameters.Length < 0) || (parameters.Length > 2))
            {
                throw new NotSupportedException();
            }

            var sku = parameters[0] as SKUInfo;
            if (sku == null)
            {
                return false;
            }

            var sections = parameters[1] as IEnumerable<string>;
            if (sections == null)
            {
                return false;
            }

            var nodeIDs = DocumentNodeDataInfoProvider.GetDocumentNodes().WhereIn("NodeGUID", sections.Select(s => s.ToGuid(Guid.Empty)).ToList()).OnSite(SiteContext.CurrentSiteID).Column("NodeID").GetListResult<int>();

            
            // Check whether given SKU belongs to given sections
            return sku.SectionIDs.Intersect(nodeIDs).Any();
        }
    }
}
