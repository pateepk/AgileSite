using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class containing string constants of all e-commerce module permissions and static methods for checking permissions.
    /// </summary>
    public class EcommercePermissions
    {
        #region "Permission constants"

        /// <summary>
        /// Constant representing 'EcommerceRead' permission
        /// </summary>
        public const string ECOMMERCE_READ = "EcommerceRead";

        /// <summary>
        /// Constant representing 'EcommerceModify' permission
        /// </summary>
        public const string ECOMMERCE_MODIFY = "EcommerceModify";

        /// <summary>
        /// Constant representing 'EcommerceGlobalModify' permission
        /// </summary>
        public const string ECOMMERCE_MODIFYGLOBAL = "EcommerceGlobalModify";

        /// <summary>
        /// Constant representing 'Destroy' permission
        /// </summary>
        public const string ECOMMERCE_DESTROY = "Destroy";

        /// <summary>
        /// Constant representing 'ConfigurationRead' permission
        /// </summary>
        public const string CONFIGURATION_READ = "ConfigurationRead";

        /// <summary>
        /// Constant representing 'ConfigurationModify' permission
        /// </summary>
        public const string CONFIGURATION_MODIFY = "ConfigurationModify";

        /// <summary>
        /// Constant representing 'ConfigurationGlobalModify' permission
        /// </summary>
        public const string CONFIGURATION_MODIFYGLOBAL = "ConfigurationGlobalModify";

        /// <summary>
        /// Constant representing 'ReadOrders' permission
        /// </summary>
        public const string ORDERS_READ = "ReadOrders";

        /// <summary>
        /// Constant representing 'ModifyOrders' permission
        /// </summary>
        public const string ORDERS_MODIFY = "ModifyOrders";

        /// <summary>
        /// Constant representing 'ReadReports' permission
        /// </summary>
        public const string REPORTS_READ = "ReadReports";

        /// <summary>
        /// Constant representing 'ReadCustomers' permission
        /// </summary>
        public const string CUSTOMERS_READ = "ReadCustomers";

        /// <summary>
        /// Constant representing 'ModifyCustomers' permission
        /// </summary>
        public const string CUSTOMERS_MODIFY = "ModifyCustomers";

        /// <summary>
        /// Constant representing 'ReadProducts' permission
        /// </summary>
        public const string PRODUCTS_READ = "ReadProducts";

        /// <summary>
        /// Constant representing 'ModifyProducts' permission
        /// </summary>
        public const string PRODUCTS_MODIFY = "ModifyProducts";

        /// <summary>
        /// Constant representing 'ReadDiscounts' permission
        /// </summary>
        public const string DISCOUNTS_READ = "ReadDiscounts";

        /// <summary>
        /// Constant representing 'ModifyDiscounts' permission
        /// </summary>
        public const string DISCOUNTS_MODIFY = "ModifyDiscounts";

        /// <summary>
        /// Constant representing 'ReadManufacturers' permission
        /// </summary>
        public const string MANUFACTURERS_READ = "ReadManufacturers";

        /// <summary>
        /// Constant representing 'ModifyManufacturers' permission
        /// </summary>
        public const string MANUFACTURERS_MODIFY = "ModifyManufacturers";

        /// <summary>
        /// Constant representing 'ReadSuppliers' permission
        /// </summary>
        public const string SUPPLIERS_READ = "ReadSuppliers";

        /// <summary>
        /// Constant representing 'ModifySuppliers' permission
        /// </summary>
        public const string SUPPLIERS_MODIFY = "ModifySuppliers";

        #endregion


        #region "Permission methods"

        /// <summary>
        /// Checks the configuration permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails.</param>
        /// <param name="isGlobal"><c>True</c> if the object is global object. <c>False</c> if the object belongs to specific site only.</param>
        /// <param name="checkPermissionsBase">Default method for checking permissions.</param>
        internal static bool CheckConfigurationPermissions(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure, bool isGlobal, Func<PermissionsEnum, string, IUserInfo, bool, bool> checkPermissionsBase)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(CONFIGURATION_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return ECommerceHelper.IsUserAuthorizedToModifyConfiguration(isGlobal, siteName, userInfo, exceptionOnFailure);

                default:
                    return checkPermissionsBase(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Checks the products permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails.</param>
        /// <param name="isGlobal"><c>True</c> if the object is global object. <c>False</c> if the object belongs to specific site only.</param>
        /// <param name="checkPermissionsBase">Default method for checking permissions.</param>
        internal static bool CheckProductsPermissions(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure, bool isGlobal, Func<PermissionsEnum, string, IUserInfo, bool, bool> checkPermissionsBase)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(PRODUCTS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return SKUInfoProvider.IsUserAuthorizedToModifySKU(isGlobal, siteName, userInfo, exceptionOnFailure);

                default:
                    return checkPermissionsBase(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Checks the customers permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <param name="checkPermissionsBase">Default method for checking permissions.</param>
        internal static bool CheckCustomersPermissions(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure, Func<PermissionsEnum, string, IUserInfo, bool, bool> checkPermissionsBase)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(CUSTOMERS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return ECommerceHelper.IsUserAuthorizedForPermission(CUSTOMERS_MODIFY, siteName, userInfo, exceptionOnFailure);

                default:
                    return checkPermissionsBase(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Checks the orders permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <param name="checkPermissionsBase">Default method for checking permissions.</param>
        internal static bool CheckOrdersPermissions(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure, Func<PermissionsEnum, string, IUserInfo, bool, bool> checkPermissionsBase)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(ORDERS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return ECommerceHelper.IsUserAuthorizedForPermission(ORDERS_MODIFY, siteName, userInfo, exceptionOnFailure);

                default:
                    return checkPermissionsBase(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}