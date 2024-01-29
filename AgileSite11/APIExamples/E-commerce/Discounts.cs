using System.Linq;

using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds discount API examples.
    /// </summary>
    /// <pageTitle>Discounts</pageTitle>
    internal class Discounts
    {
        /// <summary>
        /// Holds Buy X Get Y discount API examples.
        /// </summary>
        /// <groupHeading>Buy X Get Y discounts</groupHeading>
        private class BxgyDiscounts
        {
            /// <heading>Creating a Buy X Get Y discount</heading>
            private void CreateBxgyDiscount()
            {
                // Gets a product for a 2+1 free Buy X Get Y discount
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereStartsWith("SKUName", "New")
                                               .FirstObject;

                if (product != null)
                {
                    // Creates a new Buy X Get Y discount object and sets its properties
                    MultiBuyDiscountInfo newDiscount = new MultiBuyDiscountInfo()
                    {
                        MultiBuyDiscountName = "MyDiscount",
                        MultiBuyDiscountDisplayName = "My Discount",
                        MultiBuyDiscountApplyFurtherDiscounts = true,
                        MultiBuyDiscountMinimumBuyCount = 2,
                        MultiBuyDiscountApplyToSKUID = product.SKUID,
                        MultiBuyDiscountIsFlat = false,
                        MultiBuyDiscountValue = 100,
                        MultiBuyDiscountAutoAddEnabled = true,
                        MultiBuyDiscountEnabled = true,
                        MultiBuyDiscountCustomerRestriction = DiscountCustomerEnum.All,
                        MultiBuyDiscountSiteID = SiteContext.CurrentSiteID
                    };

                    // Saves the Buy X Get Y discount to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(newDiscount);

                    // Creates a new relationship of a product that needs to be bought and the discount
                    MultiBuyDiscountSKUInfo newSkuDiscount = new MultiBuyDiscountSKUInfo();

                    // Sets the product-discount relationship properties
                    newSkuDiscount.MultiBuyDiscountID = newDiscount.MultiBuyDiscountID;
                    newSkuDiscount.SKUID = product.SKUID;

                    // Saves the product-discount relationship to the database
                    MultiBuyDiscountSKUInfoProvider.SetMultiBuyDiscountSKUInfo(newSkuDiscount);
                }
            }


            /// <heading>Updating a Buy X Get Y discount</heading>
            private void GetAndUpdateBxgyDiscount()
            {
                // Gets the first Buy X Get Y discount that contains 'MyDiscount' on the current site
                MultiBuyDiscountInfo discount = MultiBuyDiscountInfoProvider.GetMultiBuyDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "MyDiscount")
                                                                            .FirstObject;

                if (discount != null)
                {
                    // Updates the Buy X Get Y discount properties
                    discount.MultiBuyDiscountMinimumBuyCount = 3;

                    // Saves the changes to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(discount);
                }
            }


            /// <heading>Updating multiple Buy X Get Y discounts</heading>
            private void GetAndBulkUpdateBxgyDiscounts()
            {
                // Gets all enabled Buy X Get Y discounts on the current site
                var discounts = MultiBuyDiscountInfoProvider.GetMultiBuyDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereTrue("MultiBuyDiscountEnabled");

                // Loops through the Buy X Get Y discounts
                foreach (MultiBuyDiscountInfo discount in discounts)
                {
                    // Updates the Buy X Get Y discounts properties
                    discount.MultiBuyDiscountEnabled = false;

                    // Saves the changes to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(discount);
                }
            }


            /// <heading>Adding coupon codes to a Buy X Get Y discount</heading>
            private void AddCouponsToBxgyDiscount()
            {
                // Gets the first Buy X Get Y discount on the current site whose name contains 'MyDiscount'
                MultiBuyDiscountInfo discount = MultiBuyDiscountInfoProvider.GetMultiBuyDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "MyDiscount")
                                                                            .FirstObject;

                if (discount != null)
                {
                    // Enables using of coupon codes for the Buy X Get Y discount
                    discount.MultiBuyDiscountUsesCoupons = true;

                    // Saves the change to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(discount);

                    // Prepares a query that gets all existing coupon codes from the current site
                    var existingCouponCodeQuery = ECommerceHelper.GetAllCouponCodesQuery(SiteContext.CurrentSiteID);

                    // Creates a cache of coupon codes on the current site
                    var existingCodes = existingCouponCodeQuery.GetListResult<string>();

                    // Prepares an instance of a class that checks against existing coupon codes to avoid duplicates
                    var coudeUniquenessChecker = new CodeUniquenessChecker(existingCodes);

                    // Initializes a coupon code generator
                    RandomCodeGenerator codeGenerator = new RandomCodeGenerator(coudeUniquenessChecker, "**********");

                    // Loops to generate 300 coupon codes
                    for (int i = 0; i < 300; i++)
                    {
                        // Generates a new unique code text
                        string code = codeGenerator.GenerateCode();
                        
                        // Creates a coupon code and adds it to the discount
                        MultiBuyCouponCodeInfoProvider.CreateCoupon(discount, code, 1);
                    }
                }
            }


            /// <heading>Deleting a Buy X Get Y discount</heading>
            private void DeleteBxgyDiscount()
            {
                // Gets the first Buy X Get Y discount that contains 'MyDiscount' on the current site
                MultiBuyDiscountInfo discount = MultiBuyDiscountInfoProvider.GetMultiBuyDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "MyDiscount")
                                                                            .FirstObject;

                if (discount != null)
                {
                    // Deletes the Buy X Get Y discount
                    MultiBuyDiscountInfoProvider.DeleteMultiBuyDiscountInfo(discount);
                }
            }
        }


        /// <summary>
        /// Holds catalog discount API examples.
        /// </summary>
        /// <groupHeading>Catalog discounts</groupHeading>
        private class CatalogDiscounts
        {
            /// <heading>Creating a catalog discount</heading>
            private void CreateCatalogDiscount()
            {
                // Creates a new catalog discount object and sets its properties
                DiscountInfo newDiscount = new DiscountInfo()
                {
                    DiscountName = "MyDiscount",
                    DiscountDisplayName = "My Discount",
                    DiscountIsFlat = false,
                    DiscountValue = 20,
                    DiscountApplyTo = DiscountApplicationEnum.Products,
                    DiscountProductCondition = @"{% SKU.SKUName.Contains(""new"") %}",
                    DiscountEnabled = true,
                    DiscountOrder = 1,
                    DiscountApplyFurtherDiscounts = true,
                    DiscountUsesCoupons = false,
                    DiscountSiteID = SiteContext.CurrentSiteID
                };

                // Saves the catalog discount to the database
                DiscountInfoProvider.SetDiscountInfo(newDiscount);
            }


            /// <heading>Updating a catalog discount</heading>
            private void GetAndUpdateCatalogDiscount()
            {
                // Gets the first catalog discount that contains 'MyDiscount' on the current site
                DiscountInfo discount = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereContains("DiscountName", "MyDiscount")
                                                            .WhereContains("DiscountApplyTo", DiscountApplicationEnum.Products.ToString())
                                                            .FirstObject;

                if (discount != null)
                {
                    // Updates the catalog discount properties
                    discount.DiscountEnabled = false;

                    // Saves the changes to the database
                    DiscountInfoProvider.SetDiscountInfo(discount);
                }
            }


            /// <heading>Updating multiple catalog discounts</heading>
            private void GetAndBulkUpdateCatalogDiscounts()
            {
                // Gets all enabled catalog discounts on the current site
                var discounts = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID, true)
                                                    .WhereContains("DiscountApplyTo", DiscountApplicationEnum.Products.ToString());

                // Loops through the catalog discounts
                foreach (DiscountInfo discount in discounts)
                {
                    // Updates the catalog discount properties
                    discount.DiscountEnabled = false;

                    // Saves the changes to the database
                    DiscountInfoProvider.SetDiscountInfo(discount);
                }
            }


            /// <heading>Deleting a catalog discount</heading>
            private void DeleteCatalogDiscount()
            {
                // Gets the first catalog discount that contains 'MyDiscount' on the current site
                DiscountInfo discount = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereContains("DiscountName", "MyDiscount")
                                                            .WhereContains("DiscountApplyTo", DiscountApplicationEnum.Products.ToString())
                                                            .FirstObject;

                if (discount != null)
                {
                    // Deletes the catalog discount
                    DiscountInfoProvider.DeleteDiscountInfo(discount);
                }
            }
        }


        /// <summary>
        /// Holds order discount API examples.
        /// </summary>
        /// <groupHeading>Order discounts</groupHeading>
        private class OrderDiscounts
        {
            /// <heading>Creating an order discount</heading>
            private void CreateOrderDiscount()
            {
                // Creates a new order discount object and sets its properties
                DiscountInfo newDiscount = new DiscountInfo()
                {
                    DiscountDisplayName = "New order discount",
                    DiscountName = "NewOrderDiscount",
                    DiscountApplyTo = DiscountApplicationEnum.Order,
                    DiscountIsFlat = false,
                    DiscountSiteID = SiteContext.CurrentSiteID,

                    DiscountEnabled = true,
                    DiscountValue = 10,
                    DiscountCartCondition = @"{% Currency.CurrencyName==""Dollar"" %}",
                    DiscountOrder = 1,
                    DiscountApplyFurtherDiscounts = true,
                    DiscountUsesCoupons = false,
                };

                // Saves the order discount to the database
                DiscountInfoProvider.SetDiscountInfo(newDiscount);
            }


            /// <heading>Updating an order discount</heading>
            private void GetAndUpdateOrderDiscount()
            {
                // Gets the first order discount on the current site whose name contains 'NewOrderDiscount'
                DiscountInfo discount = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereContains("DiscountName", "NewOrderDiscount")
                                                            .WhereEquals("DiscountApplyTo", DiscountApplicationEnum.Order.ToString())
                                                            .TopN(1)
                                                            .FirstObject;

                if (discount != null)
                {
                    // Updates the order discount properties
                    discount.DiscountEnabled = false;

                    // Saves the changes to the database
                    DiscountInfoProvider.SetDiscountInfo(discount);
                }
            }


            /// <heading>Updating multiple order discounts</heading>
            private void GetAndBulkUpdateOrderDiscounts()
            {
                // Gets all enabled order discounts on the current site
                var discounts = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID, true)
                                                    .WhereEquals("DiscountApplyTo", DiscountApplicationEnum.Order.ToString())
                                                    .WhereTrue("DiscountEnabled");

                // Loops through the order discounts
                foreach (DiscountInfo discount in discounts)
                {
                    // Updates the order discount properties
                    discount.DiscountItemMinOrderAmount = 100;

                    // Saves the changes to the database
                    DiscountInfoProvider.SetDiscountInfo(discount);
                }
            }


            /// <heading>Deleting an order discount</heading>
            private void DeleteOrderDiscount()
            {
                // Gets the first order discount on the current site whose name contains 'NewOrderDiscount'
                DiscountInfo discount = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereContains("DiscountName", "NewOrderDiscount")
                                                            .WhereEquals("DiscountApplyTo", DiscountApplicationEnum.Order.ToString())
                                                            .TopN(1)
                                                            .FirstObject;

                if (discount != null)
                {
                    // Deletes the order discount
                    DiscountInfoProvider.DeleteDiscountInfo(discount);
                }
            }
        }


        /// <summary>
        /// Holds free shipping offer API examples.
        /// </summary>
        /// <groupHeading>Free shipping offers</groupHeading>
        private class FreeShippingOffers
        {
            /// <heading>Creating a free shipping offer</heading>
            private void CreateFreeShippingOffer()
            {
                // Creates a new free shipping offer object and sets its properties
                DiscountInfo newDiscount = new DiscountInfo()
                {
                    DiscountName = "MyShippingOffer",
                    DiscountDisplayName = "My Shipping Offer",
                    DiscountIsFlat = false,
                    DiscountValue = 20,
                    DiscountApplyTo = DiscountApplicationEnum.Shipping,
                    DiscountOrderAmount = 100,
                    DiscountEnabled = true,
                    DiscountOrder = 1,
                    DiscountApplyFurtherDiscounts = true,
                    DiscountUsesCoupons = false,
                    DiscountSiteID = SiteContext.CurrentSiteID
                };

                // Saves the free shipping offer to the database
                DiscountInfoProvider.SetDiscountInfo(newDiscount);
            }


            /// <heading>Updating a free shipping offer</heading>
            private void GetAndUpdateFreeShippingOffer()
            {
                // Gets the first free shipping offer that contains 'MyShippingOffer' on the current site
                DiscountInfo discount = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereContains("DiscountName", "MyShippingOffer")
                                                            .WhereContains("DiscountApplyTo", DiscountApplicationEnum.Shipping.ToString())
                                                            .FirstObject;

                if (discount != null)
                {
                    // Updates the free shipping offer properties
                    discount.DiscountEnabled = false;

                    // Saves the changes to the database
                    DiscountInfoProvider.SetDiscountInfo(discount);
                }
            }


            /// <heading>Updating multiple free shipping offer</heading>
            private void GetAndBulkUpdateFreeShippingOffers()
            {
                // Gets all enabled free shipping offers on the current site
                var discounts = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID, true)
                                                    .WhereContains("DiscountApplyTo", DiscountApplicationEnum.Shipping.ToString());

                // Loops through the free shipping offers
                foreach (DiscountInfo discount in discounts)
                {
                    // Updates the free shipping offer properties
                    discount.DiscountEnabled = false;

                    // Saves the changes to the database
                    DiscountInfoProvider.SetDiscountInfo(discount);
                }
            }


            /// <heading>Deleting a free shipping offer</heading>
            private void DeleteFreeShippingOffer()
            {
                // Gets the first free shipping offer that contains 'MyShippingOffer' on the current site
                DiscountInfo discount = DiscountInfoProvider.GetDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereContains("DiscountName", "MyShippingOffer")
                                                            .WhereContains("DiscountApplyTo", DiscountApplicationEnum.Shipping.ToString())
                                                            .FirstObject;

                if (discount != null)
                {
                    // Deletes the free shipping offer
                    DiscountInfoProvider.DeleteDiscountInfo(discount);
                }
            }
        }


        /// <summary>
        /// Holds volume discount API examples.
        /// </summary>
        /// <groupHeading>Volume discounts</groupHeading>
        private class VolumeDiscounts
        {
            /// <heading>Creating a volume discount</heading>
            private void CreateVolumeDiscount()
            {
                // Gets a product for the volume discount
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                 .WhereStartsWith("SKUName", "New")
                                                 .FirstObject;

                if (product != null)
                {
                    // Creates a new volume discount object
                    VolumeDiscountInfo newDiscount = new VolumeDiscountInfo();

                    // Sets the volume discount properties
                    newDiscount.VolumeDiscountMinCount = 100;
                    newDiscount.VolumeDiscountValue = 20;
                    newDiscount.VolumeDiscountSKUID = product.SKUID;
                    newDiscount.VolumeDiscountIsFlatValue = false;

                    // Saves the volume discount to the database
                    VolumeDiscountInfoProvider.SetVolumeDiscountInfo(newDiscount);
                }
            }


            /// <heading>Updating a volume discount</heading>
            private void GetAndUpdateVolumeDiscount()
            {
                // Gets a product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                .WhereEquals("SKUName", "NewProduct")
                                                .FirstObject;

                if (product != null)
                {
                    // Gets the first volume discount defined for the product
                    VolumeDiscountInfo discount = VolumeDiscountInfoProvider.GetVolumeDiscounts(product.SKUID).FirstObject;
                    if (discount != null)
                    {
                        // Updates the volume discount properties
                        discount.VolumeDiscountMinCount = 800;

                        // Saves the changes to the database
                        VolumeDiscountInfoProvider.SetVolumeDiscountInfo(discount);
                    }
                }
            }


            /// <heading>Updating multiple volume discounts</heading>
            private void GetAndBulkUpdateVolumeDiscounts()
            {
                // Gets a product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereStartsWith("SKUName", "New")
                                               .FirstObject;

                if (product != null)
                {
                    // Gets the product's volume discounts
                    var discounts = VolumeDiscountInfoProvider.GetVolumeDiscounts(product.SKUID);

                    // Loops through the volume discounts
                    foreach (VolumeDiscountInfo discount in discounts)
                    {
                        // Updates the volume discount properties
                        discount.VolumeDiscountMinCount = 500;

                        // Saves the changes to the database
                        VolumeDiscountInfoProvider.SetVolumeDiscountInfo(discount);
                    }
                }
            }


            /// <heading>Deleting a volume discount</heading>
            private void DeleteVolumeDiscount()
            {
                // Gets a product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereStartsWith("SKUName", "New")
                                               .FirstObject;

                if (product != null)
                {
                    // Gets the first volume discount defined for the product
                    VolumeDiscountInfo discount = VolumeDiscountInfoProvider.GetVolumeDiscounts(product.SKUID).FirstObject;
                    if (discount != null)
                    {
                        // Deletes the volume discount
                        VolumeDiscountInfoProvider.DeleteVolumeDiscountInfo(discount);
                    }
                }
            }
        }


        /// <summary>
        /// Holds product coupon API examples.
        /// </summary>
        /// <groupHeading>Product coupons</groupHeading>
        private class ProductCoupons
        {
            /// <heading>Creating a product coupon</heading>
            private void CreateProductCoupon()
            {
                // Gets a product for the product coupon
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereStartsWith("SKUName", "New")
                                               .FirstObject;

                if (product != null)
                {
                    // Creates a new Product coupon object and sets its properties
                    MultiBuyDiscountInfo newProductCoupon = new MultiBuyDiscountInfo()
                    {
                        MultiBuyDiscountName = "NewProductCoupon",
                        MultiBuyDiscountDisplayName = "New product coupon",
                        MultiBuyDiscountEnabled = true,
                        MultiBuyDiscountSiteID = SiteContext.CurrentSiteID,

                        // Configures the object as a Product coupon (not a Buy X Get Y discount)
                        MultiBuyDiscountIsProductCoupon = true,
                        MultiBuyDiscountUsesCoupons = true,
                        MultiBuyDiscountApplyFurtherDiscounts = true,
                        MultiBuyDiscountMinimumBuyCount = 1,
                        MultiBuyDiscountAutoAddEnabled = false,

                        // Sets the coupon's discount value to a fixed amount of 10
                        MultiBuyDiscountIsFlat = true,
                        MultiBuyDiscountValue = 10,

                        // Makes the coupon available for all store visitors
                        MultiBuyDiscountCustomerRestriction = DiscountCustomerEnum.All,
                    };

                    // Saves the product coupon to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(newProductCoupon);

                    // Configures the coupon to apply to the retrieved product
                    MultiBuyDiscountSKUInfo couponSkuBinding = new MultiBuyDiscountSKUInfo();
                    couponSkuBinding.MultiBuyDiscountID = newProductCoupon.MultiBuyDiscountID;
                    couponSkuBinding.SKUID = product.SKUID;

                    // Saves the coupon-product relationship to the database
                    MultiBuyDiscountSKUInfoProvider.SetMultiBuyDiscountSKUInfo(couponSkuBinding);
                }
            }


            /// <heading>Adding coupon codes to a product coupon</heading>
            private void AddCodesToProductCoupon()
            {
                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                if (productCoupon != null)
                {
                    // Prepares a query that gets all existing coupon codes from the current site
                    var existingCouponCodeQuery = ECommerceHelper.GetAllCouponCodesQuery(SiteContext.CurrentSiteID);

                    // Creates a cache of coupon codes on the current site
                    var existingCodes = existingCouponCodeQuery.GetListResult<string>();

                    // Prepares an instance of a class that checks against existing coupon codes to avoid duplicates
                    var coudeUniquenessChecker = new CodeUniquenessChecker(existingCodes);

                    // Initializes a coupon code generator
                    RandomCodeGenerator codeGenerator = new RandomCodeGenerator(coudeUniquenessChecker, "**********");

                    // Loops to generate 100 coupon codes
                    for (int i = 0; i < 100; i++)
                    {
                        // Generates a new unique code text
                        string code = codeGenerator.GenerateCode();

                        // Creates a coupon code and adds it to the product coupon
                        MultiBuyCouponCodeInfoProvider.CreateCoupon(productCoupon, code, 1);
                    }
                }
            }


            /// <heading>Updating a product coupon</heading>
            private void GetAndUpdateProductCoupon()
            {
                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")                                                                            
                                                                            .FirstObject;

                if (productCoupon != null)
                {
                    // Updates the product coupon properties
                    productCoupon.MultiBuyDiscountValue = 20;

                    // Saves the changes to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(productCoupon);
                }
            }


            /// <heading>Updating multiple product coupons</heading>
            private void GetAndBulkUpdateProductCoupons()
            {
                // Gets all enabled product coupons on the current site
                var productCoupons = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                            .WhereTrue("MultiBuyDiscountEnabled");

                // Loops through the product coupons
                foreach (MultiBuyDiscountInfo coupon in productCoupons)
                {
                    // Updates the product coupon properties (disables the coupon)
                    coupon.MultiBuyDiscountEnabled = false;

                    // Saves the changes to the database
                    MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(coupon);
                }
            }


            /// <heading>Removing products from a product coupon</heading>
            private void RemoveProductFromCoupon()
            {
                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                // Gets all coupon-product relationships for the product coupon
                var couponSkuBindings = MultiBuyDiscountSKUInfoProvider.GetMultiBuyDiscountSKUs()
                                                                .WhereEquals("MultiBuyDiscountID", productCoupon.MultiBuyDiscountID);

                // Loops through the retrieved coupon-product relationships
                foreach (MultiBuyDiscountSKUInfo couponSkuBinding in couponSkuBindings)
                {
                    // Removes the product from the product coupon
                    MultiBuyDiscountSKUInfoProvider.DeleteMultiBuyDiscountSKUInfo(couponSkuBinding);
                }
            }


            /// <heading>Applying a product coupon to product sections</heading>
            private void AddSectionToProductCoupon()
            {
                // NOTE: Remove any assigned products, departments, brands or collections from the product coupon before assigning sections

                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                // Gets the starting path of the current site's product section
                string productStartingPath = ECommerceSettings.ProductsStartingPath(SiteContext.CurrentSiteID);

                // Gets a list of all page type names that represent product sections
                var productSectionTypes = CMS.DataEngine.DataClassInfoProvider.GetClasses()
                                                                                    .WhereTrue("ClassIsProductSection")
                                                                                    .Column("ClassName")
                                                                                    .GetListResult<string>();
                
                // Prepares an array of the product section page type names
                // Requires a 'using System.Linq;' statement                
                string[] sectionTypeNames = productSectionTypes.ToArray();

                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets all product section pages on the first level under the product section root
                var productSections = tree.SelectNodes()
                                            .Types(sectionTypeNames)
                                            .OnCurrentSite()
                                            .Path(productStartingPath, PathTypeEnum.Children)
                                            .NestingLevel(1);

                foreach (TreeNode section in productSections)
                {
                    // Creates a relationship between the product coupon and product section
                    // Tip: To add excluded sections to the product coupon, set the method's 'included' parameter to false
                    MultiBuyDiscountTreeInfoProvider.AddMultiBuyDiscountToTree(productCoupon.MultiBuyDiscountID, section.NodeID, included: true);
                }

                // Clears the product coupon's cache record
                CMS.Helpers.CacheHelper.TouchKey(MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON + "|byid|" + productCoupon.MultiBuyDiscountID);
            }


            /// <heading>Applying a product coupon to departments</heading>
            private void AddDepartmentToProductCoupon()
            {
                // NOTE: Remove any assigned products, sections, brands or collections from the product coupon before assigning departments

                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                // Gets all departments on the current site
                var departments = DepartmentInfoProvider.GetDepartments(SiteContext.CurrentSiteID);

                // Loops through the departments
                foreach (DepartmentInfo department in departments)
                {
                    // Creates a relationship between the product coupon and department
                    MultiBuyDiscountDepartmentInfoProvider.AddMultiBuyDiscountToDepartment(productCoupon.MultiBuyDiscountID, department.DepartmentID);
                }

                // Clears the product coupon's cache record
                CMS.Helpers.CacheHelper.TouchKey(MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON + "|byid|" + productCoupon.MultiBuyDiscountID);
            }


            /// <heading>Applying a product coupon to brands</heading>
            private void AddBrandsToProductCoupon()
            {
                // NOTE: Remove any assigned products, sections, departments or collections from the product coupon before assigning brands

                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                // Gets all brands on the current site
                var brands = BrandInfoProvider.GetBrands().OnSite(SiteContext.CurrentSiteID);

                // Loops through the brands
                foreach (BrandInfo brand in brands)
                {
                    // Creates a relationship between the product coupon and brand
                    // Tip: To add excluded brands to the product coupon, set the method's 'included' parameter to false
                    MultiBuyDiscountBrandInfoProvider.AddMultiBuyDiscountToBrand(productCoupon.MultiBuyDiscountID, brand.BrandID, included: true);
                }

                // Clears the product coupon's cache record
                CMS.Helpers.CacheHelper.TouchKey(MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON + "|byid|" + productCoupon.MultiBuyDiscountID);
            }


            /// <heading>Applying a product coupon to collections</heading>
            private void AddCollectionsToProductCoupon()
            {
                // NOTE: Remove any assigned products, sections, departments or brands from the product coupon before assigning collections

                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                // Gets all collections on the current site
                var collections = CollectionInfoProvider.GetCollections().OnSite(SiteContext.CurrentSiteID);

                // Loops through the collections
                foreach (CollectionInfo collection in collections)
                {
                    // Creates a relationship between the product coupon and collection
                    // Tip: To add excluded collections to the product coupon, set the method's 'included' parameter to false
                    MultiBuyDiscountCollectionInfoProvider.AddMultiBuyDiscountToCollection(productCoupon.MultiBuyDiscountID, collection.CollectionID, included: true);
                }

                // Clears the product coupon's cache record
                CMS.Helpers.CacheHelper.TouchKey(MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON + "|byid|" + productCoupon.MultiBuyDiscountID);
            }


            /// <heading>Deleting a product coupon</heading>
            private void DeleteProductCoupon()
            {
                // Gets the first product coupon on the current site whose name contains 'NewProductCoupon'
                MultiBuyDiscountInfo productCoupon = MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(SiteContext.CurrentSiteID)
                                                                            .WhereContains("MultiBuyDiscountName", "NewProductCoupon")
                                                                            .FirstObject;

                if (productCoupon != null)
                {
                    // Deletes the product coupon
                    MultiBuyDiscountInfoProvider.DeleteMultiBuyDiscountInfo(productCoupon);
                }
            }
        }
    }
}
