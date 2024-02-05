using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing DepartmentInfo management.
    /// </summary>
    public class DepartmentInfoProvider : AbstractInfoProvider<DepartmentInfo, DepartmentInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DepartmentInfoProvider()
            : base(DepartmentInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all departments.
        /// </summary>
        public static ObjectQuery<DepartmentInfo> GetDepartments()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns department with specified ID.
        /// </summary>
        /// <param name="departmentId">Department ID</param>        
        public static DepartmentInfo GetDepartmentInfo(int departmentId)
        {
            return ProviderObject.GetInfoById(departmentId);
        }


        /// <summary>
        /// Returns department with specified name.
        /// </summary>
        /// <param name="departmentName">Department name</param>                
        /// <param name="siteName">Site name</param>                
        public static DepartmentInfo GetDepartmentInfo(string departmentName, string siteName)
        {
            return ProviderObject.GetDepartmentInfoInternal(departmentName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified department.
        /// </summary>
        /// <param name="departmentObj">Department to be set</param>
        public static void SetDepartmentInfo(DepartmentInfo departmentObj)
        {
            ProviderObject.SetInfo(departmentObj);
        }


        /// <summary>
        /// Deletes specified department.
        /// </summary>
        /// <param name="departmentObj">Department to be deleted</param>
        public static void DeleteDepartmentInfo(DepartmentInfo departmentObj)
        {
            ProviderObject.DeleteInfo(departmentObj);
        }


        /// <summary>
        /// Deletes department with specified ID.
        /// </summary>
        /// <param name="departmentId">Department ID</param>
        public static void DeleteDepartmentInfo(int departmentId)
        {
            var departmentObj = GetDepartmentInfo(departmentId);
            DeleteDepartmentInfo(departmentObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns the query for all departments on specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        public static ObjectQuery<DepartmentInfo> GetDepartments(int siteId)
        {
            return ProviderObject.GetDepartmentsInternal(siteId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns department with specified name.
        /// </summary>
        /// <param name="departmentName">Department name</param>                
        /// <param name="siteName">Site name</param>         
        protected virtual DepartmentInfo GetDepartmentInfoInternal(string departmentName, string siteName)
        {
            // Search for global department if site department not found
            bool searchGlobal = ECommerceSettings.AllowGlobalDepartments(siteName);

            return GetInfoByCodeName(departmentName, SiteInfoProvider.GetSiteID(siteName), true, searchGlobal);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DepartmentInfo info)
        {
            // Ensure null value - in case DepartmentDefaultTaxClassID is set to zero or negative value using SetValue
            info.DepartmentDefaultTaxClassID = info.DepartmentDefaultTaxClassID;

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns the query for all departments on specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        protected virtual ObjectQuery<DepartmentInfo> GetDepartmentsInternal(int siteId)
        {
            // Check if site uses site or global departments
            var includeGlobal = ECommerceSettings.AllowGlobalDepartments(siteId);

            // Get departments on requested site
            return GetDepartments().OnSite(siteId, includeGlobal);
        }

        #endregion
    }
}