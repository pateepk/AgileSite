using System;

using CMS.DataEngine;
using CMS.ResponsiveImages;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Simplifies checking attachment variant context.
    /// </summary>
    public class AttachmentVariantContextScope : IVariantContextScope
    {
        private string aliasPath;
        private PathTypeEnum pathType;
        private string siteName;
        private string className;


        /// <summary>
        /// Checks the given attachment variant context. Returns true if the context matches the defined filter.
        /// </summary>
        /// <param name="context">Variant processing context.</param>
        public bool CheckContext(IVariantContext context)
        {
            // No context - automatically pass, caller explicitly wanted the variant to be generated
            if (context == null)
            {
                return true;
            }

            // Wrong type of scope - not covered by current scope
            var typedContext = context as AttachmentVariantContext;
            if (typedContext == null)
            {
                return false;
            }

            return
                CheckExactMatch(siteName, typedContext.SiteName) &&
                CheckExactMatch(className, typedContext.ClassName) &&
                CheckPathMatch(aliasPath, pathType, typedContext.AliasPath);
        }


        /// <summary>
        /// Defines the path limitation of the allowed scope to match.
        /// </summary>
        /// <param name="path">Node alias path.</param>
        /// <param name="type">Path type to define selection scope. Default value is <see cref="PathTypeEnum.Section"/>.</param>
        public AttachmentVariantContextScope Path(string path, PathTypeEnum type = PathTypeEnum.Section)
        {
            aliasPath = path;
            pathType = type;

            return this;
        }


        /// <summary>
        /// Defines the site of the allowed scope to match.
        /// </summary>
        /// <param name="siteIdentifier">Site identifier.</param>
        public AttachmentVariantContextScope OnSite(SiteInfoIdentifier siteIdentifier)
        {
            if (siteIdentifier != null)
            {
                siteName = siteIdentifier.ObjectCodeName;
            }

            return this;
        }


        /// <summary>
        /// Defines the page type of allowed scope to match.
        /// </summary>
        /// <param name="type">Class name of the page type.</param>
        public AttachmentVariantContextScope Type(string type)
        {
            className = type;

            return this;
        }


        private static bool CheckPathMatch(string checkPath, PathTypeEnum checkPathType, string contextPath)
        {
            if (String.IsNullOrEmpty(checkPath))
            {
                return true;
            }

            switch (checkPathType)
            {
                case PathTypeEnum.Single:
                    return CheckExactMatch(checkPath, contextPath);

                case PathTypeEnum.Children:
                    return contextPath.StartsWith(checkPath, StringComparison.InvariantCultureIgnoreCase) && !CheckExactMatch(checkPath, contextPath);

                case PathTypeEnum.Section:
                    return CheckExactMatch(checkPath, contextPath) || contextPath.StartsWith(checkPath, StringComparison.InvariantCultureIgnoreCase);

                default:
                    {
                        if (checkPath.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return contextPath.StartsWith(checkPath.Substring(0, checkPath.Length - 1), StringComparison.InvariantCultureIgnoreCase);
                        }

                        return CheckExactMatch(checkPath, contextPath);
                    }
            }
        }


        private static bool CheckExactMatch(string checkValue, string contextValue)
        {
            return String.IsNullOrEmpty(checkValue) || String.Equals(contextValue, checkValue, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}