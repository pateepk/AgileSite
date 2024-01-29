using System;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Marks the method or class as one which can disable the license check
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CanDisableLicenseCheckAttribute : Attribute
    {
        /// <summary>
        /// Signature
        /// </summary>
        public string Signature
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="signature">Signature</param>
        public CanDisableLicenseCheckAttribute(string signature)
        {
            Signature = signature;
        }
    }
}