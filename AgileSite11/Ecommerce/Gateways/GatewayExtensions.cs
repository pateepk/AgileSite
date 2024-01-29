using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Extension methods usable in gateway implementations.
    /// </summary>
    internal static class GatewayExtensions
    {
        /// <summary>
        /// Returns given <paramref name="dateTime"/> in RFC 3339 compliant format.
        /// </summary>
        /// <remarks>
        /// For more info see https://tools.ietf.org/html/rfc3339#section-5.6.
        /// </remarks>
        public static string ToRfcDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ", DateTimeFormatInfo.InvariantInfo);
        }


        /// <summary>
        /// Returns null if given <paramref name="value"/> is null or <see cref="string.Empty"/>.
        /// </summary>
        public static string ToNullIfEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value;
        }


        /// <summary>
        /// Resets the <see cref="PaymentResultItemInfo.Value" /> and <see cref="PaymentResultItemInfo.Text" /> property values.
        /// </summary>
        public static void Reset(this PaymentResultItemInfo itemInfo)
        {
            if (itemInfo == null)
            {
                return;
            }

            itemInfo.Value = null;
            itemInfo.Text = null;
        }


        /// <summary>
        /// Returns discount names contained in <paramref name="summary"/> formatted for external gateways.
        /// </summary>
        public static string GetFormattedDiscountNames(this ValuesSummary summary)
        {
            var builder = new StringBuilder();

            if (summary != null)
            {
                foreach (var summaryItem in summary)
                {
                    builder.AppendLine(summaryItem.Name);
                }
            }

            return builder.ToString();
        }


        /// <summary>
        /// Returns sum of particular values in given <paramref name="valuesSummary"/>.
        /// </summary>
        public static decimal GetTotalValue(this ValuesSummary valuesSummary)
        {
            if (valuesSummary == null)
            {
                return decimal.Zero;
            }

            return valuesSummary.Sum(i => i.Value);
        }
    }
}
