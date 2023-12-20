using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using CMS.DataEngine;
using CMS.SiteProvider;


namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class assigning (graph) colors to AB variants. Is used to generate custom (and cached) colors to Reporting module, so 
    /// colors of AB testing graphs are not different throughout CMS.
    /// </summary>
    public static class ABVariantColorAssigner
    {
        #region "Variables"

        /// <summary>
        /// Palette from which to assign colors to a variant.
        /// </summary>
        private static readonly Color[] Palette =
        {
            Color.FromArgb(0x00, 0x00, 0x00),   // Black
            Color.FromArgb(0xCB, 0x20, 0x26),   // Red
            Color.FromArgb(0xF8, 0x9C, 0x1B),   // Orange
            Color.FromArgb(0x16, 0x47, 0x9C),   // Dark blue
            Color.FromArgb(0xEC, 0x7F, 0xB2),   // Pink
            Color.FromArgb(0x4F, 0xB7, 0x48),   // Light green
            Color.FromArgb(0x67, 0x3F, 0x98),   // Violet
            Color.FromArgb(0x66, 0xCD, 0xF4),   // Light blue
            Color.FromArgb(0x8D, 0x63, 0x26),   // Brown
            Color.FromArgb(0x0F, 0x68, 0x35),   // Dark green
        };


        /// <summary>
        /// Cached colors assigned to variants of all tests.
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Color>> AssignedColors = new ConcurrentDictionary<int, ConcurrentDictionary<int, Color>>();

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns cached colors for AB variants that belong to given test.
        /// </summary>
        /// <param name="test">AB test to get cached colors for</param>
        /// <returns>Dictionary of cached colors</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null</exception>
        public static Dictionary<string, Color> GetColors(ABTestInfo test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            // Ensure test cache
            AssignedColors.GetOrAdd(test.ABTestID, new ConcurrentDictionary<int, Color>());

            var site = SiteInfoProvider.GetSiteInfo(test.ABTestSiteID);

            var colors = new Dictionary<string, Color>();
            IEnumerable<BaseInfo> variants;

            if (site.SiteIsContentOnly)
            {
                variants = ABCachedObjects.GetVariantsData(test).OrderBy(x => x.ABVariantID);
            }
            else
            {
                variants = ABCachedObjects.GetVariants(test).OrderBy(x => x.ABVariantID);
            }

            foreach (BaseInfo variant in variants)
            {
                colors[variant.Generalized.ObjectDisplayName] = GetVariantColor(variant.Generalized.ObjectID, variant.Generalized.ObjectParentID);
            }

            return colors;
        }


        /// <summary>
        /// Tries to remove AB test from cache.
        /// </summary>
        /// <param name="test">AB test to remove</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null</exception>
        public static void RemoveTest(ABTestInfo test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }
            ((IDictionary)AssignedColors).Remove(test.ABTestID);
        }


        /// <summary>
        /// Tries to remove AB variant from cache. 
        /// </summary>
        /// <param name="variant">AB variant to remove</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="variant"/> is null</exception>
        public static void RemoveVariant(ABVariantInfo variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }

            RemoveVariantInternal(variant.ABVariantTestID, variant.ABVariantID);
        }


        /// <summary>
        /// Tries to remove AB variant data from cache. 
        /// </summary>
        /// <param name="variant">AB variant to remove</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="variant"/> is null</exception>
        public static void RemoveVariantData(ABVariantDataInfo variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }

            RemoveVariantInternal(variant.ABVariantTestID, variant.ABVariantID);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets cached variant color.
        /// </summary>
        /// <param name="variantID">ID of variant to get color for</param>
        /// <param name="abTestID">ID of test the variant belongs to</param>
        /// <returns>Cached variant color</returns>
        private static Color GetVariantColor(int variantID, int abTestID)
        {
            return AssignedColors[abTestID].GetOrAdd(variantID, _ => FindLeastUsedColor(AssignedColors[abTestID]));
        }


        /// <summary>
        /// Finds the least used color in given test dictionary. If there is an ambiguity, returns the first from internal palette.
        /// </summary>
        /// <param name="assignedColors">AB Test dictionary of colors to search for the least used</param>
        /// <returns>Color that is least used</returns>
        private static Color FindLeastUsedColor(ConcurrentDictionary<int, Color> assignedColors)
        {
            // Shortcut - if the dictionary is not filled with all colors, use the first not used color
            var missingColor = Palette.FirstOrDefault(x => !assignedColors.Values.Contains(x));
            if (missingColor != default(Color))
            {
                return missingColor;
            }

            // Group colors by their usage
            var colorsWithCounts = assignedColors
                .GroupBy(x => x.Value)
                .Select(x => new { Color = x.Key, Count = x.Count() })
                .ToList();

            // Get list of colors that are used least
            var leastUsedColors = colorsWithCounts
                .Where(x => x.Count == colorsWithCounts.Select(y => y.Count).Min())
                .Select(x => x.Color);

            // Get the first color of the least used
            return Palette.FirstOrDefault(leastUsedColors.Contains);
        }


        private static void RemoveVariantInternal(int testId, int variantId)
        {
            if (AssignedColors.ContainsKey(testId))
            {
                ((IDictionary)AssignedColors[testId]).Remove(variantId);
            }
        }

        #endregion
    }
}
