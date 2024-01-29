using System;

namespace CMS.DataProtection
{
    /// <summary>
    /// Represents a consent which contains text either from <see cref="ConsentInfo"/> or <see cref="ConsentArchiveInfo"/>,
    ///     depending on which version of the consent's text has been agreed by the contact.
    /// </summary>
    public class Consent
    {
        private ConsentText consentText;


        /// <summary>
        /// Consent ID.
        /// </summary>
        public int Id
        {
            get;
            internal set;
        }


        /// <summary>
        /// Consent name.
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }


        /// <summary>
        /// Consent display name.
        /// </summary>
        public string DisplayName
        {
            get;
            internal set;
        }


        /// <summary>
        /// Consent content.
        /// </summary>
        internal string Content
        {
            get;
            set;
        }


        /// <summary>
        /// Consent hash.
        /// </summary>
        public string Hash
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets consent's text for specific <paramref name="cultureCode"/>.
        /// </summary>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <returns>Language specific consent's text.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="cultureCode"/> is null or empty.</exception>
        public ConsentText GetConsentText(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(cultureCode));
            }

            if (consentText == null)
            {
                var manager = new ConsentContentManager(Content);

                consentText = new ConsentText
                {
                    ShortText = manager.GetShortText(cultureCode),
                    FullText = manager.GetFullText(cultureCode)
                };
            }

            return consentText;
        }
    }
}
