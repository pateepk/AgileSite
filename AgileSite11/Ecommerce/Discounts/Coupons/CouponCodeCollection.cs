using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using CMS.IO;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the collection of coupon codes.
    /// </summary>
    [Serializable]
    [XmlRoot("CouponCodes")]
    public sealed class CouponCodeCollection : ICouponCodeCollection
    {
        private List<ICouponCode> mCodes = new List<ICouponCode>();


        /// <summary>
        /// Gets the coupon codes.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<ICouponCode> Codes => mCodes;


        /// <summary>
        /// Gets coupon codes, that are in cart, but not applied.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<ICouponCode> NotAppliedInCartCodes
        {
            get
            {
                return mCodes.Where(i => i.ApplicationStatus == CouponCodeApplicationStatusEnum.NotAppliedInCart);
            }
        }


        /// <summary>
        /// Gets coupon codes that are already applied in cart.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<ICouponCode> CartAppliedCodes
        {
            get
            {
                return mCodes.Where(i => i.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInCart);
            }
        }


        /// <summary>
        /// Gets all applied coupon codes that are already applied in order.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<ICouponCode> OrderAppliedCodes
        {
            get
            {
                return mCodes.Where(i => i.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder);
            }
        }


        /// <summary>
        /// Gets all applied coupon codes.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<ICouponCode> AllAppliedCodes
        {
            get
            {
                return mCodes.Where(i => (i.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInCart) || (i.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder));
            }
        }


        /// <summary>
        /// Gets the coupon codes.
        /// </summary>
        /// <remarks>
        /// Exists due to serialization reasons.
        /// </remarks>
        [XmlElement(ElementName = "CouponCode", IsNullable = false)]
        public CouponCode[] CouponCodes
        {
            get
            {
                return AllAppliedCodes.Select(x => x as CouponCode).ToArray();
            }
            set
            {
                if (value != null)
                {
                    mCodes = value.ToList<ICouponCode>();
                }
            }
        }


        /// <summary>
        /// Adds coupon with given <paramref name="couponCode"/> and <paramref name="status"/> to the collection.
        /// </summary>
        public void Add(string couponCode, CouponCodeApplicationStatusEnum status)
        {
            if (couponCode == null)
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            var existing = GetCouponCode(couponCode);

            // Gift card correction is the only scenario that allows having multiple instances of the same coupon code
            if (existing == null || status == CouponCodeApplicationStatusEnum.GiftCardCorrection)
            {
                mCodes.Add(new CouponCode(couponCode, status));
            }
        }


        /// <summary>
        /// Removes the coupon with given <paramref name="couponCode"/> from collection.
        /// Does nothing when no such coupon exists.
        /// </summary>
        public void Remove(string couponCode)
        {
            if (couponCode == null)
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            var item = GetCouponCode(couponCode);
            if (item != null)
            {
                mCodes.Remove(item);
            }
        }


        /// <summary>
        /// Merge existing coupon codes with given ones.
        /// </summary>
        public void Merge(IEnumerable<ICouponCode> couponCodes)
        {
            if (couponCodes == null)
            {
                return;
            }

            var couponCodesList = couponCodes.ToList();

            RemoveGiftCardCorrections();
            RemoveInvalidCoupons();
            UpdateCurrentlyAppliedCoupons(couponCodesList);
            UpdateNoLongerAppliedCoupons(couponCodesList);
        }


        /// <summary>
        /// Returns true when given codes is present in cart, but it is not applied.
        /// </summary>
        public bool IsNotAppliedInCart(string couponCode)
        {
            if (couponCode == null)
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            var existing = GetCouponCode(couponCode);
            return existing != null && existing.ApplicationStatus == CouponCodeApplicationStatusEnum.NotAppliedInCart;
        }


        /// <summary>
        /// Returns true when given codes is present and was applied in cart.
        /// </summary>
        public bool IsAppliedInCart(string couponCode)
        {
            if (couponCode == null)
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            var existing = GetCouponCode(couponCode);
            return existing != null && existing.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInCart;
        }


        /// <summary>
        /// Returns true when given codes is present and was applied in order.
        /// </summary>
        public bool IsAppliedInOrder(string couponCode)
        {
            if (couponCode == null)
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            var existing = GetCouponCode(couponCode);
            return existing != null && existing.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder;
        }


        /// <summary>
        /// Applies all applicable coupon codes during order creation.
        /// </summary>
        public void Apply()
        {
            foreach (var code in mCodes.Where(x => x.ApplicationStatus == CouponCodeApplicationStatusEnum.GiftCardCorrection))
            {
                code.Apply();

                var existingAppliedInOrder = mCodes.FirstOrDefault(i => i.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder && ECommerceHelper.CouponCodeComparer.Equals(i.Code, code.Code));

                if (existingAppliedInOrder != null)
                {
                    existingAppliedInOrder.ValueInMainCurrency += code.ValueInMainCurrency;
                }
            }


            foreach (var code in mCodes.Where(x => x.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInCart))
            {
                code.Apply();
                code.ApplicationStatus = CouponCodeApplicationStatusEnum.AppliedInOrder;    
            }

            RemoveGiftCardCorrections();
        }


        /// <summary>
        /// Serializes the collection into string.
        /// </summary>
        public string Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(CouponCodeCollection));
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
            var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    xmlSerializer.Serialize(xmlWriter, this, namespaces);
                    return stringWriter.ToString();
                }
            }
        }


        /// <summary>
        /// Returns a new instance of <see cref="CouponCodeCollection"/> deserialized from given string.
        /// </summary>
        public static CouponCodeCollection Deserialize(string serialized)
        {
            try
            {
                var stringReader = new StringReader(serialized);
                var serializer = new XmlSerializer(typeof(CouponCodeCollection));
                return (CouponCodeCollection)serializer.Deserialize(stringReader);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }


        private ICouponCode GetCouponCode(string couponCode, IEnumerable<ICouponCode> source = null)
        {
            var comparer = ECommerceHelper.CouponCodeComparer;
            var couponSource = source ?? mCodes;
            return couponSource.FirstOrDefault(i => comparer.Equals(i.Code, couponCode));
        }


        /// <summary>
        /// Override old coupon codes with appropriate new coupon codes. Coupon codes already applied in order are left intact.
        /// </summary>
        /// <param name="couponCodes">Currently applied coupon codes</param>
        private void UpdateCurrentlyAppliedCoupons(IEnumerable<ICouponCode> couponCodes)
        {
            foreach (var couponCode in couponCodes)
            {
                if (couponCode.ApplicationStatus == CouponCodeApplicationStatusEnum.GiftCardCorrection)
                {
                    mCodes.Add(couponCode);
                    continue;
                }

                var existing = GetCouponCode(couponCode.Code);
                if (existing != null)
                {
                    if (existing.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder)
                    {
                        // Do not override coupons that are already applied in order
                        continue;
                    }
                    mCodes.Remove(existing);
                }

                mCodes.Add(couponCode);
            }
        }


        /// <summary>
        /// Change application status of coupon codes that are present in cart, but ceased to be applied.
        /// </summary>
        /// <param name="couponCodes">Currently applied coupon codes</param>
        private void UpdateNoLongerAppliedCoupons(IEnumerable<ICouponCode> couponCodes)
        {
            foreach (var noLongerAppliedCoupon in mCodes.Where(c => (c.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInCart) && (GetCouponCode(c.Code, couponCodes) == null)))
            {
                noLongerAppliedCoupon.ApplicationStatus = CouponCodeApplicationStatusEnum.NotAppliedInCart;
            }
        }


        /// <summary>
        /// Removes coupon codes that are not valid.
        /// </summary>
        private void RemoveInvalidCoupons()
        {
            mCodes.RemoveAll(x => x.ApplicationStatus == CouponCodeApplicationStatusEnum.Invalid);
        }


        /// <summary>
        /// Removes coupon codes that represent gift card corrections.
        /// </summary>
        private void RemoveGiftCardCorrections()
        {
            mCodes.RemoveAll(x => x.ApplicationStatus == CouponCodeApplicationStatusEnum.GiftCardCorrection);
        }
    }
}