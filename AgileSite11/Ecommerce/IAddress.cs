using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Address interface used to encapsulate OrderAddressInfo and Customer's AddressInfo
    /// </summary>
    public interface IAddress : ISimpleDataContainer
    {
        #region "Properties"

        /// <summary>
        /// Address ZIP code.
        /// </summary>
        string AddressZip
        {
            get;
            set;
        }


        /// <summary>
        /// Address state ID.
        /// </summary>
        int AddressStateID
        {
            get;
            set;
        }


        /// <summary>
        /// Address phone.
        /// </summary>
        string AddressPhone
        {
            get;
            set;
        }


        /// <summary>
        /// Address country ID.
        /// </summary>
        int AddressCountryID
        {
            get;
            set;
        }


        /// <summary>
        /// Address ID.
        /// </summary>
        int AddressID
        {
            get;
            set;
        }


        /// <summary>
        /// Address personal name.
        /// </summary>
        string AddressPersonalName
        {
            get;
            set;
        }


        /// <summary>
        /// Address line 1.
        /// </summary>
        string AddressLine1
        {
            get;
            set;
        }


        /// <summary>
        /// Address line 2.
        /// </summary>
        string AddressLine2
        {
            get;
            set;
        }


        /// <summary>
        /// Address city.
        /// </summary>
        string AddressCity
        {
            get;
            set;
        }


        /// <summary>
        /// Address GUID.
        /// </summary>
        Guid AddressGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Date and time when the address was last modified.
        /// </summary>
        DateTime AddressLastModified
        {
            get;
            set;
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the address object using appropriate provider.
        /// </summary>
        void DeleteAddress();


        /// <summary>
        /// Updates the address object using appropriate provider.
        /// </summary>
        void SetAddress();

        #endregion
    }
}
