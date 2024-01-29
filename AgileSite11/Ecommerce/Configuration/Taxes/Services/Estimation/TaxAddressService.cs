using System;

using CMS.Globalization;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a service providing an address for the tax calculation based on e-commerce module settings.
    /// </summary>
    /// <seealso cref="ITaxAddressServiceFactory"/>
    public class TaxAddressService : ITaxAddressService
    {
        /// <summary>
        /// ID of the site for which this instance was created.
        /// </summary>
        protected int SiteID
        {
            get;
        }


        /// <summary>
        /// Indicates whether a shipping address is preferred on the site for which this instance was created.
        /// </summary>
        protected bool SitePrefersShippingAddress => ECommerceSettings.ApplyTaxesBasedOn(SiteID) == ApplyTaxBasedOnEnum.ShippingAddress;


        /// <summary>
        /// Initializes a new instance of the <see cref="TaxAddressService"/> reflecting settings of the specified site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        public TaxAddressService(int siteId)
        {
            SiteID = siteId;
        }


        /// <summary>
        /// Returns an address which should be used for the tax calculation. 
        /// </summary>
        /// <remarks>
        /// <para>This method picks one of the supplied addresses according to the 'CMSStoreApplyTaxBasedOn' ecommerce setting (<see cref="ECommerceSettings.ApplyTaxesBasedOn"/>).
        /// The default address is returned when both <paramref name="billingAddress"/> and <paramref name="shippingAddress"/> are <c>null</c>. Returns <c>null</c> when the
        /// <see cref="ECommerceSettings.DefaultCountryName"/> settings is empty.
        /// </para>
        /// <para>
        /// The <paramref name="taxClass"/> and <paramref name="customer"/> are ignored by default, but you can override this method and leverage these parameters to customize
        /// the decision logic. Use the <see cref="GetDefaultAddress"/> method to get the site's default address or the <see cref="CreateAddress"/> method to create a 
        /// completely new address (e.g. address of your store).
        /// </para>
        /// </remarks>
        /// <param name="billingAddress">A billing address.</param>
        /// <param name="shippingAddress">An address where the goods are delivered.</param>
        /// <param name="taxClass">A tax class to get an address for.</param>
        /// <param name="customer">A customer making a purchase.</param>
        public virtual IAddress GetTaxAddress(IAddress billingAddress, IAddress shippingAddress, TaxClassInfo taxClass, CustomerInfo customer)
        {
            IAddress result;

            if (SitePrefersShippingAddress)
            {
                result = shippingAddress ?? billingAddress;
            }
            else
            {
                result = billingAddress ?? shippingAddress;
            }

            return result ?? GetDefaultAddress();
        }


        /// <summary>
        /// Returns the default address or <c>null</c> when can not be determined.
        /// </summary>
        /// <remarks>
        /// You can override this method to customize the way how the default address is obtained. Use the <see cref="SiteID"/> to get the ID of the site for which
        /// this instance was created.
        /// </remarks>
        protected virtual IAddress GetDefaultAddress()
        {
            var country = CountryInfoProvider.GetCountryInfo(ECommerceSettings.DefaultCountryName(SiteID));

            return country == null ? null : CreateAddress(country.CountryID);
        }


        /// <summary>
        /// Creates a new address according to the given parameters.
        /// </summary>
        /// <param name="countryId">An ID of the country.</param>
        /// <param name="stateId">An ID of the state.</param>
        /// <param name="city">A city name.</param>
        /// <param name="zip">A ZIP code.</param>
        /// <param name="line1">A first line of an address.</param>
        /// <param name="line2">A second line of an address.</param>
        protected IAddress CreateAddress(int countryId, int stateId = 0, string city = "", string zip = "", string line1 = "", string line2 = "")
        {
            return new Address
                   {
                       AddressCountryID = countryId,
                       AddressStateID = stateId,
                       AddressCity = city,
                       AddressZip = zip,
                       AddressLine1 = line1,
                       AddressLine2 = line2
                   };
        }


        private class Address : IAddress
        {
            public string AddressZip
            {
                get; set;
            }


            public int AddressStateID
            {
                get; set;
            }


            public string AddressPhone
            {
                get; set;
            }


            public int AddressCountryID
            {
                get; set;
            }


            public int AddressID
            {
                get; set;
            }


            public string AddressPersonalName
            {
                get; set;
            }


            public string AddressLine1
            {
                get; set;
            }


            public string AddressLine2
            {
                get; set;
            }


            public string AddressCity
            {
                get; set;
            }


            public Guid AddressGUID
            {
                get; set;
            }


            public DateTime AddressLastModified
            {
                get; set;
            }


            public void DeleteAddress()
            {
            }


            public void SetAddress()
            {
            }


            public object this[string columnName]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }


            public object GetValue(string columnName)
            {
                throw new NotImplementedException();
            }


            public bool SetValue(string columnName, object value)
            {
                throw new NotImplementedException();
            }
        }
    }
}