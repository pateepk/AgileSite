using System;
using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.OnlineMarketing.Internal;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Defines set of methods for managing test variants.
    /// </summary>
    public interface IABTestManager
    {
        /// <summary>
        /// Creates a new A/B test for <paramref name="page"/> with included traffic set to 100
        /// and stores it to the database. If the page has any A/B test variants defined,
        /// they are cleared.
        /// </summary>
        /// <param name="page">Page to create A/B test for.</param>
        /// <returns>Returns a new A/B for <paramref name="page"/>.</returns>
        /// <remarks>
        /// The A/B test's name is inferred using <see cref="ABTestNameHelper.GetDefaultDisplayName"/>.
        /// </remarks>
        ABTestInfo CreateABTest(TreeNode page);


        /// <summary>
        /// Gets A/B test without a winner for a given <paramref name="page"/>.
        /// Returns null if no A/B test is associated with the <paramref name="page"/> or associated A/B test has a winner.
        /// </summary>
        /// <param name="page">Page to retrieve an unconclued A/B test for.</param>
        /// <returns>Returns an unconcluded A/B test for the page, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        ABTestInfo GetABTestWithoutWinner(TreeNode page);


        /// <summary>
        /// Gets A/B test for <paramref name="page"/> which is still running.
        /// Returns null if no running A/B is available for the page.
        /// </summary>
        /// <param name="page">Page to retrieve running A/B test for.</param>
        /// <returns>Returns running A/B test for the page, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        ABTestInfo GetRunningABTest(TreeNode page);


        /// <summary>
        /// Adds a new A/B test variant into <paramref name="page"/> based on an existing source variant.
        /// </summary>
        /// <param name="page">Page for which to add a new variant.</param>
        /// <param name="sourceVariantGuid">GUID of the source variant (if null or <see cref="Guid.Empty"/>, original is assumed as the source).</param>
        /// <returns>Returns the new A/B test variant.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="sourceVariantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        IABTestVariant AddVariant(TreeNode page, Guid? sourceVariantGuid);


        /// <summary>
        /// Removes an A/B test variant identified by <paramref name="variantGuid"/> from <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page from which to remove a variant.</param>
        /// <param name="variantGuid">GUID of the variant to be removed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the original A/B variant is to be deleted.</exception>
        void RemoveVariant(TreeNode page, Guid variantGuid);


        /// <summary>
        /// Promotes a variant identified by <paramref name="variantGuid"/> as the winner variant.
        /// Winning variant is stored in the database within the A/B test.
        /// </summary>
        /// <param name="page">Page for which to promote the winner variant.</param>
        /// <param name="variantGuid">GUID of the variant to be promoted.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when there is no unconcluded A/B test or <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the A/B test for the current <paramref name="page"/> has not finished yet.</exception>
        /// <seealso cref="GetABTestWithoutWinner(TreeNode)"/>
        /// <seealso cref="ABTestStatusEvaluator.ABTestIsFinished(ABTestInfo)"/>
        void PromoteVariant(TreeNode page, Guid variantGuid);


        /// <summary>
        /// Returns all A/B test variants existing for given <paramref name="page"/>.
        /// </summary>
        /// <returns>
        /// Existing variants or empty enumeration when document has empty A/B variant configuration.
        /// Variants are represented by <see cref="IABTestVariant"/> interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">When passed <paramref name="page"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When document has malformed A/B variant configuration data.</exception>
        IEnumerable<IABTestVariant> GetVariants(TreeNode page);


        /// <summary>
        /// Renames an A/B test variant in <paramref name="page"/> identified by variant GUID with given name.
        /// </summary>
        /// <param name="page">Page in which to rename the variant.</param>
        /// <param name="variantGuid">Unique identifier of the variant for which the change the name.</param>
        /// <param name="newVariantName">New name for the variant identified with <paramref name="variantGuid"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newVariantName"/> is null, empty or has more than 100 characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no variant with <paramref name="variantGuid"/> identifier.</exception>
        void RenameVariant(TreeNode page, Guid variantGuid, string newVariantName);


        /// <summary>
        /// Updates an A/B test variant in <paramref name="page"/> identified by variant GUID with given Page builder widgets configuration.
        /// </summary>
        /// <param name="page">Page in which to update the variant.</param>
        /// <param name="variantGuid">Unique identifier of the variant for which to update the configuration.</param>
        /// <param name="configurationSource">Source of the configuration to update the variant with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> or <paramref name="configurationSource"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        void UpdateVariant(TreeNode page, Guid variantGuid, VariantConfigurationSource configurationSource);
    }
}