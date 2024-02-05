using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class that validates given test (checks for valid from, to and if there's another test already running in this period).
    /// </summary>
    public static class ABTestValidator
    {
        /// <summary>
        /// Checks whether the start date is valid.
        /// </summary>
        /// <param name="start">Test start date</param>
        public static bool IsValidStart(DateTime start)
        {
            return (start.Date >= DateTime.Now.Date) || (start == DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Checks whether the finish date is valid.
        /// </summary>
        /// <param name="start">Test start date</param>
        /// <param name="finish">Test finish date</param>
        public static bool IsValidFinish(DateTime start, DateTime finish)
        {
            return (((finish.Date >= DateTime.Now.Date) && (finish > start)) || (finish == DateTimeHelper.ZERO_TIME));
        }


        /// <summary>
        /// Checks whether the test is in collision with other tests on the same page.
        /// </summary>
        /// <param name="abTest">AB test</param>
        public static bool CollidesWithOtherTests(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            return !String.IsNullOrEmpty(GetCollidingTestName(abTest));
        }


        /// <summary>
        /// Returns display name of the colliding test.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        public static string GetCollidingTestName(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            // Test with unfilled open from doesn't collide with other tests
            if (abTest.ABTestOpenFrom == DateTimeHelper.ZERO_TIME)
            {
                return null;
            }

            // Get other tests on the same page
            var abTests = ABCachedObjects.GetTests()
                                         .Where(t => (t.ABTestID != abTest.ABTestID) &&
                                                     (t.ABTestSiteID == abTest.ABTestSiteID) &&
                                                     (t.ABTestOriginalPage == abTest.ABTestOriginalPage) &&
                                                     (t.ABTestOpenFrom != DateTimeHelper.ZERO_TIME));

            // If the test is limited only for one culture, check for tests with the same or no culture restriction
            if (!String.IsNullOrEmpty(abTest.ABTestCulture))
            {
                abTests = abTests.Where(t => (String.IsNullOrEmpty(t.ABTestCulture)) || (t.ABTestCulture == abTest.ABTestCulture));
            }

            // Check schedule collisions
            foreach (var rivalTest in abTests)
            {
                // Return name of the colliding test
                if (Collides(abTest, rivalTest))
                {
                    return rivalTest.ABTestDisplayName;
                }
            }
            return null;
        }


        /// <summary>
        /// Returns true if test schedules collide, false otherwise.
        /// </summary>
        /// <param name="editedTest">Test being edited</param>
        /// <param name="rivalTest">Test being compared</param>
        /// <remarks>This method is internal so it can be accessed in separate assembly for tests.</remarks>
        internal static bool Collides(ABTestInfo editedTest, ABTestInfo rivalTest)
        {
            if (editedTest == null)
            {
                throw new ArgumentNullException("editedTest");
            }

            if (rivalTest == null)
            {
                throw new ArgumentNullException("rivalTest");
            }

            // Test with unfilled open from doesn't collide with other tests
            if (editedTest.ABTestOpenFrom == DateTimeHelper.ZERO_TIME || rivalTest.ABTestOpenFrom == DateTimeHelper.ZERO_TIME)
            {
                return false;
            }

            // Check for collisions (treat tests with unfilled ending dates the same way as never-ending tests)
            var editedTestDuration = new Range<DateTime>(editedTest.ABTestOpenFrom, (editedTest.ABTestOpenTo == DateTimeHelper.ZERO_TIME) ? DateTime.MaxValue : editedTest.ABTestOpenTo);
            var rivalTestDuration = new Range<DateTime>(rivalTest.ABTestOpenFrom, (rivalTest.ABTestOpenTo == DateTimeHelper.ZERO_TIME) ? DateTime.MaxValue : rivalTest.ABTestOpenTo);
            return editedTestDuration.IntersectsWith(rivalTestDuration);
        }
    }
}
