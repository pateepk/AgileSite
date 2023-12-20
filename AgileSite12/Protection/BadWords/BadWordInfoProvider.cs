using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.Protection
{
    using BadWordList = List<BadWordInfo>;
    using BadWordTable = SafeDictionary<string, List<BadWordInfo>>;
    using TypedDataSet = InfoDataSet<BadWordInfo>;


    /// <summary>
    /// Class providing BadWordInfo management.
    /// </summary>
    public class BadWordInfoProvider : AbstractInfoProvider<BadWordInfo, BadWordInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Remove action.
        /// </summary>
        public const string REMOVE = "REMOVE";

        /// <summary>
        /// Replace action.
        /// </summary>
        public const string REPLACE = "REPLACE";

        /// <summary>
        /// Report abuse action.
        /// </summary>
        public const string REPORT = "REPORT";

        /// <summary>
        /// Moderation action.
        /// </summary>
        public const string MODERATION = "MODERATION";

        /// <summary>
        /// Deny action.
        /// </summary>
        public const string DENY = "DENY";


        private const string CLEAR_ACTION_NAME = "clearbadwords";

        #endregion


        #region "Variables

        /// <summary>
        /// Culture specified bad words provider dictionary.
        /// </summary>
        private static CMSStatic<BadWordTable> mCultureBadWords = new CMSStatic<BadWordTable>();

        /// <summary>
        /// Global bad words list.
        /// </summary>
        private static CMSStatic<BadWordList> mGlobalBadWords = new CMSStatic<BadWordList>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Culture specified bad words provider dictionary.
        /// </summary>
        private static BadWordTable CultureBadWords
        {
            get
            {
                return mCultureBadWords;
            }
            set
            {
                mCultureBadWords.Value = value;
            }
        }


        /// <summary>
        /// Global bad words list.
        /// </summary>
        private static BadWordList GlobalBadWords
        {
            get
            {
                return mGlobalBadWords;
            }
            set
            {
                mGlobalBadWords.Value = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Determines whether user can use bad words.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="siteName">Site name</param>
        /// <returns>True if user can use bad words.</returns>
        public static bool CanUseBadWords(IUserInfo user, string siteName)
        {
            return (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || user.IsAuthorizedPerResource("CMS.Badwords", "UseBadWords", siteName, false));
        }


        /// <summary>
        /// Checks whether bad word exists.
        /// </summary>
        /// <param name="badWord">Bad word</param>
        /// <returns>True if bad word exists.</returns>
        public static bool BadWordExists(string badWord)
        {
            return ProviderObject.BadWordExistsInternal(badWord);
        }


        /// <summary>
        /// Returns all bad words.
        /// </summary>
        public static ObjectQuery<BadWordInfo> GetBadWords()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the BadWordInfo structure for the specified badWord.
        /// </summary>
        /// <param name="badWordId">Bad word ID</param>
        public static BadWordInfo GetBadWordInfo(int badWordId)
        {
            return ProviderObject.GetBadWordInfoInternal(badWordId);
        }


        /// <summary>
        /// Returns the BadWordInfo structure for the specified bad word GUID.
        /// </summary>
        /// <param name="wordGuid">Bad word GUID</param>
        /// <returns>BadWord info object</returns>
        public static BadWordInfo GetBadWordInfo(Guid wordGuid)
        {
            return ProviderObject.GetBadWordInfoByGuidInternal(wordGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified badWord.
        /// </summary>
        /// <param name="badWord">BadWord to set</param>
        public static void SetBadWordInfo(BadWordInfo badWord)
        {
            ProviderObject.SetBadWordInfoInternal(badWord);
        }


        /// <summary>
        /// Deletes specified badWord and updates hashtables.
        /// </summary>
        /// <param name="infoObj">BadWord object</param>
        public static void DeleteBadWordInfo(BadWordInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified badWord and clears hashtables.
        /// </summary>
        /// <param name="wordId">Bad word ID</param>
        public static void DeleteBadWordInfo(int wordId)
        {
            BadWordInfo infoObj = GetBadWordInfo(wordId);
            DeleteBadWordInfo(infoObj);
        }


        /// <summary>
        /// Checks given text for given bad word.
        /// </summary>
        /// <param name="word">BadWordInfo object</param>
        /// <param name="cultureCode">Code of culture</param>
        /// <param name="siteName">Site name</param>
        /// <param name="text">Text to check</param>
        /// <param name="foundWords">Found words</param>
        /// <param name="maxTextLength">Maximum length of the text to be checked. If value is zero, text length is unlimited.</param>
        /// <returns>What action should be performed and modified text through the text parameter.</returns>
        public static BadWordActionEnum CheckBadWord(BadWordInfo word, string cultureCode, string siteName, ref string text, Hashtable foundWords, int maxTextLength)
        {
            // Load global bad words
            LoadBadWords();
            if (cultureCode != null)
            {
                // Load bad words for specified culture
                cultureCode = cultureCode.ToLowerCSafe();
                LoadBadWords(cultureCode);
            }

            // Perform bad word check
            return PerformCheck(word, ref text, foundWords, siteName, maxTextLength);
        }


        /// <summary>
        /// Checks given text for all bad words.
        /// </summary>
        /// <param name="cultureCode">Code of culture</param>
        /// <param name="siteName">Site name</param>
        /// <param name="infoObject">Object info</param>
        /// <param name="columns">Columns to check (column name and column size). If columns size is zero, column has maximal size and will not checked.</param>
        /// <param name="foundWords">Found words</param>
        /// <returns>What action should be performed and modified text through the text parameter.</returns>
        public static BadWordActionEnum CheckAllBadWords(string cultureCode, string siteName, GeneralizedInfo infoObject, Dictionary<string, int> columns, Hashtable foundWords)
        {
            BadWordActionEnum action = BadWordActionEnum.None;
            // If there are some data with columns
            if ((infoObject != null) && (columns != null) && (columns.Count > 0))
            {
                // Perform check for all columns
                foreach (KeyValuePair<string, int> item in columns)
                {
                    string columnName = item.Key;
                    int columnSize = item.Value;

                    // Get column value
                    string currentValue = ValidationHelper.GetString(infoObject.GetValue(columnName), string.Empty);

                    // Perform bad words check
                    BadWordActionEnum currentAction = CheckAllBadWords(cultureCode, siteName, ref currentValue, foundWords, columnSize);

                    // Store changed column value
                    infoObject.SetValue(columnName, currentValue);

                    // Get worse case of the action
                    if (currentAction > action)
                    {
                        action = currentAction;
                    }
                }
            }
            return action;
        }


        /// <summary>
        /// Checks given text for all bad words.
        /// </summary>
        /// <param name="cultureCode">Code of culture</param>
        /// <param name="siteName">Site name</param>
        /// <param name="text">Text to check</param>
        /// <param name="foundWords">Found words</param>
        /// <returns>What action should be performed and modified text through the text parameter.</returns>
        public static BadWordActionEnum CheckAllBadWords(string cultureCode, string siteName, ref string text, Hashtable foundWords)
        {
            return CheckAllBadWords(cultureCode, siteName, ref text, foundWords, 0);
        }


        /// <summary>
        /// Checks given text for all bad words.
        /// </summary>
        /// <param name="cultureCode">Code of culture</param>
        /// <param name="siteName">Site name</param>
        /// <param name="text">Text to check</param>
        /// <param name="foundWords">Found words</param>
        /// <param name="maxTextLength">Maximum length of the text to be checked. If value is zero, text length is unlimited.</param>
        /// <returns>What action should be performed and modified text through the text parameter.</returns>
        public static BadWordActionEnum CheckAllBadWords(string cultureCode, string siteName, ref string text, Hashtable foundWords, int maxTextLength)
        {
            string originalText = text;
            // Check bad words
            BadWordActionEnum action = CheckWords(cultureCode, siteName, ref text, foundWords, maxTextLength);

            // Return original text
            if (action == BadWordActionEnum.Deny)
            {
                text = originalText;
            }

            // Log to event log
            if (action != BadWordActionEnum.None)
            {
                StringBuilder description = new StringBuilder();
                // Add result to description
                description.Append("Result: " + action + "\n\n");

                // Add culture code to description
                if (cultureCode != null)
                {
                    description.Append("Checked culture: " + cultureCode + "\n\n");
                }

                // If some bad words were found
                if (foundWords != null)
                {
                    if (foundWords.Keys.Count > 0)
                    {
                        description.Append("Found words:\n");
                        // Add each bad word to description
                        foreach (BadWordActionEnum type in foundWords.Keys)
                        {
                            // Get hashtable for each action type
                            Hashtable exHash = (Hashtable)foundWords[type];
                            // If this hashtable is not empty
                            if ((exHash != null) && (exHash.Keys.Count > 0))
                            {
                                description.Append("\t" + type + ": ");
                                foreach (string expression in exHash.Keys)
                                {
                                    ArrayList occurrences = ((ArrayList)exHash[expression]);
                                    // Add each occurrence to description
                                    foreach (string occurrence in occurrences)
                                    {
                                        description.Append(occurrence + ", ");
                                    }
                                    // Remove last ', '
                                    description.Remove(description.Length - 2, 2);
                                    description.Append("\n");
                                }
                            }
                        }
                        description.Append("\n");
                    }
                }

                // Add original text to description
                description.Append("Original text:\n" + originalText + "\n\n");

                // If text was modified add it to description
                if ((action == BadWordActionEnum.Remove) || (action == BadWordActionEnum.Replace))
                {
                    description.Append("Modified text:\n" + text);
                }

                // Get site ID
                int siteId = 0;
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    siteId = si.SiteID;
                }

                // Log event to event log
                EventLogProvider.LogEvent(EventType.INFORMATION, "Bad words check", "BADWORD", description.ToString(), RequestContext.RawURL, 0, null, 0, null, RequestContext.UserHostAddress, siteId);
            }

            return action;
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            // Clear global hashtable
            if (GlobalBadWords != null)
            {
                GlobalBadWords = null;
            }

            // Clear culture related hashtable
            if (CultureBadWords != null)
            {
                CultureBadWords = null;
            }

            // Create webfarm task if needed
            if (logTasks)
            {
                CreateWebFarmTask(CLEAR_ACTION_NAME, String.Empty);
            }
        }


        /// <summary>
        /// Returns all bad words for given culture.
        /// </summary>
        /// <param name="cultureId">ID of culture</param>
        public static TypedDataSet GetBadWords(int cultureId)
        {
            return ProviderObject.GetBadWordsInternal(cultureId);
        }

        #endregion


        #region "Private static methods"

        /// <summary>
        /// Check bad words and returns action.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        /// <param name="text">Checked text</param>
        /// <param name="foundWords">Found words</param>
        /// <param name="maxTextLength">Maximum length of the text to be checked. If value is zero, text length is unlimited.</param>
        private static BadWordActionEnum CheckWords(string cultureCode, string siteName, ref string text, Hashtable foundWords, int maxTextLength)
        {
            BadWordActionEnum action = BadWordActionEnum.None;

            // Load global bad words
            LoadBadWords();
            if (cultureCode != null)
            {
                // Load bad words for specified culture
                cultureCode = cultureCode.ToLowerCSafe();
                LoadBadWords(cultureCode);
            }

            // Check all global bad words
            if (GlobalBadWords != null)
            {
                foreach (BadWordInfo badWord in GlobalBadWords)
                {
                    BadWordActionEnum currentAction = CheckBadWord(badWord, null, siteName, ref text, foundWords, maxTextLength);
                    // Set worse case of the action
                    if (currentAction > action)
                    {
                        action = currentAction;
                    }
                }
            }

            // Check bad words for given culture
            if (cultureCode != null)
            {
                if (CultureBadWords != null)
                {
                    foreach (BadWordInfo badWord in CultureBadWords[cultureCode])
                    {
                        BadWordActionEnum currentAction = CheckBadWord(badWord, cultureCode, siteName, ref text, foundWords, maxTextLength);
                        // Set worse case of the action
                        if (currentAction > action)
                        {
                            action = currentAction;
                        }
                    }
                }
            }

            return action;
        }


        /// <summary>
        /// Performes bad word check.
        /// </summary>
        /// <param name="badWord">Bad word to check</param>
        /// <param name="text">Text to check</param>
        /// <param name="foundWords">Hash table of found words</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="maxTextLength">Maximum length of the text to be checked. If value is zero, text length is unlimited.</param>
        private static BadWordActionEnum PerformCheck(BadWordInfo badWord, ref string text, Hashtable foundWords, string siteName, int maxTextLength)
        {
            // If bad word info object exists
            if (badWord != null)
            {
                // Check if action should be inherited from global settings
                BadWordActionEnum action = (badWord.WordAction == BadWordActionEnum.None) ? BadWordsHelper.BadWordsAction(siteName) : badWord.WordAction;

                // Check if replacement should be inherited from global settings
                string badWordReplacement = string.Empty;
                if (action == BadWordActionEnum.Replace)
                {
                    badWordReplacement = badWord.WordReplacement ?? BadWordsHelper.BadWordsReplacement(siteName);
                }

                string expression = badWord.WordExpression;

                // If bad word expression is not regular expression
                if (!badWord.WordIsRegularExpression)
                {
                    // Escape expression
                    expression = Regex.Escape(expression);
                }

                // Check if match whole word
                if (badWord.WordMatchWholeWord)
                {
                    expression = "\\b" + expression + "\\b";
                }

                ArrayList list = null;
                switch (action)
                {
                    case BadWordActionEnum.Remove:
                    case BadWordActionEnum.Replace:
                        // Get replacement
                        string replacement = string.Empty;

                        if (action == BadWordActionEnum.Replace)
                        {
                            // Use bad word replacement
                            replacement = badWordReplacement;
                        }

                        // Perform replace action (get list of found words)
                        list = Replace(expression, replacement, ref text, maxTextLength);

                        // If performed action found some words
                        if (list != null)
                        {
                            if (foundWords[action] == null)
                            {
                                foundWords[action] = new Hashtable();
                            }
                            // Fill hashtable with found words
                            ((Hashtable)foundWords[action])[badWord.WordExpression] = list;
                        }
                        else
                        {
                            // No action was performed
                            action = BadWordActionEnum.None;
                        }
                        break;

                    case BadWordActionEnum.ReportAbuse:
                    case BadWordActionEnum.RequestModeration:
                    case BadWordActionEnum.Deny:
                        // Remove bad word
                        list = Find(expression, text);

                        // If performed action found some words
                        if (list != null)
                        {
                            if (foundWords != null)
                            {
                                if (foundWords[action] == null)
                                {
                                    foundWords[action] = new Hashtable();
                                }
                                // Fill hashtable with found words
                                ((Hashtable)foundWords[action])[badWord.WordExpression] = list;
                            }
                        }
                        else
                        {
                            action = BadWordActionEnum.None;
                        }
                        break;
                }
                return action;
            }
            return BadWordActionEnum.None;
        }


        /// <summary>
        /// Replaces expression with replacement in given text.
        /// </summary>
        /// <param name="expression">Expression to be replaced</param>
        /// <param name="replacement">String replacement</param>
        /// <param name="text">Text in which the given expression is being searched</param>
        /// <param name="maxTextLength">Maximum length of the text to be checked. If value is zero, text length is unlimited.</param>
        /// <returns>ArrayList with found occurrences of expression</returns>
        private static ArrayList Replace(string expression, string replacement, ref string text, int maxTextLength)
        {
            // Localize replacement
            replacement = ResHelper.LocalizeString(replacement);

            // Regular expression replacement
            Regex re = RegexHelper.GetRegex(expression, true);
            ArrayList foundWords = Find(re, text);

            string textToReplace = text;
            long charsCount = 0;

            // Create match evaluator to process expression replacement
            var matchEvaluator = new MatchEvaluator(
                match =>
                {
                    // Length of the text is limited
                    if (maxTextLength > 0)
                    {
                        // Get found word
                        string foundWord = match.Value;

                        // If length of a found word is smaller than length of the replacement, replacement can exceed the maximal text length
                        int diff = (replacement.Length - foundWord.Length);
                        if (diff > 0)
                        {
                            // Keep additional characters count
                            charsCount += diff;
                            // Text length exceeds the maximum length, use default replacement
                            if ((charsCount + textToReplace.Length) > maxTextLength)
                            {
                                return new string(BadWordsHelper.DefaultReplacement, foundWord.Length);
                            }
                        }
                    }

                    return replacement;
                });

            // Ref return edited text
            text = re.Replace(textToReplace, matchEvaluator);

            return foundWords;
        }


        /// <summary>
        /// Finds occurrences of string in given text.
        /// </summary>
        /// <param name="expression">Expression to find</param>
        /// <param name="text">Text to search in</param>
        /// <returns>ArrayList with found occurrences</returns>
        private static ArrayList Find(string expression, string text)
        {
            Regex re = RegexHelper.GetRegex(expression, true);
            return Find(re, text);
        }


        /// <summary>
        /// Finds occurrences of regular expression in given text.
        /// </summary>
        /// <param name="regularExpression">Regular expression to find</param>
        /// <param name="text">Text to search in</param>
        /// <returns>ArrayList with found occurrences</returns>
        private static ArrayList Find(Regex regularExpression, string text)
        {
            ArrayList foundWords = null;

            // Find every occurrence of regular expression in text
            foreach (Match match in regularExpression.Matches(text))
            {
                if (foundWords == null)
                {
                    foundWords = new ArrayList();
                }
                // Add occurrence to ArrayList with found words
                foundWords.Add(match.Value);
            }
            return foundWords;
        }


        /// <summary>
        /// Loads the values to the hash tables.
        /// </summary>
        private static void LoadBadWords()
        {
            LoadBadWords(null);
        }


        /// <summary>
        /// Loads the values to the culture hash table.
        /// </summary>
        /// <param name="cultureCode">Code of culture</param>
        private static void LoadBadWords(string cultureCode)
        {
            // Load global bad words
            if (cultureCode == null)
            {
                // If ArrayList with global bad words doesn't exist
                if (GlobalBadWords == null)
                {
                    // Create new list of bad words
                    List<BadWordInfo> mBadWords = new List<BadWordInfo>();

                    // Get global bad words
                    ProviderObject.GetObjectQuery().WhereEquals("WordIsGlobal", 1).ForEachObject(
                        i => mBadWords.Add(i)
                    );

                    // Copy internal list to GlobalBadWords list
                    GlobalBadWords = mBadWords;
                }
            }
            // Load culture specified bad words
            else
            {
                cultureCode = cultureCode.ToLowerCSafe();
                // Create new SafeDictionary if doesn't exist
                if (CultureBadWords == null)
                {
                    CultureBadWords = new SafeDictionary<string, List<BadWordInfo>>();
                }
                // If SafeDictionary doesn't contain ArrayList for specific culture key
                if (CultureBadWords[cultureCode] == null)
                {
                    // Create new list of bad words
                    List<BadWordInfo> mBadWords = new List<BadWordInfo>();

                    // Get the badwords for given culture
                    CultureInfo ci = CultureInfoProvider.GetCultureInfo(cultureCode);
                    // If culture exists
                    if (ci != null)
                    {
                        // Get bad words for given culture
                        DataSet ds = GetBadWords(ci.CultureID);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            // Add each bad word info to ArrayList
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                BadWordInfo bwi = new BadWordInfo(row);
                                mBadWords.Add(bwi);
                            }
                        }
                        // Add list with specified culture to SafeDictionary
                        CultureBadWords[cultureCode] = mBadWords;
                    }
                }
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns all bad words for given culture.
        /// </summary>
        /// <param name="cultureId">ID of culture</param>
        /// <returns>DataSet with bad words</returns>
        protected virtual TypedDataSet GetBadWordsInternal(int cultureId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@CultureID", cultureId);
            parameters.EnsureDataSet<BadWordInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("badwords.word.selectbycultureid", parameters).As<BadWordInfo>();
        }


        /// <summary>
        /// Returns true if bad word exists.
        /// </summary>
        /// <param name="badWord">Bad word</param>
        protected virtual bool BadWordExistsInternal(string badWord)
        {
            // Check whether bad word exists
            return GetObjectQuery().WhereEquals("WordExpression", badWord).HasResults();
        }


        /// <summary>
        /// Returns the BadWordInfo structure for the specified badWord.
        /// </summary>
        /// <param name="badWordId">BadWord ID</param>
        protected virtual BadWordInfo GetBadWordInfoInternal(int badWordId)
        {
            return GetObjectQuery().WhereEquals("WordID", badWordId).FirstOrDefault();
        }


        /// <summary>
        /// Returns the BadWordInfo structure for the specified GUID.
        /// </summary>
        /// <param name="guid">Bad word GUID</param>
        protected virtual BadWordInfo GetBadWordInfoByGuidInternal(Guid guid)
        {
            return GetObjectQuery().WhereEquals("WordGUID", guid).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified BadWord object.
        /// </summary>
        /// <param name="badWord">BadWord object to set</param>
        protected virtual void SetBadWordInfoInternal(BadWordInfo badWord)
        {
            if (badWord != null)
            {
                // Update or insert object to database
                if (badWord.WordID > 0)
                {
                    badWord.Generalized.UpdateData();
                }
                else
                {
                    badWord.Generalized.InsertData();
                }

                // Clear hashtables
                ClearHashtables(badWord.Generalized.LogWebFarmTasks);
            }
            else
            {
                throw new Exception("[BadWordInfoProvider.SetBadWordInfo]: No BadWordInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(BadWordInfo info)
        {
            if (info != null)
            {
                // Delete object from database
                base.DeleteInfo(info);

                // Clear hashtables
                ClearHashtables(info.Generalized.LogWebFarmTasks);
            }
        }

        #endregion


        #region "Web farm"

        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            // Switch by action name
            switch (actionName)
            {
                // Clear bad words hash tables
                case CLEAR_ACTION_NAME:
                    ClearHashtables(false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}