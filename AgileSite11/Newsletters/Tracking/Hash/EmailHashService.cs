using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.Newsletters.Internal;

[assembly: RegisterImplementation(typeof(IEmailHashValidator), typeof(EmailHashService), Priority = CMS.Core.RegistrationPriority.SystemDefault, Name = "IEmailHashValidator_EmailHashService")]
[assembly: RegisterImplementation(typeof(IEmailHashGenerator), typeof(EmailHashService), Priority = CMS.Core.RegistrationPriority.SystemDefault, Name = "IEmailHashGenerator_EmailHashService")]

namespace CMS.Newsletters.Internal
{
    /// <summary>
    /// Provides implementation of <see cref="IEmailHashValidator"/> and <see cref="IEmailHashGenerator"/> for manipulating with validation hash.
    /// </summary>
    public sealed class EmailHashService : IEmailHashValidator, IEmailHashGenerator
    {
        private readonly string mSalt;


        /// <summary>
        /// Initializes new instance of <see cref="EmailHashService"/>.
        /// </summary>
        public EmailHashService()
            :this(ValidationHelper.GetDefaultHashStringSalt())
        {

        }


        /// <summary>
        /// Initializes new instance of <see cref="EmailHashService"/> with provided salt. Only for test purposes.
        /// </summary>
        internal EmailHashService(string salt)
        {
            mSalt = salt;
        }


        /// <summary>
        /// Gets hash for given <paramref name="emailAddress"/>.
        /// </summary>
        /// <param name="emailAddress">Email address tracked link is sent to</param>
        /// <exception cref="ArgumentNullException"><paramref name="emailAddress"/> is null</exception>
        /// <returns>Hash obtained from given input parameters</returns>
        public string GetEmailHash(string emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress");
            }

            return SecurityHelper.GetSHA2Hash(string.Format("{0}{1}", emailAddress, mSalt));
        }


        /// <summary>
        /// Validates given <paramref name="hash"/> against <paramref name="emailAddress"/>.
        /// </summary>
        /// <param name="hash">Hash to be validated</param>
        /// <param name="emailAddress">Email address tracked link is sent to</param>
        /// <exception cref="ArgumentNullException"><paramref name="hash"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="emailAddress"/></exception>
        /// <returns>True, if given <paramref name="hash"/> is valid; otherwise, false</returns>
        public bool ValidateEmailHash(string hash, string emailAddress)
        {
            if (hash == null)
            {
                throw new ArgumentNullException("hash");
            }

            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress");
            }

            return hash == GetEmailHash(emailAddress);
        }
    }
}
