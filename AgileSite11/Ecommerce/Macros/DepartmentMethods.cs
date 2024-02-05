using System;

using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.MacroEngine;
using CMS.DataEngine.Query;

[assembly: RegisterExtension(typeof(DepartmentMethods), typeof(DepartmentInfo))]
namespace CMS.Ecommerce
{
    internal class DepartmentMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns where condition to select tax classes which can be assigned to edited department.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns where condition to select tax classes which can be assigned to current department", 1)]
        [MacroMethodParam(0, "department", typeof(DepartmentInfo), "Department info")]
        public static object GetDepartmentTaxClassSelectorWhereCondition(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new NotSupportedException();
            }

            var department = (DepartmentInfo)parameters[0];
            if (department == null)
            {
                return "";
            }

            var where = new WhereCondition();

            // Get tax class assigned to department
            where.WhereEquals("TaxClassID", department.DepartmentDefaultTaxClassID);

            // Add site specific records when global taxes are not allowed
            int taxSiteId = 0;
            if (department.DepartmentSiteID > 0)
            {
                if (!ECommerceSettings.UseGlobalTaxClasses(department.DepartmentSiteID))
                {
                    taxSiteId = department.DepartmentSiteID;
                }
            }

            where.Or().WhereEquals("TaxClassSiteID".AsColumn().IsNull(0), taxSiteId);

            return where.ToString(true);
        }
    }
}
