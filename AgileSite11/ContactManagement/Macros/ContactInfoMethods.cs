using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Activities;
using CMS.Activities.Internal;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Core.Internal;
using CMS.ContactManagement.Internal;

[assembly: RegisterExtension(typeof(ContactInfoMethods), typeof(ContactInfo))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Online marketing methods - wrapping methods for macro resolver.
    /// </summary>
    internal class ContactInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if the contact is in any/all of the specified roles (i.e. if any of the user assigned to the contact is in any/all specified roles).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact is in any/all of the specified roles (i.e. if any of the user assigned to the contact is in any/all specified roles).", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "roleGuids", typeof (string), "Guids of the roles separated with semicolon.")]
        [MacroMethodParam(2, "allRoles", typeof (bool), "If true, contact has to in all specified roles. If false, it is sufficient if the contact is at least in one of the roles.")]
        public static object IsInRoles(EvaluationContext context, params object[] parameters)
        {
            ICollection<Guid> guids = null;
            if (parameters.Length > 1)
            {
                string roleGuids = ValidationHelper.GetString(parameters[1], "");
                if (!MacroValidationHelper.TryParseGuids(ValidationHelper.GetString(parameters[1], ""), out guids))
                {
                    MacroValidationHelper.LogInvalidGuidParameter("IsInRoles", roleGuids);
                    return false;
                }
            }

            switch (parameters.Length)
            {
                case 2:
                    return IsInRoles(parameters[0], guids, false);

                case 3:
                    return IsInRoles(parameters[0], guids, ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact is in any/all of the specified community groups (i.e. if any of the user assigned to the contact is in any/all specified community groups).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact is in any/all of the specified community groups (i.e. if any of the user assigned to the contact is in any/all specified community groups).", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "groupGuids", typeof (string), "GUIDs of the groups separated with semicolon.")]
        [MacroMethodParam(2, "allGroups", typeof (bool), "If true, contact has to in all specified groups. If false, it is sufficient if the contact is at least in one of the groups.")]
        public static object IsInCommunityGroup(EvaluationContext context, params object[] parameters)
        {
            ICollection<Guid> guids = null;
            if (parameters.Length > 1)
            {
                string groupGuids = ValidationHelper.GetString(parameters[1], "");
                if (!MacroValidationHelper.TryParseGuids(groupGuids, out guids))
                {
                    MacroValidationHelper.LogInvalidGuidParameter("IsInCommunityGroup", groupGuids);
                    return false;
                }
            }

            switch (parameters.Length)
            {
                case 2:
                    return IsInCommunityGroup(parameters[0], guids, false);

                case 3:
                    return IsInCommunityGroup(parameters[0], guids, ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns contact's last activity of specified activity type.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (ActivityInfo), "Returns contact's last activity of specified activity type.", 1)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "activityType", typeof (string), "Name of the activity type, optional.")]
        public static object LastActivityOfType(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return LastActivityOfType(parameters[0], null);

                case 2:
                    return LastActivityOfType(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns contact's first activity of specified activity type.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (ActivityInfo), "Returns contact's first activity of specified activity type.", 1)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "activityType", typeof (string), "Name of the activity type, optional.")]
        public static object FirstActivityOfType(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return FirstActivityOfType(parameters[0], null);

                case 2:
                    return FirstActivityOfType(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns TRUE if the contact is in specified contact group on current site.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if contact is in contact group.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "groupNames", typeof (string), "Name of the contact group(s) separated by semicolon to test whether contact is in.")]
        [MacroMethodParam(2, "allGroups", typeof (bool), "If true contact has to be in all specified groups, if false, it is sufficient if the contact is in any of the specified groups.")]
        public static object IsInContactGroup(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return IsInContactGroup(parameters[0], parameters[1], false);

                case 3:
                    return IsInContactGroup(parameters[0], parameters[1], ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns contact's points in specified score on current site.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (int), "Returns contact's points in specified score on current site.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "scoreName", typeof (string), "Name of the score to get contact's points of.")]
        public static object GetScore(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return GetScore(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }



        /// <summary>
        /// Returns true if contact belongs to specified account.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact belongs to the specified account.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "accountID", typeof (int), "ID of the account.")]
        public static object BelongsToAccount(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return BelongsToAccount(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact did any/all of the specified activities.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact did any/all of the specified activities.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "activityTypes", typeof (string), "Name of the activity(ies) to check separated with semicolon.")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        [MacroMethodParam(3, "allActivities", typeof (string), "If true, all specified types has to be found for the method to return true. If false, at least one of any specified types is sufficient to return true for the method.")]
        public static object DidActivities(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return DidActivities(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0, false);

                case 3:
                    return DidActivities(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0), false);

                case 4:
                    return DidActivities(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetBoolean(parameters[3], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if contact belongs to specified account.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact did a specified activity.", 1)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "activityType", typeof (string), "Name of the activity to check.")]
        [MacroMethodParam(2, "cancelActivityType", typeof (string), "Name of the activity which cancels the original activity (for example UnsubscribeNewsletter is a canceling event for SubscribeNewsletter etc.).")]
        [MacroMethodParam(3, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        [MacroMethodParam(4, "whereCondition", typeof (string), "Additional WHERE condition.")]
        public static object DidActivity(EvaluationContext context, params object[] parameters)
        {
            var contactInfo = parameters[0] as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            switch (parameters.Length)
            {
                case 1:
                    return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, null, null, 0, null);

                case 2:
                    return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, ValidationHelper.GetString(parameters[1], ""), null, 0, null);

                case 3:
                    return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""), 0, null);

                case 4:
                    return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""), ValidationHelper.GetInteger(parameters[3], 0), null);

                case 5:
                    return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetString(parameters[4], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if contact came to specified landing page.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact came to specified landing page.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "page", typeof (string), "Node ID or node alias path of the landing page.")]
        public static object CameToLandingPage(EvaluationContext context, params object[] parameters)
        {
            var contactInfo = parameters[0] as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            switch (parameters.Length)
            {
                case 2:
                    int nodeId = ValidationHelper.GetInteger(parameters[1], 0);
                    string nodeIds = null;
                    if (nodeId <= 0)
                    {
                        string alias = ValidationHelper.GetString(parameters[1], "");
                        if (!string.IsNullOrEmpty(alias))
                        {
                            var ds = new TreeProvider().SelectNodes(TreeProvider.ALL_SITES, alias, TreeProvider.ALL_CULTURES, true);
                            if (!DataHelper.DataSourceIsEmpty(ds))
                            {
                                nodeIds = TextHelper.Join(",", DataHelper.GetStringValues(ds.Tables[0], "NodeID"));
                            }
                        }
                    }

                    if (nodeId > 0)
                    {
                        return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, "landingpage", null, 0, "ActivityNodeID = " + nodeId);
                    }

                    if (!string.IsNullOrEmpty(nodeIds))
                    {
                        return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, "landingpage", null, 0, "ActivityNodeID IN (" + nodeIds + ")");
                    }

                    return false;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact came to the specified landing page with the specified URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact came to the specified landing page with the specified URL.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "url", typeof(string), "Page URL to be checked")]
        public static object CameToLandingUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return VisitedURL(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0, true);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact's country matches one of the specified countries.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact's country matches one of the specified countries.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "countryList", typeof (string), "List of country names separated with semicolon.")]
        public static object IsFromCountry(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return IsFromCountry(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact's state matches one of the specified countries.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact's state matches one of the specified state.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "stateList", typeof (string), "List of state names separated with semicolon.")]
        public static object IsFromState(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return IsFromState(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact did search for specified keywords in last X days.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact did search for specified keywords in last X days.", 1)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "keywords", typeof (string), "Keywords separated with comma or semicolon (if null or empty, than method returns true if contact did any search in last X days).")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        [MacroMethodParam(3, "op", typeof (string), "Operator to be used in WHERE condition (use AND for all keywords match, use OR for any keyword match).")]
        public static object SearchedForKeywords(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return SearchedForKeywords(parameters[0], null, 0, false);

                case 2:
                    return SearchedForKeywords(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0, false);

                case 3:
                    return SearchedForKeywords(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0), false);

                case 4:
                    return SearchedForKeywords(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetBoolean(parameters[3], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact spent specified amount of money (counted by order total price in main currency) in last X days.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact spent specified amount of money (counted by order total price in main currency) in last X days.", 1)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "lowerBound", typeof (decimal), "Inclusive lower bound of the amount of money (in main currency) spent by contact.")]
        [MacroMethodParam(2, "upperBound", typeof (decimal), "Inclusive upper bound of the amount of money (in main currency) spent by contact.")]
        [MacroMethodParam(3, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object SpentMoney(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return SpentMoney(parameters[0], ValidationHelper.GetDecimal(parameters[1], 0), ValidationHelper.GetDecimal(parameters[2], 0), 0);

                case 4:
                    return SpentMoney(parameters[0], ValidationHelper.GetDecimal(parameters[1], 0), ValidationHelper.GetDecimal(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact purchased specified number of products in last X days.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact purchased specified number of products in last X days.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "lowerBound", typeof (int), "Inclusive lower bound of the number of products bought.")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object PurchasedNumberOfProducts(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return PurchasedNumberOfProducts(parameters[0], ValidationHelper.GetInteger(parameters[1], 0), 0);

                case 3:
                    return PurchasedNumberOfProducts(parameters[0], ValidationHelper.GetInteger(parameters[1], 0), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact visited specified page.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact visited specified page.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "nodeGUID", typeof (Guid), "Page node GUID.")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object VisitedPage(EvaluationContext context, params object[] parameters)
        {
            Guid guid = Guid.Empty;
            if (parameters.Length > 1)
            {
                string nodeGuid = ValidationHelper.GetString(parameters[1], "");
                if (!Guid.TryParse(nodeGuid, out guid))
                {
                    MacroValidationHelper.LogInvalidGuidParameter("VisitedPage", nodeGuid);
                    return false;
                }
            }

            switch (parameters.Length)
            {
                case 2:
                    return VisitedPage(parameters[0], guid, null, 0);

                case 3:
                    return VisitedPage(parameters[0], guid, null, ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact visited specified site.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact visited specified site.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "siteName", typeof(string), "Name of the site which should be checked.")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object VisitedSite(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return VisitedSite(parameters[0], ValidationHelper.GetString(parameters[1], string.Empty), 0);

                case 3:
                    return VisitedSite(parameters[0], ValidationHelper.GetString(parameters[1], string.Empty), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact visited page containing specified URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact visited page containing specified URL.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "url", typeof(string), "Page URL to be checked")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object VisitedURL(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return VisitedURL(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0);

                case 3:
                    return VisitedURL(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact opened specified newsletter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact opened specified newsletter.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "newsletterGuid", typeof (Guid), "Newsletter guid.")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object OpenedNewsletter(EvaluationContext context, params object[] parameters)
        {
            Guid guid = Guid.Empty;
            if (parameters.Length > 1)
            {
                string newsletterGuid = ValidationHelper.GetString(parameters[1], "");
                if (!Guid.TryParse(newsletterGuid, out guid))
                {
                    MacroValidationHelper.LogInvalidGuidParameter("OpenedNewsletter", newsletterGuid);
                    return false;
                }
            }

            switch (parameters.Length)
            {
                case 2:
                    return OpenedNewsletter(parameters[0], guid, 0);

                case 3:
                    return OpenedNewsletter(parameters[0], guid, ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact voted in the poll.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact voted in the poll.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "pollGuid", typeof (Guid), "Poll GUID.")]
        [MacroMethodParam(2, "pollAnswer", typeof (string), "Poll answer text.")]
        [MacroMethodParam(3, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object VotedInPoll(EvaluationContext context, params object[] parameters)
        {
            Guid guid = Guid.Empty;
            if (parameters.Length > 1)
            {
                string pollGuid = ValidationHelper.GetString(parameters[1], "");
                if (!Guid.TryParse(pollGuid, out guid))
                {
                    MacroValidationHelper.LogInvalidGuidParameter("VotedInPoll", pollGuid);
                    return false;
                }
            }

            switch (parameters.Length)
            {
                case 2:
                    return VotedInPoll(parameters[0], guid, null, 0);

                case 3:
                    return VotedInPoll(parameters[0], guid, ValidationHelper.GetString(parameters[2], ""), 0);

                case 4:
                    return VotedInPoll(parameters[0], guid, ValidationHelper.GetString(parameters[2], ""), ValidationHelper.GetInteger(parameters[3], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact logged in.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact logged in.", 1)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object LoggedIn(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return LoggedIn(parameters[0], ValidationHelper.GetInteger(parameters[1], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact registered for specific event.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact registered for specific event.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "nodeAliasPath", typeof (string), "Page node alias path.")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object RegisteredForEvent(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return RegisteredForEvent(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0);

                case 3:
                    return RegisteredForEvent(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact purchased specified product.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if the contact purchased specified product.", 2)]
        [MacroMethodParam(0, "contact", typeof (object), "Contact info object.")]
        [MacroMethodParam(1, "productGuid", typeof (Guid), "GUID of the product.")]
        [MacroMethodParam(2, "lastXDays", typeof (int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object PurchasedProduct(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return PurchasedProduct(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty), 0);

                case 3:
                    return PurchasedProduct(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns TRUE if the contact is in any/all specified contact group(s) on current site.
        /// </summary>
        /// <param name="contact">Contact info object</param>
        /// <param name="contactGroupName">Contact group name(s) separated by semicolon</param>
        /// <param name="allGroups">If true contact has to be in all specified groups, if false, it is sufficient if the contact is in any of the specified groups.</param>
        internal static bool IsInContactGroup(object contact, object contactGroupName, bool allGroups)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            string groupNames = ValidationHelper.GetString(contactGroupName, string.Empty);
            if (string.IsNullOrEmpty(groupNames))
            {
                return false;
            }

            string[] groupNamesArray = groupNames.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (allGroups)
            {
                return contactInfo.IsInAllContactGroups(groupNamesArray);
            }

            return contactInfo.IsInAnyContactGroup(groupNamesArray);
        }


        /// <summary>
        /// Returns true if the contact did any/all of the specified activities.
        /// </summary>
        /// <param name="contact">Contact the activities of which should be checked</param>
        /// <param name="activityTypes">Name of the activity(ies) to check separated with semicolon</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        /// <param name="allActivities">If true, all specified types has to be found for the method to return true. If false, at least one of any specified types is sufficient to return true for the method.</param>
        internal static bool DidActivities(object contact, string activityTypes, int lastXDays, bool allActivities)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityContactID", contactInfo.ContactID)
                                                 .Column("ActivityType")
                                                 .Distinct();

            string[] types = activityTypes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            activities.WhereIn("ActivityType", types);

            if (lastXDays > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            if (allActivities)
            {
                return activities.Count == types.Length;
            }

            return activities.Any();
        }


        /// <summary>
        /// Returns contact's last activity of specified activity type.
        /// </summary>
        /// <param name="contact">Contact info object</param>
        /// <param name="activityType">Activity type</param>
        private static ActivityInfo LastActivityOfType(object contact, object activityType)
        {
            ContactInfo ci = (ContactInfo)contact;
            string type = ValidationHelper.GetString(activityType, string.Empty);

            if (ci != null)
            {
                return ActivityInfoProvider.GetContactsLastActivity(ci.ContactID, type);
            }

            return null;
        }


        /// <summary>
        /// Returns contact's first activity of specified activity type.
        /// </summary>
        /// <param name="contact">Contact info object</param>
        /// <param name="activityType">Activity type</param>
        private static ActivityInfo FirstActivityOfType(object contact, object activityType)
        {
            ContactInfo ci = (ContactInfo)contact;
            string type = ValidationHelper.GetString(activityType, string.Empty);

            if (ci != null)
            {
                return ActivityInfoProvider.GetContactsFirstActivity(ci.ContactID, type);
            }

            return null;
        }


        /// <summary>
        /// Returns contact's points in specified score on current site.
        /// </summary>
        /// <param name="contact">Contact info object</param>
        /// <param name="scoreName">Score name</param>
        private static int GetScore(object contact, object scoreName)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return 0;
            }

            string score = ValidationHelper.GetString(scoreName, string.Empty);

            if (string.IsNullOrEmpty(score))
            {
                return 0;
            }

            return ScoreInfoProvider
                .GetScores()
                .Column(new AggregatedColumn(AggregationType.Sum, "Value").As("Score"))
                .From(new QuerySource("OM_Score")
                    .Join("OM_ScoreContactRule", "ScoreID", "ScoreID"))
                .Where(w => w.WhereNull("Expiration")
                             .Or()
                             .Where("Expiration", QueryOperator.LargerThan, Service.Resolve<IDateTimeNowService>().GetDateTimeNow()))
                .WhereEquals("ContactID", contactInfo.ContactID)
                .WhereEquals("ScoreName", score)
                .GetScalarResult(0);
        }


        /// <summary>
        /// Returns true if contact belongs to specified account.
        /// </summary>
        /// <param name="contact">Contact info object</param>
        /// <param name="accountID">Account ID</param>
        private static bool BelongsToAccount(object contact, object accountID)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            int accountId = ValidationHelper.GetInteger(accountID, 0);
            if (accountId > 0)
            {
                return AccountContactInfoProvider.GetAccountContactInfo(accountId, contactInfo.ContactID) != null;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the contact's country matches one of the specified countries.
        /// </summary>
        /// <param name="contact">Contact the activities of which should be checked</param>
        /// <param name="countryList">Countries list (separated with semicolon)</param>
        private static bool IsFromCountry(object contact, string countryList)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var country = CountryInfoProvider.GetCountryInfo(contactInfo.ContactCountryID);
            if (country == null)
            {
                return false;
            }

            string[] countryNames = countryList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string countryName in countryNames)
            {
                if (countryName.Equals(country.CountryDisplayName, StringComparison.InvariantCultureIgnoreCase) || countryName.Equals(country.CountryName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if the contact's country matches one of the specified countries.
        /// </summary>
        /// <param name="contact">Contact the activities of which should be checked</param>
        /// <param name="stateList">State list (separated with semicolon)</param>
        private static bool IsFromState(object contact, string stateList)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            StateInfo state = StateInfoProvider.GetStateInfo(contactInfo.ContactStateID);
            if (state == null)
            {
                return false;
            }

            string[] states = stateList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in states)
            {
                if (s.Equals(state.StateDisplayName, StringComparison.InvariantCultureIgnoreCase) || s.Equals(state.StateName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if the contact did search (internal or external) for specified keywords in last X days.
        /// </summary>
        /// <param name="contact">Contact the activities of which should be checked</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        /// <param name="keywords">Keywords separated with comma or semicolon (if null or empty, than method returns true if contact did any search in last X days)</param>
        /// <param name="allKeywords">If true all keywords has to be matched, if false at least one match has to be found</param>
        private static bool SearchedForKeywords(object contact, string keywords, int lastXDays, bool allKeywords)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            string[] words = keywords.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            var query = ContactInfoProvider.GetContacts().WhereEquals("ContactID", contactInfo.ContactID);
            if (allKeywords)
            {
                return query.SearchedForAll(words, lastXDays).Any();
            }

            return query.SearchedForAny(words, lastXDays).Any();
        }


        /// <summary>
        /// Returns true if the contact spent specified amount of money (counted by order total price in main currency) in last X days.
        /// </summary>
        /// <param name="contact">Contact the activities of which should be checked</param>
        /// <param name="lowerBound">Inclusive lower bound of the amount of money (in main currency) spent by contact</param>
        /// <param name="upperBound">Inclusive upper bound of the amount of money (in main currency) spent by contact</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool SpentMoney(object contact, decimal lowerBound, decimal upperBound, int lastXDays)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PURCHASE)
                                                 .WhereEquals("ActivityContactID", contactInfo.ContactID)
                                                 .Column("ActivityContactID")
                                                 .GroupBy("ActivityContactID")
                                                 .Having(w =>
                                                     w.Where(new AggregatedColumn(AggregationType.Sum, SqlHelper.GetCast("ActivityValue", "DECIMAL")), QueryOperator.LargerOrEquals, lowerBound)
                                                 )
                                                 .Having(w =>
                                                     w.Where(new AggregatedColumn(AggregationType.Sum, SqlHelper.GetCast("ActivityValue", "DECIMAL")), QueryOperator.LessOrEquals, upperBound)
                                                 );


            if (lastXDays > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            return activities.Any();
        }


        /// <summary>
        /// Returns true if the contact purchased specified number of products in last X days.
        /// </summary>
        /// <param name="contact">Contact the activities of which should be checked</param>
        /// <param name="lowerBound">Inclusive lower bound of the number of products bought</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool PurchasedNumberOfProducts(object contact, int lowerBound, int lastXDays)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var whenThenStatements = new Dictionary<string, string>
            {
                {
                    new WhereCondition().WhereEquals(SqlHelper.GetIsNumeric("ActivityValue").AsExpression(), 1).ToString(true),
                    SqlHelper.GetConvert("ActivityValue", "INT")
                }
            };

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PURCHASEDPRODUCT)
                                                 .WhereEquals("ActivityContactID", contactInfo.ContactID)
                                                 .Column("ActivityContactID")
                                                 .GroupBy("ActivityContactID")
                                                 .Having(w => w.Where(
                                                     new AggregatedColumn(AggregationType.Sum, SqlHelper.GetCase(whenThenStatements, elseCase: "1")), QueryOperator.LargerOrEquals, lowerBound)
                                                 );

            if (lastXDays > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            return activities.Any();
        }


        /// <summary>
        /// Returns true if the contact is in any/all of the specified roles (i.e. if any of the user assigned to the contact is in any/all specified roles).
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="roleGuids">Role guids</param>
        /// <param name="allRoles">If true, contact has to in all specified roles. If false, it is sufficient if the contact is at least in one of the roles.</param>
        private static bool IsInRoles(object contact, ICollection<Guid> roleGuids, bool allRoles)
        {
            // In order to ensure same behavior as CMSContactIsInRoleInstanceTranslator, condition must be fulfilled by a single user related to contact. If contact is related to more users
            // and condition is fulfilled by more combined users which are related to contact, but no single user fulfills the 
            // rule, the rule is considered not fulfilled.

            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null || !roleGuids.Any())
            {
                return false;
            }

            var roles = RoleInfoProvider.GetRoles().WhereIn("RoleGuid", roleGuids).Columns("RoleName", "SiteID");
            if ((roles.Count < roleGuids.Count) && allRoles)
            {
                return false;
            }

            var result = allRoles ? IsContactInAllRoles(contactInfo, roles.ToList()) : IsContactInAnyRole(contactInfo, roles.ToList());

            return result;
        }


        /// <summary>
        /// Return true, if contact is at least in one role.
        /// </summary>
        /// <param name="contact">Contact to check.</param>
        /// <param name="roles">Collection of roles in which at least one role is assigned to any of the <paramref name="contact"/>'s users.</param>
        private static bool IsContactInAnyRole(ContactInfo contact, IEnumerable<RoleInfo> roles)
        {
            return contact.Users.Any(u => roles.Any(r => u.IsInRole(r.RoleName, SiteInfoProvider.GetSiteName(r.SiteID), true, true)));
        }


        /// <summary>
        /// Returns true, if at least one user of the contact's <see cref="ContactInfo.Users"/> is in all roles.
        /// </summary>
        /// <param name="contact">Contact to check.</param>
        /// <param name="roles">Collection of all roles in which at least one of the <paramref name="contact"/>'s user should be in.</param>
        private static bool IsContactInAllRoles(ContactInfo contact, IEnumerable<RoleInfo> roles)
        {
            return contact.Users.Any(u => roles.All(r => u.IsInRole(r.RoleName, SiteInfoProvider.GetSiteName(r.SiteID), true, true)));
        }


        /// <summary>
        /// Returns true if the contact is in any/all of the specified community groups (i.e. if any of the user assigned to the contact is in any/all specified community groups).
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="groupGuids">Group guids</param>
        /// <param name="allGroups">If true, contact has to in all specified groups. If false, it is sufficient if the contact is at least in one of the groups.</param>
        private static bool IsInCommunityGroup(object contact, ICollection<Guid> groupGuids, bool allGroups)
        {
            ContactInfo contactInfo = contact as ContactInfo;
            if (contactInfo == null || !groupGuids.Any())
            {
                return false;
            }

            var groupInfos = new ObjectQuery(PredefinedObjectType.GROUP).WhereIn("GroupGuid", groupGuids).Columns("GroupID").GetListResult<int>();
            if ((groupInfos.Count < groupGuids.Count) && allGroups)
            {
                return false;
            }

            Func<Func<int, bool>, bool> userInfoPredicate;
            if (allGroups)
            {
                userInfoPredicate = groupInfos.All;
            }
            else
            {
                userInfoPredicate = groupInfos.Any;
            }

            return contactInfo.Users.Any(userInfo => userInfoPredicate(groupId => ModuleCommands.CommunityIsMemberOfGroup(userInfo.UserID, groupId)));
        }


        /// <summary>
        /// Returns true if the contact opened specified newsletter.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="newsletterGuid">Newsletter guid</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool OpenedNewsletter(object contact, Guid newsletterGuid, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null || newsletterGuid == Guid.Empty)
            {
                return false;
            }

            // Gets all newsletters with the given guid, there can be more newsletters with the same guid in different sites
            var newsletters = new DataQuery(PredefinedObjectType.NEWSLETTER, null)
                .WhereEquals("NewsletterGuid", newsletterGuid)
                .Column("NewsletterID")
                .Result;

            foreach (DataRow row in newsletters.Tables[0].Rows)
            {
                if (ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.NEWSLETTER_OPEN, null, lastXDays, "ActivityItemID = " + row["NewsletterID"]))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if the contact voted in the poll.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="pollGuid">Poll GUID</param>
        /// <param name="answer">Poll answer text</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool VotedInPoll(object contact, Guid pollGuid, string answer, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var pollIds = new ObjectQuery(PredefinedObjectType.POLL).WhereEquals("PollGUID", pollGuid).Column("PollID").GetListResult<int>();
            if (string.IsNullOrEmpty(answer))
            {
                return VotedInPollForAnyAnswer(contactInfo, pollIds, lastXDays);
            }

            return VotedInPollForSpecifiedAnswer(contactInfo, pollIds, lastXDays, answer);
        }


        /// <summary>
        /// Returns true if the contact voted in the poll for any answer.
        /// </summary>
        /// <param name="contactInfo">Contact which should be checked</param>
        /// <param name="pollIds">Poll IDs</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool VotedInPollForAnyAnswer(ContactInfo contactInfo, IList<int> pollIds, int lastXDays)
        {
            var whereCondition = new WhereCondition().WhereIn("ActivityItemID", pollIds).ToString(true);
            return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.POLL_VOTING, null, lastXDays, whereCondition);
        }


        /// <summary>
        /// Returns true if the contact voted in the poll for specified answer.
        /// </summary>
        /// <param name="contactInfo">Contact which should be checked</param>
        /// <param name="pollIds">Poll IDs</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        /// <param name="specifiedAnswer">Poll answer text</param>
        private static bool VotedInPollForSpecifiedAnswer(ContactInfo contactInfo, IList<int> pollIds, int lastXDays, string specifiedAnswer)
        {
            var whereCondition = new WhereCondition().WhereIn("ActivityItemID", pollIds).ToString(true);
            var ds = ActivityInfoProvider.GetContactActivities(contactInfo.ContactID, PredefinedActivityType.POLL_VOTING, null, lastXDays, whereCondition)
                .Column("ActivityValue");

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return false;
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string[] answers = ValidationHelper.GetString(dr[0], "").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string answer in answers)
                {
                    BaseInfo pollAnswer = ProviderHelper.GetInfoById("polls.pollanswer", ValidationHelper.GetInteger(answer, 0));
                    if ((pollAnswer != null) && (pollAnswer.GetStringValue("AnswerText", "").Equals(specifiedAnswer, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if the contact logged in.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool LoggedIn(object contact, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.USER_LOGIN, null, lastXDays, null);
        }


        /// <summary>
        /// Returns true if the contact purchased specified product or its variant.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="skuGuid">GUID of the SKU</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool PurchasedProduct(object contact, Guid skuGuid, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            // Get all products ids with given guid
            IList<int> productsIds = new DataQuery(PredefinedObjectType.SKU, null).WhereEquals("SKUGUID", skuGuid).Column("SKUID").GetListResult<int>();

            if (productsIds.Count > 0)
            {
                // Find all product variants belonging to the specified products
                IList<int> productVariantsIds = new DataQuery().From("COM_SKU").Column("SKUID").WhereIn("SKUParentSKUID", productsIds).GetListResult<int>();
                // SkuIds of the parent products has to be added in case contact bought parent product (no variant exists)
                var skuIds = productVariantsIds.Union(productsIds);

                return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.PURCHASEDPRODUCT, null, lastXDays, SqlHelper.GetWhereCondition("ActivityItemID", skuIds));
            }

            return false;
        }


        /// <summary>
        /// Returns true if the contact visited specified page.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="nodeGuid">Page node GUID</param>
        /// <param name="culture">Culture of the document</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool VisitedPage(object contact, Guid nodeGuid, string culture, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var nodeIds = new DocumentQuery()
                .WhereEquals("NodeGUID", nodeGuid)
                .CombineWithAnyCulture()
                .Column("NodeID")
                .GetListResult<int>();

            if (!nodeIds.Any())
            {
                return false;
            }

            var whereCondition = new WhereCondition().WhereIn("ActivityNodeID", nodeIds);
            if (!string.IsNullOrEmpty(culture))
            {
                whereCondition.WhereEquals("ActivityCulture", culture);
            }

            return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.PAGE_VISIT, null, lastXDays, whereCondition.ToString(true));
        }


        /// <summary>
        /// Returns true if the contact visited specified site.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="siteName">Name of the site which should be checked</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool VisitedSite(object contact, string siteName, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var siteId = SiteInfoProvider.GetSiteID(siteName);
            return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.PAGE_VISIT, null, lastXDays, string.Format("ActivitySiteID = {0}", siteId));
        }


        /// <summary>
        /// Returns true if the contact visited page containing specified URL.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="url">Page URL to be checked</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        /// <param name="landingPage">Indicates whether contact visited landing page or normal page</param>
        private static bool VisitedURL(object contact, string url, int lastXDays, bool landingPage = false)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var whereCondition = new WhereCondition().WhereContains("ActivityURL", url);
            return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, landingPage ? PredefinedActivityType.LANDING_PAGE : PredefinedActivityType.PAGE_VISIT, null, lastXDays, whereCondition.ToString(true));
        }


        /// <summary>
        /// Returns true if the contact registered for specific event.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="nodeAliasPath">Node alias path of BookingEvent document</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool RegisteredForEvent(object contact, string nodeAliasPath, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var documentIds = new TreeProvider().SelectNodes()
                                                .All()
                                                .WhereEquals("NodeAliasPath", nodeAliasPath)
                                                .Column("DocumentID")
                                                .GetListResult<int>();

            var activityCondition = new WhereCondition().WhereIn("ActivityItemDetailID", documentIds);

            return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.EVENT_BOOKING, null, lastXDays, activityCondition.ToString(true));
        }
    }
}