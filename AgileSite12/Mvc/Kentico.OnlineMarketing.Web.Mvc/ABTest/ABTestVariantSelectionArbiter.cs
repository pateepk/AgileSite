using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DocumentEngine;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Defines methods for getting the desired A/B page variant to display to the user.
    /// </summary>
    internal class ABTestVariantSelectionArbiter : IABTestVariantSelectionArbiter
    {
        public const string PAGE_AB_VARIANTS_CONFIGURATION = "DocumentABTestConfiguration";


        private IABTestConfigurationSerializer ABTestConfigurationSerializer
        {
            get;
        }


        private IABUserStateManagerFactory ABUserStateManagerFactory
        {
            get;
        }


        private ICachedABTestManager ABTestManager
        {
            get;
        }


        private IABTestArbiterHelper ABTestArbiterHelper
        {
            get;
        }


        internal Func<int, int> RandomNumberGenerator
        {
            get;
            set;
        } = StaticRandom.Next;


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestVariantSelectionArbiter"/>.
        /// </summary>
        /// <param name="abTestConfigurationSerializer">Serializer for A/B test variants configuration.</param>
        /// <param name="abUserStateManagerFactory">Instance of manager factory for the user state.</param>
        /// <param name="abTestManager">Instance of manager for managing A/B test variants.</param>
        /// <param name="abTestArbiterHelper">Instance of helper for A/B test variants.</param>
        public ABTestVariantSelectionArbiter(IABTestConfigurationSerializer abTestConfigurationSerializer, IABUserStateManagerFactory abUserStateManagerFactory,
                                             ICachedABTestManager abTestManager, IABTestArbiterHelper abTestArbiterHelper)
        {
            ABTestConfigurationSerializer = abTestConfigurationSerializer ?? throw new ArgumentNullException(nameof(abTestConfigurationSerializer));
            ABUserStateManagerFactory = abUserStateManagerFactory ?? throw new ArgumentNullException(nameof(abUserStateManagerFactory));
            ABTestManager = abTestManager ?? throw new ArgumentNullException(nameof(abTestManager));
            ABTestArbiterHelper = abTestArbiterHelper ?? throw new ArgumentNullException(nameof(abTestArbiterHelper));
        }


        /// <summary>
        /// Gets A/B variant from the user's cookie or randomly selects one and assigns it to the user. Also checks whether the user should be excluded or not
        /// (see <see cref="ABTestInfo.ABTestIncludedTraffic"/> or <see cref="ABTestInfo.ABTestVisitorTargeting"/>).
        /// </summary>
        /// <param name="page">Page for which to get the A/B test variant.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public ABTestVariant SelectVariant(TreeNode page)
        {
            return SelectVariant(page, out _);
        }


        /// <summary>
        /// Gets A/B variant from the user's cookie or randomly selects one and assigns it to the user. Also checks whether the user should be excluded or not
        /// (see <see cref="ABTestInfo.ABTestIncludedTraffic"/> or <see cref="ABTestInfo.ABTestVisitorTargeting"/>).
        /// </summary>
        /// <param name="page">Page for which to get the A/B test variant.</param>
        /// <param name="variantAssigned">True if variant has been assigned to user, false if it has been loaded from cookie.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public ABTestVariant SelectVariant(TreeNode page, out bool variantAssigned)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            variantAssigned = false;

            var runningABTest = ABTestManager.GetRunningABTest(page);
            if (runningABTest == null)
            {
                return null;
            }

            var abUserStateManager = ABUserStateManagerFactory.Create<Guid?>(runningABTest.ABTestName);
            if (abUserStateManager.IsExcluded)
            {
                return null;
            }

            if (!ABTestArbiterHelper.IsCurrentUserABTested(runningABTest))
            {
                abUserStateManager.Exclude();

                return null;
            }

            var abTestVariants = GetABTestVariants(page.GetValue(PAGE_AB_VARIANTS_CONFIGURATION, String.Empty));
            if (abTestVariants == null)
            {
                abUserStateManager.Exclude();

                return null;
            }

            ABTestVariant variant = null;
            var variantGuid = abUserStateManager.GetVariantIdentifier();
            if (variantGuid == null)
            {
                SelectAndAssignRandomVariant(abUserStateManager, abTestVariants, out variant);
                variantAssigned = true;

                return variant;
            }

            variant = GetAssignedVariant(abTestVariants, variantGuid.Value);
            if (variant != null)
            {
                abUserStateManager.AssignVariant(variant.Guid);

                return variant;
            }

            SelectAndAssignRandomVariant(abUserStateManager, abTestVariants, out variant);
            variantAssigned = true;

            return variant;
        }


        /// <summary>
        /// Selects and assigns a variant from <paramref name="abTestVariants"/> to the current user.
        /// </summary>
        /// <param name="abUserStateManager">User state manager to be used.</param>
        /// <param name="abTestVariants">Collection of A/B test variants.</param>
        /// <param name="variant">Selected variant.</param>
        private void SelectAndAssignRandomVariant(IABUserStateManager<Guid?> abUserStateManager, ICollection<ABTestVariant> abTestVariants, out ABTestVariant variant)
        {
            variant = SelectRandomTestVariant(abTestVariants);

            abUserStateManager.AssignVariant(variant.Guid);
        }


        /// <summary>
        /// Returns the assigned A/B test variant from <paramref name="abTestVariants"/> or null if the user has no assigned variants.
        /// </summary>
        /// <param name="abTestVariants">Collection of A/B test variants.</param>
        /// <param name="variantIdentifier">Identifier of the A/B variant assigned to the current user.</param>
        private ABTestVariant GetAssignedVariant(ICollection<ABTestVariant> abTestVariants, Guid variantIdentifier)
        {
            ABTestVariant variant = null;

            if (!variantIdentifier.Equals(Guid.Empty))
            {
                variant = abTestVariants.FirstOrDefault(v => v.Guid == variantIdentifier);
            }

            return variant;
        }


        /// <summary>
        /// Returns the A/B test variants for the current <paramref name="variantsConfiguration"/> or null if there are no A/B test variants.
        /// </summary>
        /// <param name="variantsConfiguration">A/B variants configuration in JSON format.</param>
        private ICollection<ABTestVariant> GetABTestVariants(string variantsConfiguration)
        {
            if (String.IsNullOrEmpty(variantsConfiguration))
            {
                return null;
            }

            var testVariants = ABTestConfigurationSerializer.Deserialize(variantsConfiguration);

            if (!testVariants.Variants.Any())
            {
                return null;
            }

            return testVariants.Variants;
        }


        /// <summary>
        /// Returns a random A/B variant.
        /// </summary>
        /// <param name="variants">Collection of A/B variants.</param>
        private ABTestVariant SelectRandomTestVariant(ICollection<ABTestVariant> variants)
        {
            int randomVariantPosition = RandomNumberGenerator(variants.Count);

            return variants.ElementAt(randomVariantPosition);
        }
    }
}
