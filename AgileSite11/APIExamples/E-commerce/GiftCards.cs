using CMS.Ecommerce;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds gift card API examples.
    /// </summary>
    /// <pageTitle>Gift cards</pageTitle>
    internal class GiftCards
    {
        /// <heading>Creating a gift card</heading>
        private void CreateGiftCard()
        {
            // Creates a new gift card object and sets its properties
            var newGiftCard = new GiftCardInfo()
            {
                GiftCardDisplayName = "New gift card",
                GiftCardName = "NewGiftCard",
                GiftCardEnabled = true,
                GiftCardSiteID = SiteContext.CurrentSiteID,
                GiftCardValue = 10,
                GiftCardMinimumOrderPrice = 50,
                GiftCardCartCondition = @"{% Currency.CurrencyName==""Dollar"" %}",
            };

            // Saves the gift card to the database
            GiftCardInfoProvider.SetGiftCardInfo(newGiftCard);
        }


        /// <heading>Adding coupon codes to a gift card</heading>
        private void AddCouponCodes()
        {
            // Gets the first gift card on the current site whose name starts with 'NewGift'
            GiftCardInfo giftCard = GiftCardInfoProvider.GetGiftCards().OnSite(SiteContext.CurrentSiteID)
                                                            .WhereStartsWith("GiftCardName", "NewGift")
                                                            .TopN(1)
                                                            .FirstObject;

            if (giftCard != null)
            {
                // Creates a coupon code and adds it to the gift card (the code is "GIFT-10")
                GiftCardCouponCodeInfoProvider.CreateGiftCardCoupon(giftCard, "GIFT-10");
            }
        }


        /// <heading>Generating coupon codes for a gift card</heading>
        private void GenerateCouponCodes()
        {
            // Gets the first gift card on the current site whose name starts with 'NewGift'
            GiftCardInfo giftCard = GiftCardInfoProvider.GetGiftCards().OnSite(SiteContext.CurrentSiteID)
                                                            .WhereStartsWith("GiftCardName", "NewGift")
                                                            .TopN(1)
                                                            .FirstObject;

            if (giftCard != null)
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

                    // Creates a coupon code and adds it to the gift card
                    GiftCardCouponCodeInfoProvider.CreateGiftCardCoupon(giftCard, code);
                }
            }
        }


        /// <heading>Updating gift cards</heading>
        private void UpdateGiftCards()
        {
            // Gets all enabled gift cards on the current site
            var giftCards = GiftCardInfoProvider.GetGiftCards().OnSite(SiteContext.CurrentSiteID)
                                                    .WhereTrue("GiftCardEnabled");

            // Loops through the gift cards
            foreach (GiftCardInfo giftCard in giftCards)
            {
                // Updates the gift card properties (sets the minimum order price to 100)
                giftCard.GiftCardMinimumOrderPrice = 100;

                // Saves the changes to the database
                GiftCardInfoProvider.SetGiftCardInfo(giftCard);
            }
        }


        /// <heading>Setting the remaining value for gift card coupon codes</heading>
        private void SetGiftCardCouponCodeRemainingValue()
        {
            // Gets the 'Gift-10' gift card coupon code (coupon codes are unique within the context of each site)
            GiftCardCouponCodeInfo giftCardCouponCode = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCodeInfo("Gift-10", SiteContext.CurrentSiteID);

            // Sets the remaining value to '5' for the coupon code
            giftCardCouponCode.GiftCardCouponCodeRemainingValue = 5m;

            // Saves the updated coupon code to the database
            giftCardCouponCode.Update();
        }


        /// <heading>Deleting a gift card</heading>
        private void DeleteGiftCard()
        {
            // Gets the first gift card on the current site whose name starts with 'NewGift'
            GiftCardInfo giftCard = GiftCardInfoProvider.GetGiftCards().OnSite(SiteContext.CurrentSiteID)
                                                            .WhereStartsWith("GiftCardName", "NewGift")
                                                            .TopN(1)
                                                            .FirstObject;

            if (giftCard != null)
            {
                // Deletes the gift card
                GiftCardInfoProvider.DeleteGiftCardInfo(giftCard);
            }
        }
    }
}
