using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Protection
{
    /// <summary>
    /// Helper class for bad words.
    /// </summary>
    public static class BadWordsHelper
    {
        #region "Variables"

        private static char? mDefaultReplacement;

        #endregion


        #region "Properties"

        /// <summary>
        /// Default replacement for 'replace' action. It is used when replaced string has greater size than maximum limit.
        /// </summary>
        public static char DefaultReplacement
        {
            get
            {
                if (mDefaultReplacement == null)
                {
                    string value = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSBadWordsReplacementChar"], "*");
                    if (!string.IsNullOrEmpty(value))
                    {
                        mDefaultReplacement = value[0];
                    }
                }

                return mDefaultReplacement.Value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets default action for the bad words from the settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static BadWordActionEnum BadWordsAction(string siteName)
        {
            return (BadWordActionEnum)SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSBadWordsAction");
        }


        /// <summary>
        /// Gets default replacement for the bad words from the settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string BadWordsReplacement(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSBadWordsReplacement");
        }


        /// <summary>
        /// Indicates if the bad words check should be performed.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool PerformBadWordsCheck(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCheckBadWords");
        }


        /// <summary>
        /// Performs bad words check.
        /// </summary>
        /// <param name="obj">GeneralizedInfo info object</param>
        /// <param name="columns">Columns to check (column name and column size). If columns size is zero, column has maximal size and will not checked.</param>
        /// <param name="currentUserId">ID of current user</param>
        /// <param name="validate">Function performing validation of checked object after respective bad words are removed. No further processing is done when validation fails.</param>
        public static string CheckBadWords(GeneralizedInfo obj, Dictionary<string, int> columns, int currentUserId, Func<bool> validate)
        {
            return CheckBadWords(obj, columns, null, null, null, null, currentUserId, validate);
        }


        /// <summary>
        /// Performs bad words check.
        /// </summary>
        /// <param name="obj">GeneralizedInfo info object</param>
        /// <param name="columns">Columns to check (column name and column size). If columns size is zero, column has maximal size and will not checked.</param>
        /// <param name="approvalColumn">Approval column name</param>
        /// <param name="approvalUserColumn">Approval user column name</param>
        /// <param name="currentUserId">ID of current user</param>
        /// <param name="validate">Function performing validation of checked object after respective bad words are removed. No further processing is done when validation fails.</param>
        public static string CheckBadWords(GeneralizedInfo obj, Dictionary<string, int> columns, string approvalColumn, string approvalUserColumn, int currentUserId, Func<bool> validate)
        {
            return CheckBadWords(obj, columns, approvalColumn, approvalUserColumn, null, null, currentUserId, validate);
        }


        /// <summary>
        /// Performs bad words check.
        /// </summary>
        /// <param name="obj">Abstract info object</param>
        /// <param name="columns">Columns to check (column name and column size). If columns size is zero, column has maximal size and will not checked.</param>
        /// <param name="approvalColumn">Approval column name</param>
        /// <param name="approvalUserColumn">Approval user column name</param>
        /// <param name="reportTitle">Abuse report title</param>
        /// <param name="currentUserId">ID of current user</param>
        /// <param name="validate">Function performing validation of checked object after respective bad words are removed. No further processing is done when validation fails.</param>
        public static string CheckBadWords(GeneralizedInfo obj, Dictionary<string, int> columns, string approvalColumn, string approvalUserColumn, string reportTitle, int currentUserId, Func<bool> validate)
        {
            return CheckBadWords(obj, columns, approvalColumn, approvalUserColumn, reportTitle, null, currentUserId, validate);
        }


        /// <summary>
        /// Performs bad words check.
        /// </summary>
        /// <param name="obj">Abstract info object</param>
        /// <param name="columns">Columns to check (column name and column size). If columns size is zero, column has maximal size and will not checked.</param>
        /// <param name="approvalColumn">Approval column name</param>
        /// <param name="approvalUserColumn">Approval user column name</param>
        /// <param name="reportTitle">Abuse report title</param>
        /// <param name="reportURL">Abuse report URL</param>
        /// <param name="currentUserId">ID of current user</param>
        /// <param name="validate">Function performing validation of checked object after respective bad words are removed. No further processing is done when validation fails.</param>
        public static string CheckBadWords(GeneralizedInfo obj, Dictionary<string, int> columns, string approvalColumn, string approvalUserColumn, string reportTitle, string reportURL,  int currentUserId, Func<bool> validate)
        {
            // Limit report title length
            reportTitle = TextHelper.LimitLength(reportTitle, 97);

            // Get current site name
            string siteName = SiteContext.CurrentSiteName;

            // Check if bad words should be checked
            if (!PerformBadWordsCheck(siteName))
            {
                return string.Empty;
            }

            Hashtable foundWords = new Hashtable();
            BadWordActionEnum action = BadWordInfoProvider.CheckAllBadWords(CultureHelper.GetPreferredCulture(), siteName, obj, columns, foundWords);
            string message = string.Empty;
            if ((validate == null) || validate())
            {
                switch (action)
                {
                    case BadWordActionEnum.Deny:
                        message = GenerateWordList(foundWords, action);
                        break;

                    case BadWordActionEnum.RequestModeration:
                        // Set approval column
                        if (approvalColumn != null)
                        {
                            obj.SetValue(approvalColumn, false);
                        }

                        // Clear approved by user ID
                        if (approvalUserColumn != null)
                        {
                            obj.SetValue(approvalUserColumn, DBNull.Value);
                        }
                        break;

                    case BadWordActionEnum.None:
                    case BadWordActionEnum.Remove:
                    case BadWordActionEnum.Replace:
                        break;

                    case BadWordActionEnum.ReportAbuse:
                        if (reportTitle != null)
                        {
                            // Store the object
                            if (obj.ObjectID == 0)
                            {
                                try
                                {
                                    obj.SetObject();
                                }
                                catch (Exception ex)
                                {
                                    message = ex.Message;
                                }
                            }

                            // Create report only if the object containing reported words exists in database
                            if (obj.ObjectID > 0)
                            {
                                // Get default culture
                                string cultureCode = CultureHelper.GetDefaultCultureCode(siteName);

                                var ti = obj.TypeInfo;
                                
                                // Create report
                                AbuseReportInfo report = new AbuseReportInfo();
                                report.ReportStatus = AbuseReportStatusEnum.New;
                                report.ReportComment = GenerateWordList(foundWords, action) + "\n\n" + GetReportComment(obj, columns);
                                report.ReportCulture = CultureHelper.GetPreferredCulture();
                                report.ReportObjectID = obj.ObjectID;
                                report.ReportObjectType = ti.ObjectType;
                                report.ReportWhen = DateTime.Now;
                                report.ReportSiteID = SiteContext.CurrentSiteID;
                                report.ReportTitle = TextHelper.LimitLength(ResHelper.GetAPIString("badwords.ReportTitle", cultureCode, "Bad words in") + " " + ResHelper.GetString(ti.ObjectType, cultureCode) + ResHelper.Colon + " " + reportTitle, 100);
                                report.ReportURL = String.IsNullOrEmpty(reportURL) ? GetOptimizedUrl(ti.ObjectType) : reportURL;
                                report.ReportUserID = currentUserId;
                                AbuseReportInfoProvider.SetAbuseReportInfo(report);
                            }
                        }
                        break;
                }
            }

            return message;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets URL of reported abuse and performs optimizations.
        /// </summary>
        /// <param name="objectType">Type of reported object</param>
        /// <returns>Optimized URL</returns>
        private static string GetOptimizedUrl(string objectType)
        {
            string url = URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL);
            string langCode = URLHelper.GetUrlParameter(url, URLHelper.LanguageParameterName);
            // If no culture is specified
            if (string.IsNullOrEmpty(langCode))
            {
                // Add preferred culture code to query string
                langCode = CultureHelper.GetPreferredCulture();
                url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, langCode);
            }

            // Manipulate with URL depending on object type
            switch (objectType)
            {
                case PredefinedObjectType.FORUMPOST:
                    // Remove 'replyto' parameter from URL
                    url = URLHelper.RemoveParameterFromUrl(url, "replyto");
                    break;
            }
            return url;
        }


        /// <summary>
        /// Gets report comment.
        /// </summary>
        /// <param name="obj">Abstract info object</param>
        /// <param name="columns">Dictionary of columns (column name and size).</param>
        private static string GetReportComment(GeneralizedInfo obj, Dictionary<string, int> columns)
        {
            string comment = string.Empty;

            if ((columns != null) && (columns.Count > 0))
            {
                StringBuilder sb = new StringBuilder();
                string colon = ResHelper.Colon;

                foreach (string columnName in columns.Keys)
                {
                    sb.Append(columnName);
                    sb.Append(colon);
                    sb.Append("\n");
                    sb.Append(obj.GetValue(columnName));
                    sb.Append("\n\n");
                }

                comment = sb.ToString();
            }

            return comment;
        }


        /// <summary>
        /// Generates bad words list.
        /// </summary>
        /// <param name="foundWords">Hashtable with found words</param>
        /// <param name="action">Performed action</param>
        /// <returns>Comma-separated list of found words</returns>
        private static string GenerateWordList(Hashtable foundWords, BadWordActionEnum action)
        {
            bool found = false;
            StringBuilder sb = null;

            if (action != BadWordActionEnum.None)
            {
                // If there are some found bad words
                if (foundWords[action] != null)
                {
                    // Get word hashtable depending on action
                    Hashtable actionHash = (Hashtable)foundWords[action];
                    // If this hashtable exists and is not empty
                    if ((actionHash != null) && (actionHash.Count > 0))
                    {
                        sb = new StringBuilder();

                        // For each expression in hashtable
                        foreach (string expression in actionHash.Keys)
                        {
                            // Add each occurrence to result string
                            ArrayList occurrences = ((ArrayList)actionHash[expression]);
                            foreach (string occurrence in occurrences)
                            {
                                sb.Append(occurrence);
                                sb.Append(", ");
                                found = true;
                            }
                        }
                    }
                    if (found)
                    {
                        // Trim last comma
                        string words = sb.ToString().Trim().TrimEnd(new [] { ',' });

                        // Generate result string
                        return ResHelper.GetAPIString("general.badwordsfound", "The text contains some expressions that are not allowed. Please remove these expressions: ") + " " + words;
                    }
                }
            }
            return string.Empty;
        }

        #endregion
    }
}