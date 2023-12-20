using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Ensures discount code uniqueness in entire application.
    /// </summary>
    public sealed class CodeUniquenessChecker : ICodeUniquenessChecker
    {
        private readonly HashSet<string> mExistingCodes;


        /// <summary>
        /// Creates a new instance of <see cref="CodeUniquenessChecker"/>.
        /// </summary>
        public CodeUniquenessChecker(IEnumerable<string> existingCodes)
        {
            mExistingCodes = new HashSet<string>(existingCodes, ECommerceHelper.CouponCodeComparer);
        }


        /// <summary>
        /// Returns true if given code is unique, false otherwise.
        /// </summary>
        public bool IsUnique(string code)
        {
            return !mExistingCodes.Contains(code);
        }
    }
}
