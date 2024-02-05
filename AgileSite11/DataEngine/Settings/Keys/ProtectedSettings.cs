using System;
using System.Collections.Generic;

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Encapsulates settings that are protected from being configurable or read by users with insufficient privileges.
    /// </summary>
    public static class ProtectedSettings
    {
        /// <summary>
        /// Names of the setting that can be configurable or read by users with sufficient privileges.
        /// </summary>
        public static readonly HashSet<string> KeyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Security & Membership
                // General
            "CMSAdminEmailAddress", "CMSMembershipReminder", "CMSDenyLoginInterval", "CMSSiteSharedAccounts", "CMSUseSitePrefixForUserName",

                // Registrations
            "CMSReservedUserNames", "CMSRegistrationEmailConfirmation", "CMSRegistrationApprovalPath", "CMSRegistrationAdministratorApproval",
            "CMSDeleteNonActivatedUserAfter", "CMSUserUniqueEmail",

                // On-line users
            "CMSUseSessionManagement", "CMSSessionUseDBRepository", "CMSSessionManagerSchedulerInterval",     
            
                // Content
            "CMSCheckPagePermissions", "CMSSecuredAreasLogonPage", "CMSAccessDeniedPageURL",   

                // Administration
            "CMSUseSSLForAdministrationInterface", "CMSAutomaticallySignInUser", "CMSEnableCodeEditSiteAdministrators",    
            
                // UI personalization
            "CMSPersonalizeUserInterface",
            
                // Reporting
            "CMSDefaultReportConnectionString",     

            // Security & Membership --> Passwords
                // General
            "CMSSendPasswordEmailsFrom", "CMSPasswordFormat",     
            
                // Password reset
            "CMSResetPasswordURL", "CMSResetPasswordInterval", "CMSSendPasswordResetConfirmation",
            
                // Password expiration
            "CMSPasswordExpiration", "CMSPasswordExpirationPeriod", "CMSPasswordExpirationBehaviour", "CMSPasswordExpirationWarningPeriod", "CMSPasswordExpirationEmail",

                // Password policy
            "CMSUsePasswordPolicy", "CMSPolicyForcePolicyOnLogon", "CMSPolicyMinimalLength", "CMSPolicyNumberOfNonAlphaNumChars",
            "CMSPolicyRegularExpression", "CMSPolicyViolationMessage",

            // Security & Membership --> Protection
                // General
            "CMSDisplayAccountLockInformation", "CMSAutocompleteEnableForLogin",     
            
                // Bad words
            "CMSCheckBadWords", "CMSBadWordsReplacement", "CMSBadWordsAction",
            
                // Invalid sign-in attempts
            "CMSBannedIPEnabled", "CMSBannedIPRedirectURL",

                // Flood protection
            "CMSFloodProtectionEnabled", "CMSFloodInterval",
            
                // CAPTCHA settings
            "CMSCaptchaControl", "CMSReCaptchaPublicKey", "CMSReCaptchaPrivateKey",
            
                // Invalid sign-in attempts
            "CMSMaximumInvalidLogonAttempts", "CMSSendAccountUnlockEmail", "CMSUserAccountUnlockPath",
            
                // Screen lock
            "CMSScreenLockEnabled", "CMSScreenLockInterval", "CMSScreenLockWarningInterval",

            // Integration --> REST
                // General
            "CMSRESTServiceEnabled", "CMSRESTServiceTypeEnabled", "CMSRESTDocumentsSecurityCheck", "CMSRESTDocumentsReadOnly", "CMSRESTObjectsReadOnly",
            "CMSRESTAllowedDocTypes", "CMSRESTAllowedObjectTypes", "CMSRESTGenerateHash", "CMSRESTDefaultEncoding", "CMSRESTAllowSensitiveFields",

            // System
                // Event Log
            "CMSLogSize", "CMSLogMetadata", "CMSLogToDatabase", "CMSLogToFileSystem", "CMSLogToTrace", "CMSUseEventLogListener",


            // Individual settings
                // Content -> Media -> "Media file allowed extensions" setting
            "CMSMediaFileAllowedExtensions",
            
                // System -> Files -> "Upload extensions" setting
            "CMSUploadExtensions"
        };
    }
}
