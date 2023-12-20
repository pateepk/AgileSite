using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Describes A/B test variant object.
    /// </summary>
    public interface IABTestVariant
    {
        /// <summary>
        /// Gets the identifier of a A/B test variant.
        /// </summary>
        Guid Guid { get; }


        /// <summary>
        /// Gets the name of a A/B test variant.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Indicates whether this A/B test variant is original variant.
        /// </summary>
        bool IsOriginal { get; }


        /// <summary>
        /// Configuration of page builder widgets in JSON format, associated with this variant.
        /// </summary>
        string PageBuilderWidgets { get; }


        /// <summary>
        /// Configuration of page template in JSON format, associated with this variant.
        /// </summary>
        string PageTemplate { get; }
    }
}