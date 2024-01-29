namespace CMS.ResponsiveImages
{
    /// <summary>
    /// General interface for variant context scopes which restrict generating of variants based on the context in which the variant is generated.
    /// </summary>
    public interface IVariantContextScope
    {
        /// <summary>
        /// Checks the given variant context. Returns true if the context matches the defined scope.
        /// </summary>
        /// <param name="context">Variant context.</param>
        bool CheckContext(IVariantContext context);
    }
}