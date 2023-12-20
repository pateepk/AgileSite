using System;
using System.Collections.Generic;
using System.Web.Mvc;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Helper class for selector components.
    /// </summary>
    internal static class SelectorDataSourceHelper
    {
        /// <summary>
        /// Parses given <paramref name="dataSource"/> by <see cref="Environment.NewLine"/> and semicolon character.
        /// </summary>
        public static IEnumerable<SelectListItem> ParseDataSource(string dataSource)
        {
            var source = dataSource ?? string.Empty;
            source = source.Trim();

            var lines = source.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var items = line.Trim().Split(';');

                var listItem = new SelectListItem
                {
                    Value = items[0]
                };
                listItem.Text = items.Length > 1 ? items[1] : listItem.Value;
                listItem.Text = ResHelper.LocalizeString(listItem.Text);

                yield return listItem;
            }
        }
    }
}
