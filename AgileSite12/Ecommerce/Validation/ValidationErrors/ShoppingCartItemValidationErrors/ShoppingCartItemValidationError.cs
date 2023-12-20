using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Base class for shopping cart item related errors.
    /// </summary>
    public class ShoppingCartItemValidationError : IValidationError
    {
        /// <summary>
        /// Key which can be used to retrieve a localized error message.
        /// </summary>
        protected string mMessageKey;


        /// <summary>
        /// SKU Id of product associated with the error.
        /// </summary>
        public int SKUId
        {
            get;
        }


        /// <summary>
        /// SKU name of product associated with the error.
        /// </summary>
        public string SKUName { get; set; }


        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey
        {
            get
            {
                return mMessageKey;
            }
        }


        /// <summary>
        /// Returns array with SKU name key.
        /// </summary>
        public virtual object[] MessageParameters => new string[] { SKUName };


        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItemValidationError"/> class.
        /// </summary>
        public ShoppingCartItemValidationError(int skuId, string skuName, string messageKey)
        {
            SKUId = skuId;
            SKUName = skuName;
            mMessageKey = messageKey;
        }
    }
}
