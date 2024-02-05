using System;

using CMS.Base;
using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Globalization;

namespace APIExamples
{
    /// <summary>
    /// Holds tax class API examples.
    /// </summary>
    /// <pageTitle>Tax classes</pageTitle>
    internal class TaxClassesMain
    {
        /// <summary>
        /// Holds tax class API examples.
        /// </summary>
        /// <groupHeading>Tax classes</groupHeading>
        private class TaxClasses
        {
            /// <heading>Creating a new tax class</heading>
            private void CreateTaxClass()
            {
                // Creates a new tax class object
                TaxClassInfo newClass = new TaxClassInfo();

                // Sets the tax class properties
                newClass.TaxClassDisplayName = "New class";
                newClass.TaxClassName = "NewClass";
                newClass.TaxClassSiteID = SiteContext.CurrentSiteID;

                // Saves the tax class to the database
                TaxClassInfoProvider.SetTaxClassInfo(newClass);
            }
            

            /// <heading>Updating a tax class</heading>
            private void GetAndUpdateTaxClass()
            {
                // Gets the tax class
                TaxClassInfo updateClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);
                if (updateClass != null)
                {
                    // Updates the tax class properties
                    updateClass.TaxClassDisplayName = updateClass.TaxClassDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    TaxClassInfoProvider.SetTaxClassInfo(updateClass);
                }
            }


            /// <heading>Updating multiple tax classes</heading>
            private void GetAndBulkUpdateTaxClasses()
            {
                // Gets all tax classes whose code name starts with 'NewClass'
                var classes = TaxClassInfoProvider.GetTaxClasses().WhereStartsWith("TaxClassName", "NewClass");

                // Loops through the tax classes
                foreach (TaxClassInfo modifyClass in classes)
                {
                    // Updates the tax class properties
                    modifyClass.TaxClassDisplayName = modifyClass.TaxClassDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    TaxClassInfoProvider.SetTaxClassInfo(modifyClass);
                }
            }


            /// <heading>Assigning tax classes to products and shipping options</heading>
            private void AssignTaxClassToObjects()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets a department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                // Gets all products in the specified department
                var products = SKUInfoProvider.GetSKUs().WhereEquals("SKUDepartmentID", department.DepartmentID);

                // Gets a shipping option
                ShippingOptionInfo shippingOption = ShippingOptionInfoProvider.GetShippingOptionInfo("NewOption", SiteContext.CurrentSiteName);

                if (taxClass != null)
                {
                    if (department != null)
                    {
                        // Assigns the tax class as the default class of the specified department
                        department.DepartmentDefaultTaxClassID = taxClass.TaxClassID;
                        DepartmentInfoProvider.SetDepartmentInfo(department);
                    }

                    // Assigns the tax class to the specified products
                    foreach (SKUInfo product in products)
                    {
                        product.SKUTaxClassID = taxClass.TaxClassID;
                        SKUInfoProvider.SetSKUInfo(product);
                    }

                    if (shippingOption != null)
                    {
                        // Assigns the tax class to the specified shipping option
                        shippingOption.ShippingOptionTaxClassID = taxClass.TaxClassID;
                        ShippingOptionInfoProvider.SetShippingOptionInfo(shippingOption);
                    }
                }
            }


