using System;

namespace CMS.Tests
{
    /// <summary>
    /// Generic base class for test extender
    /// </summary>
    /// <typeparam name="TParentTest">Type of parent test. This type specifies on which test level the extender should be applied</typeparam>
    public abstract class TestExtender<TParentTest> : TestExtender
        where TParentTest : AutomatedTests
    {
        /// <summary>
        /// Get test which is being extended.
        /// </summary>
        public TParentTest Test
        {
            get;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="test">Test being extended by extender</param>
        protected TestExtender(TParentTest test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            Test = test;
        }
    }


    /// <summary>
    /// Base class for test extenders
    /// </summary>
    public abstract class TestExtender
    {
        /// <summary>
        /// Milestone action
        /// </summary>
        public Action Milestone
        {
            get;
            set;
        }


        /// <summary>
        /// Test fixture setup
        /// </summary>
        public virtual void FixtureSetUp()
        {
        }


        /// <summary>
        /// Test setup
        /// </summary>
        public virtual void SetUp()
        {
        }


        /// <summary>
        /// Test fixture tear down
        /// </summary>
        public virtual void FixtureTearDown()
        {
        }


        /// <summary>
        /// Test tear down
        /// </summary>
        public virtual void TearDown()
        {
        }
        

        /// <summary>
        /// Test milestone
        /// </summary>
        internal virtual void TestMilestone()
        {
            Milestone?.Invoke();
        }
    }
}