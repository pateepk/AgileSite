using System;

using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.Membership;
using CMS.Localization;
using CMS.Helpers;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds product API examples.
    /// </summary>
    /// <pageTitle>Products</pageTitle>
    internal class ProductsMain
    {
        /// <summary>
        /// Holds product API examples.
        /// </summary>
        /// <groupHeading>Products (SKUs)</groupHeading>
        private class Products
        {
            /// <heading>Creating a product</heading>
            private void CreateProduct()
            {
                // Gets a department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                // Creates a new product object
                SKUInfo newProduct = new SKUInfo();

                // Sets the product properties
                newProduct.SKUName = "NewProduct";
                newProduct.SKUPrice = 120;
                newProduct.SKUEnabled = true;
                if (department != null)
                {
                    newProduct.SKUDepartmentID = department.DepartmentID;
                }
                newProduct.SKUSiteID = SiteContext.CurrentSiteID;

                // Saves the product to the database
                // Note: Only creates the SKU object. You also need to create a connected product page to add the product to the site.
                SKUInfoProvider.SetSKUInfo(newProduct);
            }


            /// <heading>Updating a product</heading>
            private void GetAndUpdateProduct()
            {
                // Gets the product
                SKUInfo updateProduct = SKUInfoProvider.GetSKUs()
                                                    .WhereEquals("SKUName", "NewProduct")
                                                    .WhereNull("SKUOptionCategoryID")
                                                    .FirstObject;

                if (updateProduct != null)
                {
                    // Updates the product properties
                    updateProduct.SKUName = updateProduct.SKUName.ToLowerCSafe();

                    // Saves the changes to the database
                    SKUInfoProvider.SetSKUInfo(updateProduct);
                }
            }


            /// <heading>Updating multiple products</heading>
            private void GetAndBulkUpdateProducts()
            {
                // Gets all products whose name starts with 'New'
                var products = SKUInfoProvider.GetSKUs()
                                                .WhereStartsWith("SKUName", "New")
                                                .WhereNull("SKUOptionCategoryID");

                // Loops through the products
                foreach (SKUInfo modifyProduct in products)
                {
                    // Updates the product properties
                    modifyProduct.SKUName = modifyProduct.SKUName.ToUpperCSafe();

                    // Saves the changes to the database
                    SKUInfoProvider.SetSKUInfo(modifyProduct);
                }
            }


            /// <heading>Deleting a product</heading>
            private void DeleteProduct()
            {
                // Gets the product
                SKUInfo deleteProduct = SKUInfoProvider.GetSKUs()
                                                    .WhereEquals("SKUName", "NewProduct")
                                                    .WhereNull("SKUOptionCategoryID")
                                                    .FirstObject;

                if (deleteProduct != null)
                {
                    // Deletes the product
                    SKUInfoProvider.DeleteSKUInfo(deleteProduct);
                }
            }
        }


        /// <summary>
        /// Holds product page API examples.
        /// </summary>
        /// <groupHeading>Product pages</groupHeading>
        private class ProductPages
        {
            /// <heading>Creating a product page</heading>
            private void CreateProductDocument()
            {
                // Gets the product (SKU)
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                .WhereEquals("SKUName", "NewProduct")
                                                .FirstObject;

                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the parent page
                TreeNode parent = tree.SelectNodes(SystemDocumentTypes.MenuItem)
                    .Path("/Products")
                    .OnCurrentSite()
                    .FirstObject;

                if ((parent != null) && (product != null))
                {
                    // Creates a new product page of the 'CMS.Product' type
                    SKUTreeNode node = (SKUTreeNode)TreeNode.New("CMS.Product", tree);

                    // Sets the product page properties
                    node.DocumentCulture = LocalizationContext.PreferredCultureCode;
                    var name = "Product page";
                    node.DocumentName = name;
                    // Synchronize SKU name with document name in a default culture
                    node.DocumentSKUName = name;

                    // Sets a value for a field of the given product page type
                    node.SetValue("ProductColor", "Blue");

                    // Assigns the product to the page
                    node.NodeSKUID = product.SKUID;

                    // Saves the product page to the database
                    node.Insert(parent);
                }
            }


            /// <heading>Updating a product page</heading>
            private void GetAndUpdateProductDocument()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the product page
                SKUTreeNode node = (SKUTreeNode)tree.SelectNodes()
                    .Path("/Products/NewProduct")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (node != null)
                {
                    // Updates the product page properties
                    node.DocumentSKUDescription = "Product was updated.";
                    node.DocumentName = node.DocumentName.ToLowerCSafe();

                    // Saves the product page to the database
                    DocumentHelper.UpdateDocument(node, tree);
                }
            }


            /// <heading>Deleting a product page</heading>
            private void DeleteProductDocument()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the product page
                TreeNode node = tree.SelectNodes()
                    .Path("/Products/NewProduct")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (node != null)
                {
                    // Deletes the product page
                    DocumentHelper.DeleteDocument(node, tree, true, true);
                }
            }
        }


        /// <summary>
        /// Holds membership product API examples.
        /// </summary>
        /// <groupHeading>Membership products</groupHeading>
        private class MembershipProducts
        {
            /// <heading>Creating a membership product</heading>
            private void CreateMembershipProduct()
            {
                // Gets the name of the current site
                string siteName = SiteContext.CurrentSiteName;

                // Gets a department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", siteName);

                // Gets a membership
                MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("NewMembership", siteName);

                // Creates a membership if "NewMembership" does not exist
                if (membership == null)
                {
                    // Creates a membership object and sets its properties
                    membership = new MembershipInfo
                    {
                        MembershipDisplayName = "New Membership",
                        MembershipName = "NewMembership",
                        MembershipSiteID = SiteContext.CurrentSiteID
                    };

                    // Saves the membership
                    MembershipInfoProvider.SetMembershipInfo(membership);
                }

                // Creates a new product object
                SKUInfo newProduct = new SKUInfo();

                // Sets the product properties (e.g. marks the product as a membership)
                if (department != null)
                {
                    newProduct.SKUDepartmentID = department.DepartmentID;
                }
                newProduct.SKUName = "NewMembershipProduct";
                newProduct.SKUPrice = 69;
                newProduct.SKUEnabled = true;
                newProduct.SKUSiteID = SiteContext.CurrentSiteID;
                newProduct.SKUProductType = SKUProductTypeEnum.Membership;
                newProduct.SKUMembershipGUID = membership.MembershipGUID;
                newProduct.SKUValidity = ValidityEnum.Months;
                newProduct.SKUValidFor = 3;

                // Creates the product
                SKUInfoProvider.SetSKUInfo(newProduct);
            }


            /// <heading>Deleting membership products</heading>
            private void DeleteMembershipProduct()
            {
                // Gets all membership products whose name starts with 'NewMembership'
                var products = SKUInfoProvider.GetSKUs()
                                               .WhereStartsWith("SKUName", "NewMembership")
                                               .WhereEquals("SKUProductType", SKUProductTypeEnum.Membership);

                // Loops through the membership products
                foreach (SKUInfo deleteProduct in products)
                {
                    // Deletes the membership product
                    SKUInfoProvider.DeleteSKUInfo(deleteProduct);
                }

                // Gets the membership
                MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);

                if (membership != null)
                {
                    // Deletes the membership
                    MembershipInfoProvider.DeleteMembershipInfo(membership);
                }
            }
        }


        /// <summary>
        /// Holds e-product API examples.
        /// </summary>
        /// <groupHeading>E-products</groupHeading>
        private class EProducts
        {
            /// <heading>Creating an e-product</heading>
            private void CreateEProduct()
            {
                // Gets a department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                // Creates a new product object
                SKUInfo newProduct = new SKUInfo();

                // Sets the product properties (e.g. marks the product as an e-product)
                if (department != null)
                {
                    newProduct.SKUDepartmentID = department.DepartmentID;
                }
                newProduct.SKUName = "NewEProduct";
                newProduct.SKUPrice = 169;
                newProduct.SKUEnabled = true;
                newProduct.SKUSiteID = SiteContext.CurrentSiteID;
                newProduct.SKUProductType = SKUProductTypeEnum.EProduct;
                newProduct.SKUValidity = ValidityEnum.Until;
                newProduct.SKUValidUntil = DateTime.Today.AddDays(10d);

                // Saves the product to the database
                SKUInfoProvider.SetSKUInfo(newProduct);

                // Prepares a path of a file to be uploaded as the e-product file
                string eProductFile = System.Web.HttpContext.Current.Server.MapPath("files/file.png");

                // Creates a metafile object storing the e-product file
                MetaFileInfo metafile = new MetaFileInfo(eProductFile, newProduct.SKUID, "ecommerce.sku", "E-product");

                // Saves the metafile to the database
                MetaFileInfoProvider.SetMetaFileInfo(metafile);

                // Creates an object representing the e-product's assigned downloadable file (SKUFile)
                SKUFileInfo skuFile = new SKUFileInfo();

                // Sets the SKUFile properties
                skuFile.FileSKUID = newProduct.SKUID;
                skuFile.FilePath = "~/getmetafile/" + metafile.MetaFileGUID.ToString().ToLowerCSafe() + "/" + metafile.MetaFileName + ".aspx";
                skuFile.FileType = "MetaFile";
                skuFile.FileName = metafile.MetaFileName;
                skuFile.FileMetaFileGUID = metafile.MetaFileGUID;

                // Saves the SKUFile to the database
                SKUFileInfoProvider.SetSKUFileInfo(skuFile);
            }


            /// <heading>Deleting e-products</heading>
            private void DeleteEProduct()
            {
                // Gets all e-products whose name starts with 'New'
                var products = SKUInfoProvider.GetSKUs()
                                                .WhereStartsWith("SKUName", "New")
                                                .WhereEquals("SKUProductType", SKUProductTypeEnum.EProduct);

                // Loops through the e-products
                foreach (SKUInfo deleteEProduct in products)
                {
                    // Gets the objects representing the files assigned to the e-product (SKUFiles)
                    var deleteFiles = SKUFileInfoProvider.GetSKUFiles().WhereEquals("FileSKUID", deleteEProduct.SKUID);

                    // Loops through the SKUFiles
                    foreach (SKUFileInfo deleteSKUFile in deleteFiles)
                    {
                        // Deletes the SKUFile
                        SKUFileInfoProvider.DeleteSKUFileInfo(deleteSKUFile);
                    }

                    // Deletes the metafiles assigned to the e-product
                    MetaFileInfoProvider.DeleteFiles(deleteEProduct.SKUID, "ecommerce.sku");

                    // Deletes the e-product
                    SKUInfoProvider.DeleteSKUInfo(deleteEProduct);
                }
            }
        }


        /// <summary>
        /// Holds bundle product API examples.
        /// </summary>
        /// <groupHeading>Bundle products</groupHeading>
        private class BundleProducts
        {
            /// <heading>Creating a bundle</heading>
            private void CreateBundle()
            {
                // Gets a set of products to be included in the bundle
                var products = SKUInfoProvider.GetSKUs().WhereEquals("SKUName", "New");

                if (products.Count > 0)
                {
                    // Gets a department
                    DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                    // Creates a new product object
                    SKUInfo newBundle = new SKUInfo();
                    
                    // Sets the product object properties (e.g. marks the product as a bundle)
                    newBundle.SKUName = "NewBundleProduct";
                    newBundle.SKUPrice = 50;
                    newBundle.SKUEnabled = true;
                    newBundle.SKUSiteID = SiteContext.CurrentSiteID;
                    newBundle.SKUProductType = SKUProductTypeEnum.Bundle;
                    newBundle.SKUBundleInventoryType = BundleInventoryTypeEnum.RemoveBundle;
                    if (department != null)
                    {
                        newBundle.SKUDepartmentID = department.DepartmentID;

                    }

                    // Saves the bundle to the database
                    SKUInfoProvider.SetSKUInfo(newBundle);

                    // Loops through the products and adds them to the bundle
                    foreach (SKUInfo product in products)
                    {                        
                        BundleInfoProvider.AddSKUToBundle(newBundle.SKUID, product.SKUID);
                    }
                }
            }


            /// <heading>Deleting a bundle</heading>
            private void DeleteBundle()
            {
                // Gets the bundle product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereEquals("SKUName", "NewBundleProduct")
                                               .WhereEquals("SKUProductType", SKUProductTypeEnum.Bundle)
                                               .WhereNull("SKUOptionCategoryID")
                                               .FirstObject;
                if (product != null)
                {
                    // Gets the relationships between the bundle and its products
                    var bundleProductBindings = BundleInfoProvider.GetBundles().WhereEquals("BundleID", product.SKUID);
                    
                    // Loops through the bundle-product relationships
                    foreach (BundleInfo bundleProduct in bundleProductBindings)
                    {
                        // Removes the product from the bundle
                        BundleInfoProvider.DeleteBundleInfo(bundleProduct);
                    }

                    // Deletes the bundle product
                    SKUInfoProvider.DeleteSKUInfo(product);
                }
            }
        }
    }
}