            /// <heading>Deleting a tax class</heading>
            private void DeleteTaxClass()
            {
                // Gets the tax class
                TaxClassInfo deleteClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                if (deleteClass != null)
                {
                    // Deletes the tax class
                    TaxClassInfoProvider.DeleteTaxClassInfo(deleteClass);
                }
            }
        }


        /// <summary>
        /// Holds API examples for managing tax class values for countries.
        /// </summary>
        /// <groupHeading>Tax values for countries</groupHeading>
        private class TaxClassesInCountries
        {
            /// <heading>Setting a country tax value for a tax class</heading>
            private void SetTaxClassValueInCountry()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the country
                CountryInfo country = CountryInfoProvider.GetCountryInfo("USA");

                if ((taxClass != null) && (country != null))
                {
                    // Creates a new object representing the tax class-country relationship
                    TaxClassCountryInfo newTaxCountry = new TaxClassCountryInfo();

                    // Sets the tax value for the given country within the tax class
                    newTaxCountry.TaxClassID = taxClass.TaxClassID;
                    newTaxCountry.CountryID = country.CountryID;
                    newTaxCountry.TaxValue = 10;

                    // Saves the changes to the database
                    TaxClassCountryInfoProvider.SetTaxClassCountryInfo(newTaxCountry);
                }
            }


            /// <heading>Updating a country tax value of a tax class</heading>
            private void GetAndUpdateTaxClassValueInCountry()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the country
                CountryInfo country = CountryInfoProvider.GetCountryInfo("USA");

                if ((taxClass != null) && (country != null))
                {
                    // Gets the object representing the tax class-country relationship
                    TaxClassCountryInfo updateTaxCountry = TaxClassCountryInfoProvider.GetTaxClassCountryInfo(country.CountryID, taxClass.TaxClassID);
                    if (updateTaxCountry != null)
                    {
                        // Updates the tax value for the given country in the tax class
                        updateTaxCountry.TaxValue = 20;

                        // Saves the changes to the database
                        TaxClassCountryInfoProvider.SetTaxClassCountryInfo(updateTaxCountry);
                    }
                }
            }


            /// <heading>Updating multiple country tax values of a tax class</heading>
            private void GetAndBulkUpdateTaxClassValuesInCountry()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the country
                CountryInfo country = CountryInfoProvider.GetCountryInfo("USA");

                if ((taxClass != null) && (country != null))
                {                    
                    // Gets the objects representing the relationships between the given country and the tax class
                    var classCountries = TaxClassCountryInfoProvider.GetTaxClassCountries()
                                                                    .WhereEquals("TaxClassID", taxClass.TaxClassID)
                                                                    .WhereEquals("CountryID", country.CountryID);

                    // Loops through the tax class-country relationships
                    foreach (TaxClassCountryInfo modifyClassCountry in classCountries)
                    {
                        // Updates the tax value for the country in the tax class
                        modifyClassCountry.TaxValue = 30;

                        // Saves the changes to the database
                        TaxClassCountryInfoProvider.SetTaxClassCountryInfo(modifyClassCountry);
                    }
                }
            }


            /// <heading>Deleting a country tax value of a tax class</heading>
            private void DeleteTaxClassValueInCountry()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the country
                CountryInfo country = CountryInfoProvider.GetCountryInfo("USA");

                if ((taxClass != null) && (country != null))
                {
                    // Gets the object representing the tax class-country relationship
                    TaxClassCountryInfo deleteTaxCountry = TaxClassCountryInfoProvider.GetTaxClassCountryInfo(country.CountryID, taxClass.TaxClassID);
                    if (deleteTaxCountry != null)
                    {
                        // Deletes the tax value for the country
                        TaxClassCountryInfoProvider.DeleteTaxClassCountryInfo(deleteTaxCountry);
                    }
                }
            }
        }


        /// <summary>
        /// Holds API examples for managing tax class values for states.
        /// </summary>
        /// <groupHeading>Tax values for states</groupHeading>
        private class TaxClassesInStates
        {
            /// <heading>Setting a state tax value for a tax class</heading>
            private void SetTaxClassValueInState()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the state
                StateInfo state = StateInfoProvider.GetStateInfo("Alabama");

                if ((taxClass != null) && (state != null))
                {                    
                    // Creates a new object representing the tax class-state relationship
                    TaxClassStateInfo newTaxState = new TaxClassStateInfo();
                    
                    // Sets the tax value for the given state within the tax class
                    newTaxState.TaxClassID = taxClass.TaxClassID;
                    newTaxState.StateID = state.StateID;
                    newTaxState.TaxValue = 10;

                    // Saves the changes to the database
                    TaxClassStateInfoProvider.SetTaxClassStateInfo(newTaxState);
                }
            }


            /// <heading>Updating a state tax value of a tax class</heading>
            private void GetAndUpdateTaxClassValueInState()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the state
                StateInfo state = StateInfoProvider.GetStateInfo("Alabama");

                if ((taxClass != null) && (state != null))
                {                    
                    // Gets the object representing the tax class-state relationship
                    TaxClassStateInfo updateTaxState = TaxClassStateInfoProvider.GetTaxClassStateInfo(taxClass.TaxClassID, state.StateID);
                    if (updateTaxState != null)
                    {                        
                        // Updates the tax value for the given state in the tax class
                        updateTaxState.TaxValue = 20;

                        // Saves the changes to the database
                        TaxClassStateInfoProvider.SetTaxClassStateInfo(updateTaxState);
                    }
                }
            }


            /// <heading>Updating multiple state tax values of a tax class</heading>
            private void GetAndBulkUpdateTaxClassValuesInState()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the state
                StateInfo state = StateInfoProvider.GetStateInfo("Alabama");

                if ((taxClass != null) && (state != null))
                {                    
                    // Gets the objects representing the relationships between the given state and the tax class
                    var classStates = TaxClassStateInfoProvider.GetTaxClassStates()
                                                                .WhereEquals("TaxClassID", taxClass.TaxClassID)
                                                                .WhereEquals("StateID", state.StateID);
                    
                    // Loops through the tax class-state relationships
                    foreach (TaxClassStateInfo modifyClassState in classStates)
                    {
                        // Updates the tax value for the state in the tax class
                        modifyClassState.TaxValue = 30;

                        // Saves the changes to the database
                        TaxClassStateInfoProvider.SetTaxClassStateInfo(modifyClassState);
                    }
                }
            }


            /// <heading>Deleting a state tax value of a tax class</heading>
            private void DeleteTaxClassValueInState()
            {
                // Gets the tax class
                TaxClassInfo taxClass = TaxClassInfoProvider.GetTaxClassInfo("NewClass", SiteContext.CurrentSiteName);

                // Gets the state
                StateInfo state = StateInfoProvider.GetStateInfo("Alabama");

                if ((taxClass != null) && (state != null))
                {
                    // Gets the object representing the tax class-state relationship
                    TaxClassStateInfo deleteTaxState = TaxClassStateInfoProvider.GetTaxClassStateInfo(taxClass.TaxClassID, state.StateID);
                    if (deleteTaxState != null)
                    {
                        // Deletes the tax value for the state
                        TaxClassStateInfoProvider.DeleteTaxClassStateInfo(deleteTaxState);
                    }
                }
            }
        }
    }
}
