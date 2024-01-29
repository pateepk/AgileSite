using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default <see cref="IRoundingService"/> implementation.
    /// Round values accordingly to application setting.
    /// </summary>
    internal class DefaultRoundingService : IRoundingService
    {
        private const string ROUNDING_SETTING_NAME = "CMSPriceRounding";
        private const int DEFAULT_ROUNDING_DECIMALS = 2;

        private const int MATHEMATICAL = 0;
        private const int TRUNCATE = 1;
        private const int FINANCIAL = 2;

        private readonly ISettingService mSettingService;


        /// <summary>
        /// Gets rounding setting value transformed to integer.
        /// </summary>
        private int SettingValue => mSettingService.GetIntegerValue(ROUNDING_SETTING_NAME);


        /// <summary>
        /// Creates a new instance of <see cref="DefaultRoundingService"/>.
        /// </summary>
        /// <param name="settingService">Service providing application settings</param>
        public DefaultRoundingService(ISettingService settingService)
        {
            if (settingService == null)
            {
                throw new ArgumentNullException(nameof(settingService));
            }

            mSettingService = settingService;
        }


        /// <summary>
        /// Returns rounded price according to the specified currency.
        /// </summary>
        /// <param name="price">Price to be rounded</param>
        /// <param name="currency">Currency which supplies number of decimal places</param>
        public decimal Round(decimal price, CurrencyInfo currency)
        {
            var decimals = currency?.CurrencyRoundTo ?? DEFAULT_ROUNDING_DECIMALS;

            return Round(price, decimals);
        }


        /// <summary>
        /// Returns rounded price to the specified number of digits.
        /// </summary>
        /// <param name="price">Price to be rounded</param>
        /// <param name="decimals">Number of decimal places</param>
        private decimal Round(decimal price, int decimals)
        {
            switch (SettingValue)
            {
                case MATHEMATICAL:
                    return Math.Round(price, decimals, MidpointRounding.AwayFromZero);

                case TRUNCATE:
                    return TruncateValue(price, decimals);

                case FINANCIAL:
                    return Math.Round(price, decimals, MidpointRounding.ToEven);

                default:
                    return Math.Round(price, decimals);
            }
        }


        private static decimal TruncateValue(decimal price, int decimals)
        {
            var multiplier = Convert.ToDecimal(Math.Pow(10, decimals));
            price = multiplier * price;
            price = Math.Truncate(price);
            price = price / multiplier;

            return price;
        }
    }
}