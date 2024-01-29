using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using TestContext = NUnit.Framework.TestContext;
using Assert = NUnit.Framework.Assert;

namespace CMS.Tests
{
    /// <summary>
    /// Provides methods for checking the test categories
    /// </summary>
    public class TestsCategoryCheck
    {
        const string APP_KEY_EXCLUDED_CATEGORIES = "CMSTestExcludedCategories";
        const string APP_KEY_RESTRICTED_CATEGORIES = "CMSTestRestrictedCategories";

        private static readonly Lazy<IEnumerable<string>> mExcludedCategories = new Lazy<IEnumerable<string>>(() => GetCategoriesFromConfig(APP_KEY_EXCLUDED_CATEGORIES));
        private static readonly Lazy<IEnumerable<string>> mRestrictedCategories = new Lazy<IEnumerable<string>>(() => GetCategoriesFromConfig(APP_KEY_RESTRICTED_CATEGORIES));


        private static IEnumerable<string> ExcludedCategories
        {
            get
            {
                return mExcludedCategories.Value;
            }
        }


        private static IEnumerable<string> RestrictedCategories
        {
            get
            {
                return mRestrictedCategories.Value;
            }
        }


        /// <summary>
        /// Throw <see cref="IgnoreException"/> if current test is assigned to not restricted or excluded categories.
        /// </summary>
        internal static void CheckCategories(Type testType)
        {
            // End check if MSTest
            if (AutomatedTests.IsMSTest(testType))
            {
                return;
            }

            // End check if no categories in tests config
            if (!ExcludedCategories.Any() && !RestrictedCategories.Any())
            {
                return;
            }

            // Get categories assigned to current test
            var test = TestContext.CurrentContext.Test;
            var cat = test.Properties["Category"];
            var currentTestCategories = cat.Cast<string>().ToArray();

            // Add categories assigned to current test class and assembly
            var classAndAssemblyCategories = GetClassAndAssemblyCategories(testType);
            currentTestCategories = currentTestCategories.Concat(classAndAssemblyCategories).ToArray();

            // End check if current test assigned to no category
            if (currentTestCategories.Any())
            {
                return;
            }

            // Ignore test if in excluded category
            if (currentTestCategories.Intersect(ExcludedCategories).Any())
            {
                Assert.Ignore("Test is assigned to excluded category.");
            }

            // Ignore test if not in restricted category
            if (!currentTestCategories.Intersect(RestrictedCategories).Any() && RestrictedCategories.Any())
            {
                Assert.Ignore("Test is not assigned to any restricted category.");
            }
        }


        /// <summary>
        /// Throw <see cref="IgnoreException"/> if no test in specified class passes category check.
        /// </summary>
        /// <param name="type">Test class type</param>
        internal static void CheckAllTestsCategories(Type type)
        {
            // End check if MSTest
            if (AutomatedTests.IsMSTest(type))
            {
                return;
            }

            // End check if not a fixture, it is wrapped by another test in that case
            if (!type.IsDefined(typeof(TestFixtureAttribute), true))
            {
                return;
            }

            var classAndAssemblyCategories = GetClassAndAssemblyCategories(type).ToArray();
            if (classAndAssemblyCategories.Intersect(ExcludedCategories).Any())
            {
                Assert.Ignore("Test fixture assigned to excluded category.");
            }

            var searchForTestMethodInRestrictedCategory = (RestrictedCategories.Any() && !classAndAssemblyCategories.Intersect(RestrictedCategories).Any());

            var allTestMethods = type.GetMethods()
                                     .Where(t => !t.IsDefined(typeof(IgnoreAttribute), false) && 
                                           (t.IsDefined(typeof(TestAttribute), false) || t.IsDefined(typeof(TestCaseAttribute), false) || t.IsDefined(typeof(TestCaseSourceAttribute), false)))
                                     .ToArray();
            
            // Check categories for each test method
            foreach (var testMethod in allTestMethods)
            {
                var categoryAttrs = (CategoryAttribute[])testMethod.GetCustomAttributes(typeof(CategoryAttribute), true);
                var currentTestCategories = categoryAttrs.Select(x => x.Name).ToArray();

                // Class and assembly are not in restricted category
                if (searchForTestMethodInRestrictedCategory)
                {
                    // Method is in restricted category and is not in excluded category
                    if (!currentTestCategories.Intersect(ExcludedCategories).Any() && currentTestCategories.Intersect(RestrictedCategories).Any())
                    {
                        return;
                    }
                }
                else
                {
                    // Method is not in excluded category
                    if (!currentTestCategories.Intersect(ExcludedCategories).Any() || !categoryAttrs.Any())
                    {
                        return;
                    }
                }
            }

            Assert.Ignore("All tests in fixture assigned to excluded category or not assigned to restricted category.");
        }


        /// <summary>
        /// Performs category check for assembly set up class.
        /// </summary>
        /// <param name="type">Type of class that performs assembly set up using <see cref="SetUpFixtureAttribute"/>.</param>
        /// <returns>True if assembly set up is required to run.</returns>
        internal static bool CheckAssemblySetUp(Type type)
        {
            var setUpAttrs = type.GetCustomAttributes(typeof(SetUpFixtureAttribute), true);
            if (setUpAttrs.Length == 0)
            {
                return false;
            }

            var categories = GetClassCategories(type).ToArray();

            // Check if in excluded categories
            if (categories.Intersect(ExcludedCategories).Any())
            {
                return false;
            }

            // Check if in restricted categories
            return !RestrictedCategories.Any() || categories.Intersect(RestrictedCategories).Any();
        }


        /// <summary>
        /// Gets categories specified in Tests.config file under given app key.
        /// </summary>
        /// <param name="keyName">Name of app key in Tests.config</param>
        private static IEnumerable<string> GetCategoriesFromConfig(string keyName)
        {
            string[] categories = { };
            char[] separator = { ';' };

            var globalTestsConfig = TestsConfig.GlobalTestsConfig;
            if (globalTestsConfig != null && globalTestsConfig.HasFile)
            {
                var globalAppSettings = globalTestsConfig.AppSettings;
                if (globalAppSettings.Settings[keyName] != null)
                {
                    categories = globalAppSettings.Settings[keyName].Value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return categories;
        }


        /// <summary>
        /// Gets categories assigned to current test class and assembly.
        /// </summary>
        /// <param name="type">Test type</param>
        /// <returns>Categories assigned to current test class and assembly.</returns>
        private static IEnumerable<string> GetClassAndAssemblyCategories(Type type)
        {
            return GetClassCategories(type).Concat(GetAssemblyCategories(type));
        }


        /// <summary>
        /// Gets categories assigned to current test class.
        /// </summary>
        /// <param name="type">Test type</param>
        /// <returns>Categories assigned to current test class.</returns>
        private static IEnumerable<string> GetClassCategories(Type type)
        {
            var categories = Enumerable.Empty<string>();

            var classCategoryAttrs = (CategoryAttribute[])type.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (classCategoryAttrs.Length > 0)
            {
                categories = classCategoryAttrs.Select(x => x.Name);
            }

            return categories;
        }


        /// <summary>
        /// Gets categories assigned to current test assembly.
        /// </summary>
        /// <param name="type">Test type</param>
        /// <returns>Categories assigned to current test assembly.</returns>
        private static IEnumerable<string> GetAssemblyCategories(Type type)
        {
            var categories = Enumerable.Empty<string>();

            var assemblyCategoryAttrs = (CategoryAttribute[])type.Assembly.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (assemblyCategoryAttrs.Length > 0)
            {
                categories = assemblyCategoryAttrs.Select(x => x.Name);
            }

            return categories;
        }
    }
}
