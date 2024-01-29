using System;
using System.Linq;
using System.Text;

namespace CMS.Tests
{
    /// <summary>
    /// Indicates that test was created due to bug fix.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RelatedBugAttribute : Attribute
    {
        private string mBugIdentifier;
        private bool mIsPublic;


        /// <summary>
        /// Indicates that test was created due to bug fix.
        /// </summary>
        /// <param name="bugIdentifier">Bug identifier (eg. issue number)</param>
        /// <param name="isPublic">True if bug is known publicly</param>
        public RelatedBugAttribute(string bugIdentifier, bool isPublic = true)
        {
            mBugIdentifier = bugIdentifier;
            mIsPublic = isPublic;
        }
    }
}
