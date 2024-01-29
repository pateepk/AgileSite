using System;
using System.Xml.Serialization;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the coupon code.
    /// </summary>
    [Serializable]
    public class CouponCode : ICouponCode
    {
        private readonly ICouponCodeApplication mApplicationAction;


        /// <summary>
        /// Creates a new instance of <see cref="CouponCode"/>
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public CouponCode()
            : this(null, CouponCodeApplicationStatusEnum.AppliedInOrder, null)
        {
        }


        /// <summary>
        /// Creates a new instance of <see cref="CouponCode"/>
        /// </summary>
        /// <param name="code">Coupon code.</param>
        public CouponCode(string code)
            : this(code, CouponCodeApplicationStatusEnum.AppliedInOrder)
        {
        }


        /// <summary>
        /// Creates a new instance of <see cref="CouponCode"/>
        /// </summary>
        /// <param name="code">Coupon code.</param>
        /// <param name="applicationStatus">Status of coupon code application.</param>
        /// <param name="applicationAction">Action for coupon code application.</param>
        /// <param name="valueInMainCurrency">Coupon code discount value.</param>
        public CouponCode(string code, CouponCodeApplicationStatusEnum applicationStatus, ICouponCodeApplication applicationAction = null, decimal? valueInMainCurrency = null)
        {         
            Code = code;
            ApplicationStatus = applicationStatus;
            mApplicationAction = applicationAction;
            ValueInMainCurrency = valueInMainCurrency;
        }


        /// <summary>
        /// Gets the coupon code.
        /// </summary>
        [XmlElement("Code")]
        public string Code
        {
            get;
            set;
        }


        /// <summary>
        /// The provided discount value in main currency.
        /// </summary>
        [XmlElement("ValueInMainCurrency")]
        public decimal? ValueInMainCurrency
        {
            get;
            set;
        }


        /// <summary>
        /// Do not remove. 
        /// A conventional method that prevents the serialization of nullable decimal property.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeValueInMainCurrency()
        {
            return ValueInMainCurrency.HasValue;
        }


        /// <summary>
        /// Gets the information about coupon application.
        /// </summary>
        [XmlIgnore]
        public CouponCodeApplicationStatusEnum ApplicationStatus
        {
            get;
            set;
        }


        /// <summary>
        /// Applies coupon code during order creation.
        /// </summary>
        public void Apply()
        {
            mApplicationAction?.Apply();
        }
    }
}