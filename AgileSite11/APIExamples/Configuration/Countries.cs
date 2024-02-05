using System;

using CMS.Globalization;

namespace APIExamples
{
    /// <summary>
    /// Holds country and state API examples.
    /// </summary>
    /// <pageTitle>Countries</pageTitle>
    internal class CountriesMain
    {
        /// <summary>
        /// Holds country API examples.
        /// </summary>
        /// <groupHeading>Countries</groupHeading>
        private class Countries
        {
            /// <heading>Creating a new country</heading>
            private void CreateCountry()
            {
                // Creates a new country object
                CountryInfo newCountry = new CountryInfo();

                // Sets the country properties
                newCountry.CountryDisplayName = "New country";
                newCountry.CountryName = "NewCountry";

                // Saves the new country into the database
                CountryInfoProvider.SetCountryInfo(newCountry);
            }

            
            /// <heading>Updating an existing country</heading>
            private void GetAndUpdateCountry()
            {
                // Gets the country
                CountryInfo updateCountry = CountryInfoProvider.GetCountryInfo("NewCountry");
                if (updateCountry != null)
                {
                    // Updates the country properties
                    updateCountry.CountryDisplayName = updateCountry.CountryDisplayName.ToLower();

                    // Saves the change to the database
                    CountryInfoProvider.SetCountryInfo(updateCountry);
                }
            }

            
            /// <heading>Updating multiple countries</heading>
            private void GetAndBulkUpdateCountries()
            {                
                // Gets all countries whose name starts with 'NewCountry'
                var countries = CountryInfoProvider.GetCountries().WhereStartsWith("CountryName", "NewCountry");
                
                // Loops through individual countries
                foreach (CountryInfo country in countries)
                {                    
                    // Updates the country properties
                    country.CountryDisplayName = country.CountryDisplayName.ToUpper();

                    // Saves the change to the database
                    CountryInfoProvider.SetCountryInfo(country);
                }
            }

            
            /// <heading>Deleting a country</heading>
            private void DeleteCountry()
            {
                // Gets the country
                CountryInfo deleteCountry = CountryInfoProvider.GetCountryInfo("NewCountry");

                if (deleteCountry != null)
                {
                    // Deletes the country
                    CountryInfoProvider.DeleteCountryInfo(deleteCountry);
                }
            }
        }


        /// <summary>
        /// Holds state API examples.
        /// </summary>
        /// <groupHeading>States</groupHeading>
        private class States
        {
            /// <heading>Creating a new state under a country</heading>
            private void CreateState()
            {
                // Gets the country
                CountryInfo country = CountryInfoProvider.GetCountryInfo("NewCountry");
                if (country != null)
                {
                    // Creates a new state object
                    StateInfo newState = new StateInfo();

                    // Sets the state properties
                    newState.StateDisplayName = "New state";
                    newState.StateName = "NewState";
                    newState.CountryID = country.CountryID;

                    // Saves the new state into the database
                    StateInfoProvider.SetStateInfo(newState);
                }
            }
            
            /// <heading>Updating an existing state</heading>
            private void GetAndUpdateState()
            {
                // Gets the state
                StateInfo updateState = StateInfoProvider.GetStateInfo("NewState");
                if (updateState != null)
                {
                    // Updates the state properties
                    updateState.StateDisplayName = updateState.StateDisplayName.ToLower();

                    // Saves the change to the database
                    StateInfoProvider.SetStateInfo(updateState);
                }
            }


            /// <heading>Updating multiple states</heading>
            private void GetAndBulkUpdateStates()
            {
                // Gets the parent country of the states
                CountryInfo country = CountryInfoProvider.GetCountryInfo("NewCountry");
                if (country != null)
                {
                    // Gets a collection of states within the parent country
                    var states = StateInfoProvider.GetStates().WhereEquals("CountryID", country.CountryID);
                    
                    // Loops through individual states
                    foreach (StateInfo state in states)
                    {                        
                        // Updates the state properties
                        state.StateDisplayName = state.StateDisplayName.ToUpper();

                        // Saves the change to the database
                        StateInfoProvider.SetStateInfo(state);
                    }
                }
            }


            /// <heading>Deleting a state</heading>
            private void DeleteState()
            {
                // Gets the state
                StateInfo deleteState = StateInfoProvider.GetStateInfo("NewState");

                if (deleteState != null)
                {
                    // Deletes the state
                    StateInfoProvider.DeleteStateInfo(deleteState);
                }
            }
        }
    }
}
