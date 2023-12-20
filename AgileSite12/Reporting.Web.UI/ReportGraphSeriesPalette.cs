using System.Drawing;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Class containing the default color palette for report graphs
    /// </summary>
    public static class ReportGraphSeriesPalette
    {
        private static Color[] mDefaultSeriesPallete = new Color[] {
                    ColorTranslator.FromHtml("#0f7abc"),
                    ColorTranslator.FromHtml("#f69c04"),
                    ColorTranslator.FromHtml("#518f02"),
                    ColorTranslator.FromHtml("#c0282a"),
                    ColorTranslator.FromHtml("#e6c32a"),
                    ColorTranslator.FromHtml("#008e98"),
                    ColorTranslator.FromHtml("#7a3a92"),
                    ColorTranslator.FromHtml("#7bac3b"),
                    ColorTranslator.FromHtml("#925a00"),
                    ColorTranslator.FromHtml("#54aadf"),
                    ColorTranslator.FromHtml("#9c5da7"),
                    ColorTranslator.FromHtml("#e56364"),
                    ColorTranslator.FromHtml("#bf995e"),
                    ColorTranslator.FromHtml("#58c6cb"),
                    ColorTranslator.FromHtml("#403e3d"),
                    ColorTranslator.FromHtml("#a3a2a2")
                };

        /// <summary>
        /// Gets the default color palette.
        /// </summary>
        public static Color[] Colors
        {
            get
            {
                return mDefaultSeriesPallete;
            }
        }
    }
}
