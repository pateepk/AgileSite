using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

[assembly: RegisterImplementation(typeof(IABTestManager), typeof(ABTestManager), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class provides management methods for retrieving and creating A/B test variants.
    /// Methods of this class do not save any changes made to pages, they have to be saved explicitly.
    /// </summary>
    public class ABTestManager : IABTestManager
    {
        private const string AB_TEST_CONFIGURATION_COLUMN_NAME = "DocumentABTestConfiguration";


        private static readonly VariantNameGenerator VariantNameGenerator = new VariantNameGenerator();


        private IABTestConfigurationSerializer ConfigurationSerializer { get; }
        

        /// <summary>
        /// Maximum length of a variant name.
        /// </summary>
        public const int MAXIMUM_VARIANT_NAME_LENGTH = 100;


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestManager"/>.
        /// </summary>
        /// <param name="serializer">Serializes the <see cref="ABTestConfiguration"/>.</param>
        public ABTestManager(IABTestConfigurationSerializer serializer)
        {
            ConfigurationSerializer = serializer;
        }


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
        public ABTestInfo CreateABTest(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var abTestInfo = new ABTestInfo
            {
                ABTestDisplayName = ABTestNameHelper.GetDefaultDisplayName(page),
                ABTestCulture = page.DocumentCulture,
                ABTestOriginalPage = page.NodeAliasPath,
                ABTestSiteID = page.NodeSiteID,
                ABTestIncludedTraffic = 100
            };
            abTestInfo.Generalized.EnsureCodeName();

            ABTestInfoProvider.SetABTestInfo(abTestInfo);

            if (!String.IsNullOrEmpty(page.GetABTestConfiguration()))
            {
                page.SetABTestConfiguration(null);
            }

            return abTestInfo;
        }


        /// <summary>
        /// Gets A/B test without a winner for a given <paramref name="page"/>.
        /// Returns null if no A/B test is associated with the <paramref name="page"/> or associated A/B test has a winner.
        /// </summary>
        /// <param name="page">Page to retrieve an unconclued A/B test for.</param>
        /// <returns>Returns an unconcluded A/B test for the page, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public ABTestInfo GetABTestWithoutWinner(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return ABTestInfoProvider.GetABTests()
                .WhereEquals("ABTestOriginalPage", page.NodeAliasPath)
                .WhereEquals("ABTestSiteID", page.NodeSiteID)
                .WhereEquals("ABTestCulture", page.DocumentCulture)
                .WhereNull("ABTestWinnerGUID")
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets A/B test for <paramref name="page"/> which is still running.
        /// Returns null if no running A/B is available for the page.
        /// </summary>
        /// <param name="page">Page to retrieve running A/B test for.</param>
        /// <returns>Returns running A/B test for the page, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public ABTestInfo GetRunningABTest(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return ABTestInfoProvider.GetABTests()
                .WhereEquals("ABTestSiteID", page.NodeSiteID)
                .WhereEquals("ABTestOriginalPage", page.NodeAliasPath)
                .WhereEquals("ABTestCulture", page.DocumentCulture)
                .Where("ABTestOpenFrom", QueryOperator.LessOrEquals, DateTime.Now)
                .And(
                    new WhereCondition("ABTestOpenTo", QueryOperator.GreaterOrEquals, DateTime.Now)
                        .Or()
                        .WhereNull("ABTestOpenTo")
                )
                .FirstOrDefault();
        }


        /// <summary>
        /// Adds a new A/B test variant into <paramref name="page"/> based on an existing source variant.
        /// </summary>
        /// <param name="page">Page for which to add a new variant.</param>
        /// <param name="sourceVariantGuid">GUID of the source variant (if null or <see cref="Guid.Empty"/>, original is assumed as the source).</param>
        /// <returns>Returns the new A/B test variant.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="sourceVariantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        public IABTestVariant AddVariant(TreeNode page, Guid? sourceVariantGuid)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var serializedVariantsConfiguration = page.GetABTestConfiguration() ?? String.Empty;
            var variantsConfiguration = ConfigurationSerializer.Deserialize(serializedVariantsConfiguration) ?? new ABTestConfiguration();

            var sourceVariantConfiguration = GetSourceVariantConfigurationOrThrow(page, variantsConfiguration, sourceVariantGuid);

            if (variantsConfiguration.Variants.Count == 0)
            {
                // If the first variant is to be created, add the original as well
                var originalVariant = CreateABVariant(VariantNameGenerator.GetOriginalName(), true, null);
                variantsConfiguration.Variants.Add(originalVariant);
            }

            var newVariantName = VariantNameGenerator.GetDefaultUniqueName(variantsConfiguration.Variants.Select(i => i.Name));
            var newVariant = CreateABVariant(newVariantName, false, sourceVariantConfiguration);

            variantsConfiguration.Variants.Add(newVariant);

            UpdateDocument(page, variantsConfiguration);

            return newVariant;
        }


        /// <summary>
        /// Removes an A/B test variant identified by <paramref name="variantGuid"/> from <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page from which to remove a variant.</param>
        /// <param name="variantGuid">GUID of the variant to be removed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the original A/B variant is to be deleted.</exception>
        public void RemoveVariant(TreeNode page, Guid variantGuid)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var variantsConfiguration = ConfigurationSerializer.Deserialize(page.GetABTestConfiguration() ?? String.Empty);
            var variant = GetVariantOrThrow(page, variantsConfiguration, variantGuid);

            if (variant.IsOriginal)
            {
                throw new InvalidOperationException("The original A/B test variant cannot be deleted.");
            }

            variantsConfiguration.Variants.Remove(variant);

            UpdateDocument(page, variantsConfiguration);
        }


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
        public void PromoteVariant(TreeNode page, Guid variantGuid)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var abTest = GetABTestWithoutWinner(page);
            if (abTest == null)
            {
                throw new ArgumentException("There is no A/B test without a winner on this page.", nameof(page));
            }
            if (!ABTestStatusEvaluator.ABTestIsFinished(abTest))
            {
                throw new InvalidOperationException("It is not possible to promote a winning variant in an unfinished A/B test.");
            }

            var variantsConfiguration = ConfigurationSerializer.Deserialize(page.GetABTestConfiguration() ?? String.Empty);
            var variant = GetVariantOrThrow(page, variantsConfiguration, variantGuid);

            abTest.ABTestWinnerGUID = variantGuid;
            ABTestInfoProvider.SetABTestInfo(abTest);

            page.SetABTestConfiguration(null);

            if (!variant.IsOriginal)
            {
                page.SetPageBuilderWidgets(variant.PageBuilderWidgets);
                page.SetPageTemplateConfiguration(variant.PageTemplate);
            }
        }


        /// <summary>
        /// Returns all A/B test variants existing for given <paramref name="page"/>.
        /// </summary>
        /// <returns>
        /// Existing variants or empty enumeration when document has empty A/B variant configuration.
        /// Variants are represented by <see cref="IABTestVariant"/> interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">When passed <paramref name="page"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When document has malformed A/B variant configuration data.</exception>
        public IEnumerable<IABTestVariant> GetVariants(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }
            
            var configurationJson = page.GetValue<string>(AB_TEST_CONFIGURATION_COLUMN_NAME, null);
            if (String.IsNullOrEmpty(configurationJson))
            {
                return Enumerable.Empty<IABTestVariant>();
            }

            var variantsConfiguration = ConfigurationSerializer.Deserialize(configurationJson);

            return variantsConfiguration.Variants.ToArray();
        }


        /// <summary>
        /// Renames an A/B test variant in <paramref name="page"/> identified by variant GUID with given name.
        /// </summary>
        /// <param name="page">Page in which to rename the variant.</param>
        /// <param name="variantGuid">Unique identifier of the variant for which the change the name.</param>
        /// <param name="newVariantName">New name for the variant identified with <paramref name="variantGuid"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newVariantName"/> is null, empty or has more than <see cref="MAXIMUM_VARIANT_NAME_LENGTH"/> characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no variant with <paramref name="variantGuid"/> identifier.</exception>
        public void RenameVariant(TreeNode page, Guid variantGuid, string newVariantName)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            newVariantName = newVariantName?.Trim();
            if (String.IsNullOrEmpty(newVariantName))
            {
                throw new ArgumentException($"Argument '{nameof(newVariantName)}' cannot be null or empty.", nameof(newVariantName));
            }

            if (newVariantName.Length > MAXIMUM_VARIANT_NAME_LENGTH)
            {
                throw new ArgumentException($"Argument '{nameof(newVariantName)}' cannot contain more than {MAXIMUM_VARIANT_NAME_LENGTH} characters.", nameof(newVariantName));
            }

            UpdateVariant(page, variantGuid, (variantToUpdate) =>
            {
                variantToUpdate.Name = newVariantName;
            });
        }


        /// <summary>
        /// Updates an A/B test variant in <paramref name="page"/> identified by variant GUID with given Page builder widgets configuration.
        /// </summary>
        /// <param name="page">Page in which to update the variant.</param>
        /// <param name="variantGuid">Unique identifier of the variant for which to update the configuration.</param>
        /// <param name="configurationSource">Source of the configuration to update the variant with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> or <paramref name="configurationSource"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        public void UpdateVariant(TreeNode page, Guid variantGuid, VariantConfigurationSource configurationSource)
        {
            if (configurationSource == null)
            {
                throw new ArgumentNullException(nameof(configurationSource));
            }

            UpdateVariant(page, variantGuid, variant =>
            {
                if (variant.IsOriginal)
                {
                    page.SetPageBuilderWidgets(configurationSource.Widgets);
                    page.SetPageTemplateConfiguration(configurationSource.PageTemplate);
                }
                else
                {
                    variant.PageBuilderWidgets = configurationSource.Widgets;
                    variant.PageTemplate = configurationSource.PageTemplate;
                }
            });
        }


        private void UpdateVariant(TreeNode page, Guid variantGuid, Action<ABTestVariant> updateAction)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var variantsConfiguration = ConfigurationSerializer.Deserialize(page.GetABTestConfiguration());
            var updatedVariant = variantsConfiguration?.Variants.FirstOrDefault(v => v.Guid == variantGuid);
            if (updatedVariant == null)
            {
                throw new ArgumentException($"Cannot set A/B test variant '{variantGuid}' in page with ID {page.NodeID}. " +
                    $"The variant was not found in the page's configuration. The page's configuration follows:{Environment.NewLine}{Environment.NewLine}{page.GetABTestConfiguration() ?? "(null)"}", nameof(variantGuid));
            }

            updateAction(updatedVariant);

            page.SetABTestConfiguration(ConfigurationSerializer.Serialize(variantsConfiguration));
        }


        /// <summary>
        /// Gets the variants configuration of the <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page for which to get the variants configuration.</param>
        /// <param name="testConfiguration">A/B test configuration of the page.</param>
        /// <param name="sourceVariantGuid">GUID of the variant.</param>
        private VariantConfigurationSource GetSourceVariantConfigurationOrThrow(TreeNode page, ABTestConfiguration testConfiguration, Guid? sourceVariantGuid)
        {
            if (sourceVariantGuid == null || sourceVariantGuid == Guid.Empty)
            {
                return new VariantConfigurationSource
                {
                    Widgets = page.GetPageBuilderWidgets(),
                    PageTemplate = page.GetPageTemplateConfiguration()
                };
            }

            var variant = testConfiguration.Variants.FirstOrDefault(v => v.Guid == sourceVariantGuid);
            if (variant == null)
            {
                throw new ArgumentException($"Cannot create a new A/B test variant for page with ID '{page.NodeID}' based on source variant with identifier '{sourceVariantGuid}'. " +
                    $"The variant was not found in the page's configuration. The page's configuration follows:{Environment.NewLine}{Environment.NewLine}{page.GetABTestConfiguration()}", nameof(sourceVariantGuid));
            }

            if (variant.IsOriginal)
            {
                return new VariantConfigurationSource
                {
                    Widgets = page.GetPageBuilderWidgets(),
                    PageTemplate = page.GetPageTemplateConfiguration()
                };
            }

            return new VariantConfigurationSource
            {
                Widgets = variant.PageBuilderWidgets,
                PageTemplate = variant.PageTemplate
            };
        }


        /// <summary>
        /// Gets a variant from <paramref name="testConfiguration"/> belonging to <paramref name="page"/> identified by <paramref name="variantGuid"/>.
        /// </summary>
        /// <param name="page">Page for which to get the variant.</param>
        /// <param name="testConfiguration">A/B test configuration of the page.</param>
        /// <param name="variantGuid">GUID of the variant to get.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="page"/> does not contain A/B test variant identified by <paramref name="variantGuid"/>.</exception>
        private ABTestVariant GetVariantOrThrow(TreeNode page, ABTestConfiguration testConfiguration, Guid variantGuid)
        {
            return testConfiguration?.Variants.FirstOrDefault(v => v.Guid == variantGuid) ??
                throw new ArgumentException($"Cannot find the A/B test variant for page with ID '{page.NodeID}' based on source variant with identifier '{variantGuid}'. " +
                    $"The variant was not found in the page's configuration. The page's configuration follows{Environment.NewLine}{Environment.NewLine}{page.GetABTestConfiguration()}", nameof(variantGuid));
        }


        /// <summary>
        /// Returns newly created instance of <see cref="ABTestVariant"/> using given parameters.
        /// </summary>
        private ABTestVariant CreateABVariant(string name, bool isOriginal, VariantConfigurationSource pageBuilderConfiguration)
        {
            return new ABTestVariant(name, isOriginal)
            {
                PageBuilderWidgets = pageBuilderConfiguration?.Widgets,
                PageTemplate = pageBuilderConfiguration?.PageTemplate
            };
        }


        /// <summary>
        /// Updates given <paramref name="page"/> by given <paramref name="variantsConfiguration"/>.
        /// </summary>
        private void UpdateDocument(TreeNode page, ABTestConfiguration variantsConfiguration)
        {
            page.SetValue(AB_TEST_CONFIGURATION_COLUMN_NAME, ConfigurationSerializer.Serialize(variantsConfiguration));
        }
    }
}
