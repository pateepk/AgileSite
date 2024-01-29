using System;

using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples of product-related objects.
    /// </summary>
    /// <pageTitle>Product-related objects</pageTitle>
    internal class ProductRelatedObjects
    {
        /// <summary>
        /// Holds public status API examples.
        /// </summary>
        /// <groupHeading>Public statuses</groupHeading>
        private class PublicStatuses
        {
            /// <heading>Creating a public status</heading>
            private void CreatePublicStatus()
            {
                // Creates a new public status object
                PublicStatusInfo newStatus = new PublicStatusInfo();

                // Sets the public status properties
                newStatus.PublicStatusDisplayName = "New status";
                newStatus.PublicStatusName = "NewStatus";
                newStatus.PublicStatusEnabled = true;
                newStatus.PublicStatusSiteID = SiteContext.CurrentSiteID;

                // Saves the public status to the database
                PublicStatusInfoProvider.SetPublicStatusInfo(newStatus);
            }


            /// <heading>Updating a public status</heading>
            private void GetAndUpdatePublicStatus()
            {
                // Gets the public status
                PublicStatusInfo updateStatus = PublicStatusInfoProvider.GetPublicStatusInfo("NewStatus", SiteContext.CurrentSiteName);
                if (updateStatus != null)
                {
                    // Updates the public status properties
                    updateStatus.PublicStatusDisplayName = updateStatus.PublicStatusDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    PublicStatusInfoProvider.SetPublicStatusInfo(updateStatus);
                }
            }


            /// <heading>Updating multiple public statuses</heading>
            private void GetAndBulkUpdatePublicStatuses()
            {
                // Gets all public statuses on the current site whose name starts with 'New'
                var statuses = PublicStatusInfoProvider.GetPublicStatuses()
                                                       .OnSite(SiteContext.CurrentSiteID)
                                                       .WhereStartsWith("PublicStatusName", "New");

                // Loops through the public statuses
                foreach (PublicStatusInfo modifyStatus in statuses)
                {
                    // Updates the public status properties
                    modifyStatus.PublicStatusDisplayName = modifyStatus.PublicStatusDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    PublicStatusInfoProvider.SetPublicStatusInfo(modifyStatus);
                }
            }


            /// <heading>Deleting a public status</heading>
            private void DeletePublicStatus()
            {
                // Gets the public status
                PublicStatusInfo deleteStatus = PublicStatusInfoProvider.GetPublicStatusInfo("NewStatus", SiteContext.CurrentSiteName);

                if (deleteStatus != null)
                {
                    // Deletes the public status
                    PublicStatusInfoProvider.DeletePublicStatusInfo(deleteStatus);
                }
            }
        }


        /// <summary>
        /// Holds internal status API examples.
        /// </summary>
        /// <groupHeading>Internal statuses</groupHeading>
        private class InternalStatuses
        {
            /// <heading>Creating an internal status</heading>
            private void CreateInternalStatus()
            {
                // Creates a new internal status object
                InternalStatusInfo newStatus = new InternalStatusInfo();

                // Sets the internal status properties
                newStatus.InternalStatusDisplayName = "New internal status";
                newStatus.InternalStatusName = "NewInternalStatus";
                newStatus.InternalStatusEnabled = true;
                newStatus.InternalStatusSiteID = SiteContext.CurrentSiteID;

                // Saves the internal status to the database
                InternalStatusInfoProvider.SetInternalStatusInfo(newStatus);
            }


            /// <heading>Updating an internal status</heading>
            private void GetAndUpdateInternalStatus()
            {
                // Gets the internal status
                InternalStatusInfo updateStatus = InternalStatusInfoProvider.GetInternalStatusInfo("NewInternalStatus", SiteContext.CurrentSiteName);
                if (updateStatus != null)
                {
                    // Updates the internal status properties
                    updateStatus.InternalStatusDisplayName = updateStatus.InternalStatusDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    InternalStatusInfoProvider.SetInternalStatusInfo(updateStatus);
                }
            }


            /// <heading>Updating multiple internal statuses</heading>
            private void GetAndBulkUpdateInternalStatuses()
            {
                // Gets all internal statuses on the current site whose name starts with 'New'
                var statuses = InternalStatusInfoProvider.GetInternalStatuses()
                                                           .OnSite(SiteContext.CurrentSiteID)
                                                           .WhereStartsWith("InternalStatusName", "New");

                // Loops through the internal statuses
                foreach (InternalStatusInfo modifyStatus in statuses)
                {
                    // Updates the internal status properties
                    modifyStatus.InternalStatusDisplayName = modifyStatus.InternalStatusDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    InternalStatusInfoProvider.SetInternalStatusInfo(modifyStatus);
                }
            }


            /// <heading>Deleting an internal status</heading>
            private void DeleteInternalStatus()
            {
                // Gets the internal status
                InternalStatusInfo deleteStatus = InternalStatusInfoProvider.GetInternalStatusInfo("NewInternalStatus", SiteContext.CurrentSiteName);

                if (deleteStatus != null)
                {
                    // Deletes the internal status
                    InternalStatusInfoProvider.DeleteInternalStatusInfo(deleteStatus);
                }
            }
        }


        /// <summary>
        /// Holds manufacturer API examples.
        /// </summary>
        /// <groupHeading>Manufacturers</groupHeading>
        private class Manufacturers
        {
            /// <heading>Creating a manufacturer</heading>
            private void CreateManufacturer()
            {
                // Creates a new manufacturer object
                ManufacturerInfo newManufacturer = new ManufacturerInfo();

                // Sets the manufacturer properties
                newManufacturer.ManufacturerDisplayName = "New manufacturer";
                newManufacturer.ManufacturerName = "NewManufacturer";
                newManufacturer.ManufacturerHomepage = "www.newmanufacturer.com";
                newManufacturer.ManufacturerSiteID = SiteContext.CurrentSiteID;
                newManufacturer.ManufacturerEnabled = true;

                // Saves the manufacturer to the database
                ManufacturerInfoProvider.SetManufacturerInfo(newManufacturer);
            }


            /// <heading>Updating a manufacturer</heading>
            private void GetAndUpdateManufacturer()
            {
                // Gets the manufacturer
                ManufacturerInfo updateManufacturer = ManufacturerInfoProvider.GetManufacturerInfo("NewManufacturer", SiteContext.CurrentSiteName);
                if (updateManufacturer != null)
                {
                    // Updates the manufacturer properties
                    updateManufacturer.ManufacturerDisplayName = updateManufacturer.ManufacturerDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    ManufacturerInfoProvider.SetManufacturerInfo(updateManufacturer);
                }
            }


            /// <heading>Updating multiple manufacturers</heading>
            private void GetAndBulkUpdateManufacturers()
            {
                // Gets all manufacturers on the current site whose name starts with 'New'
                var manufacturers = ManufacturerInfoProvider.GetManufacturers()
                                                             .OnSite(SiteContext.CurrentSiteID)
                                                             .WhereStartsWith("ManufacturerName", "New");

                // Loops through the manufacturers
                foreach (ManufacturerInfo modifyManufacturer in manufacturers)
                {
                    // Updates the manufacturer properties
                    modifyManufacturer.ManufacturerDisplayName = modifyManufacturer.ManufacturerDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    ManufacturerInfoProvider.SetManufacturerInfo(modifyManufacturer);
                }
            }


            /// <heading>Deleting a manufacturer</heading>
            private void DeleteManufacturer()
            {
                // Gets the manufacturer
                ManufacturerInfo deleteManufacturer = ManufacturerInfoProvider.GetManufacturerInfo("NewManufacturer", SiteContext.CurrentSiteName);

                if (deleteManufacturer != null)
                {
                    // Deletes the manufacturer
                    ManufacturerInfoProvider.DeleteManufacturerInfo(deleteManufacturer);
                }
            }
        }


        /// <summary>
        /// Holds supplier API examples.
        /// </summary>
        /// <groupHeading>Suppliers</groupHeading>
        private class Suppliers
        {
            /// <heading>Creating a supplier</heading>
            private void CreateSupplier()
            {
                // Creates a new supplier object
                SupplierInfo newSupplier = new SupplierInfo();

                // Sets the supplier properties
                newSupplier.SupplierDisplayName = "New supplier";
                newSupplier.SupplierName = "NewSupplier";
                newSupplier.SupplierEmail = "newsupplier@supplier.com";
                newSupplier.SupplierSiteID = SiteContext.CurrentSiteID;
                newSupplier.SupplierPhone = "123456789";
                newSupplier.SupplierFax = "987654321";
                newSupplier.SupplierEnabled = true;

                // Saves the supplier to the database
                SupplierInfoProvider.SetSupplierInfo(newSupplier);
            }


            /// <heading>Updating a supplier</heading>
            private void GetAndUpdateSupplier()
            {
                // Gets the supplier
                SupplierInfo updateSupplier = SupplierInfoProvider.GetSupplierInfo("NewSupplier", SiteContext.CurrentSiteName);
                if (updateSupplier != null)
                {
                    // Updates the supplier properties
                    updateSupplier.SupplierDisplayName = updateSupplier.SupplierDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    SupplierInfoProvider.SetSupplierInfo(updateSupplier);
                }
            }


            /// <heading>Updating multiple suppliers</heading>
            private void GetAndBulkUpdateSuppliers()
            {
                // Gets all suppliers on the current site whose name starts with 'New'
                var suppliers = SupplierInfoProvider.GetSuppliers()
                                                    .OnSite(SiteContext.CurrentSiteID)
                                                    .WhereStartsWith("SupplierName", "New");

                // Loops through the suppliers
                foreach (SupplierInfo modifySupplier in suppliers)
                {
                    // Updates the supplier properties
                    modifySupplier.SupplierDisplayName = modifySupplier.SupplierDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    SupplierInfoProvider.SetSupplierInfo(modifySupplier);
                }
            }


            /// <heading>Deleting a supplier</heading>
            private void DeleteSupplier()
            {
                // Gets the supplier
                SupplierInfo deleteSupplier = SupplierInfoProvider.GetSupplierInfo("NewSupplier", SiteContext.CurrentSiteName);

                if (deleteSupplier != null)
                {
                    // Deletes the supplier
                    SupplierInfoProvider.DeleteSupplierInfo(deleteSupplier);
                }
            }
        }
    }
}
