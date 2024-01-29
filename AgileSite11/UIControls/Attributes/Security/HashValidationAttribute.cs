using System;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Does hash validation and performs redirect to Access denied page if validation fails. Works only with pages inherited from <see cref="CMSPage"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HashValidationAttribute : AbstractAttribute, ICMSSecurityAttribute
    {
        #region "Variables"

        string mName = "hash";
        bool mValidateWithoutParameters = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of the query parameter which contains hash value. 
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// Specifies query parameters which are excluded from hash counting. Parameters are separated by semicolon.
        /// </summary>
        public string ExcludedParameters
        {
            get;
            set;
        }

        
        /// <summary>
        /// Whether should be hash validated with excluded parameters if without them validation fails.
        /// </summary>
        public bool ValidateWithoutParameters
        {
            get
            {
                return mValidateWithoutParameters;
            }
            set
            {
                mValidateWithoutParameters = value;
            }
        }


        /// <summary>
        /// Hash salt.
        /// </summary>
        public string HashSalt
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hashSalt">Hash salt.</param>
        public HashValidationAttribute(string hashSalt)
        {
            HashSalt = hashSalt;
        }

        #endregion


        #region "ICMSSecurityAttribute Members"

        /// <summary>
        /// Does hash validation and performs redirect to Access denied page if validation fails.
        /// </summary>
        /// <param name="page">Page from which is check performed</param>
        public void Check(CMSPage page)
        {
            if (!page.CheckHashValidationAttribute)
            {
                return;
            }

            var settings = new HashSettings
            {
                Redirect = true,
                HashSalt = HashSalt
            };

            QueryHelper.ValidateHash(Name, ExcludedParameters, settings, ValidateWithoutParameters);
        }

        #endregion
    }
}
