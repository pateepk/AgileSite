using System;

using CMS.Ecommerce;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds department API examples.
    /// </summary>
    /// <pageTitle>Departments</pageTitle>
    internal class DepartmentsMain
    {
        /// <summary>
        /// Holds department API examples.
        /// </summary>
        /// <groupHeading>Departments</groupHeading>
        private class Departments
        {
            /// <heading>Creating a new department</heading>
            private void CreateDepartment()
            {
                // Creates a new department object
                DepartmentInfo newDepartment = new DepartmentInfo();

                // Sets the department properties
                newDepartment.DepartmentDisplayName = "New department";
                newDepartment.DepartmentName = "NewDepartment";
                newDepartment.DepartmentSiteID = SiteContext.CurrentSiteID;

                // Saves the department to the database
                DepartmentInfoProvider.SetDepartmentInfo(newDepartment);
            }


            /// <heading>Updating a department</heading>
            private void GetAndUpdateDepartment()
            {
                // Gets the department
                DepartmentInfo updateDepartment = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);
                if (updateDepartment != null)
                {
                    // Updates the department properties
                    updateDepartment.DepartmentDisplayName = updateDepartment.DepartmentDisplayName.ToLower();

                    // Saves the changes to the database
                    DepartmentInfoProvider.SetDepartmentInfo(updateDepartment);
                }
            }


            /// <heading>Updating multiple departments</heading>
            private void GetAndBulkUpdateDepartments()
            {
                // Gets all departments whose code name starts with 'NewDepartment'
                var departments = DepartmentInfoProvider.GetDepartments().WhereStartsWith("DepartmentName", "NewDepartment");

                // Loops through the departments
                foreach (DepartmentInfo modifyDepartment in departments)
                {
                    // Updates the department properties
                    modifyDepartment.DepartmentDisplayName = modifyDepartment.DepartmentDisplayName.ToUpper();

                    // Saves the changes to the database
                    DepartmentInfoProvider.SetDepartmentInfo(modifyDepartment);
                }
            }


            /// <heading>Deleting a department</heading>
            private void DeleteDepartment()
            {
                // Gets the department
                DepartmentInfo deleteDepartment = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                if (deleteDepartment != null)
                {
                    // Deletes the department
                    DepartmentInfoProvider.DeleteDepartmentInfo(deleteDepartment);
                }
            }
        }


        /// <summary>
        /// Holds department tax class API examples.
        /// </summary>
        /// <groupHeading>Department tax classes</groupHeading>
        private class DepartmentTaxClass
        {
            /// <heading>Applying tax classes to a department</heading>
            private void AddTaxClassToDepartment()
            {
                // Gets the department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                if ((department != null) && (taxClass != null))
                {
                    // Adds the tax class to the department
                    department.DepartmentDefaultTaxClassID = taxClass.TaxClassID;

                    // Saves the changes to the database
                    DepartmentInfoProvider.SetDepartmentInfo(department);
                }
            }


            /// <heading>Removing tax classes from a department</heading>
            private void RemoveTaxClassFromDepartment()
            {
                // Gets the department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                if (department != null)
                {
                    // Removes the tax class to the department
                    department.DepartmentDefaultTaxClassID = 0;

                    // Saves the changes to the database
                    DepartmentInfoProvider.SetDepartmentInfo(department);
                }
            }
        }
    }
}
