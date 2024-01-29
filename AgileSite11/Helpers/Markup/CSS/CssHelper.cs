using System;
using System.Linq;

namespace CMS.Helpers
{
    /// <summary>
    /// Utility methods for CSS manipulation.
    /// </summary>
    public class CssHelper 
    {
        /// <summary>
        /// Encloses a style declaration block in an HTML style element.
        /// </summary>
        /// <param name="style">Style block containing CSS rules</param>
        /// <returns>HTML style element with enclosed CSS rules</returns>
        public static string GetStyle(string style)
        {
            return string.Format(
                @"<style type=""text/css"">
                {0}
              </style>",
                style);
        }


        /// <summary>
        /// Returns class attribute or empty string if no class is defined.
        /// </summary>
        /// <param name="cssClass">Css class</param>
        public static string GetCssClassAttribute(string cssClass)
        {
            return string.IsNullOrEmpty(cssClass) ? string.Empty : " class=\"" + cssClass + "\"";
        }


        /// <summary>
        /// Ensures the given class in the CSS class
        /// </summary>
        /// <param name="origClass">Original CSS class</param>
        /// <param name="newClass">CSS class to ensure</param>
        public static string EnsureClass(string origClass, string newClass)
        {
            if (String.IsNullOrEmpty(origClass))
            {
                return newClass;
            }

            string[] origClasses = origClass.Split(new[]
            {
                ' '
            }, StringSplitOptions.RemoveEmptyEntries);
            if (!origClasses.Contains(newClass))
            {
                origClass += " " + newClass;
            }

            return origClass;
        }


        /// <summary>
        /// Joins the given list of CSS classes. Omits empty and null strings.
        /// </summary>
        public static string JoinClasses(params string[] classes)
        {
            return string.Join(" ", classes.Where(s => !String.IsNullOrEmpty(s)));
        }
    }
}