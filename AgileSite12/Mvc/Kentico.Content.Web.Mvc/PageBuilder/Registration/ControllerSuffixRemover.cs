using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Removes "Controller" suffix from a controller name.
    /// </summary>
    internal sealed class ControllerSuffixRemover
    {
        /// <summary>
        /// Controller suffix.
        /// </summary>
        public const string CONTROLLER_NAME_SUFFIX = "Controller";

        /// <summary>
        /// Removes "Controller" suffix from given controller name.
        /// </summary>
        /// <param name="name">Name of a controller from which "Controller" suffix should be removed.</param>
        public string Remove(string name)
        {
            if (name.EndsWith(CONTROLLER_NAME_SUFFIX, StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(0, name.Length - CONTROLLER_NAME_SUFFIX.Length);
            }

            return name;
        }
    }
}
