using System;

using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.MacroEngine;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(MultiBuyDiscountMethods), typeof(MultiBuyDiscountInfo))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// MultiBuy discount methods - wrapping methods for macro resolver.
    /// </summary>
    internal class MultiBuyDiscountMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns where condition to remove products which can not be used in MultiBuy discount.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns where condition to filter products which can be used in MultiBuy discount.", 1)]
        [MacroMethodParam(0, "multiBuyDiscountInfo", typeof(MultiBuyDiscountInfo), "MultiBuy discount info")]
        public static object GetMultiBuyProductSelectorWhereCondition(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    var parents = new QuerySourceTable("COM_SKU", "parentIDs");

                    // Select IDs of all products that are not parents of variants
                    var ids = new IDQuery<SKUInfo>()
                        .Columns(new QueryColumn("COM_SKU.SKUID"))
                        .Source(s => s.LeftJoin(parents, "COM_SKU.SKUID", "parentIDs.SKUParentSKUID"))
                        .Where(new WhereCondition().WhereNull("parentIDs.SKUID"));

                    var whereCondition = new WhereCondition()
                        .WhereTrue("SKUEnabled")
                        .WhereNull("SKUOptionCategoryID")
                        .WhereIn("SKUID", ids)
                        .Where(GenerateSiteCondition());

                    return whereCondition.ToString(true);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Generates where condition to select only products usable on current site, current site products and global products if they are allowed on current site.
        /// </summary>
        private static WhereCondition GenerateSiteCondition()
        {
            var siteWhere = new WhereCondition()
                .WhereEquals("SKUSiteID", SiteContext.CurrentSiteID);

            // Add global products if global objects are allowed
            if (ECommerceSettings.AllowGlobalProducts(SiteContext.CurrentSiteID))
            {
                siteWhere.Or().WhereNull("SKUSiteID");
            }

            return siteWhere;
        }


        /// <summary>
        /// Generates where condition to select only departments used in current site 
        /// and global departments depending on allow global departments setting.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns where condition to select departments allowed to use at current site and for current user.", 0)]
        public static object GetMultiBuyDepartmentSelectorWhereCondition(EvaluationContext context, params object[] parameters)
        {
            var siteId = SiteContext.CurrentSiteID;

            var where = new WhereCondition()
                .WhereEquals("DepartmentSiteID", siteId);

            if (ECommerceSettings.AllowGlobalDepartments(siteId))
            {
                where.Or().WhereNull("DepartmentSiteID");
            }

            return where.ToString(true);
        }
    }
}

