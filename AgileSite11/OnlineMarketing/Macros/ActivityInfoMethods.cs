using System;

using CMS;
using CMS.Activities;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.OnlineMarketing;

[assembly: RegisterExtension(typeof(ActivityInfoMethods), typeof(ActivityInfo))]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Online marketing methods - wrapping methods for macro resolver.
    /// </summary>
    internal class ActivityInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns if activity is linked to object of type given in parameter and codename given in second parameter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns if activity is linked to given object by GUID or codename.", 3, Name = "LinkedToObject")]
        [MacroMethodParam(0, "activity", typeof (ActivityInfo), "Activity info object.")]
        [MacroMethodParam(1, "objectType", typeof (string), "Object type.")]
        [MacroMethodParam(2, "objectIdentifier", typeof (string), "Object code name or GUID.")]
        public static object ActivityLinkedToObject(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return ActivityLinkedToObject(parameters[0] as ActivityInfo, parameters[1] as string, parameters[2] as string);

                default:
                    throw new NotSupportedException();
            }
        }



        /// <summary>
        /// Returns if activity is linked to object of type given in parameter and codename given in second parameter.
        /// </summary>
        /// <param name="activityInfo">Activity that should be checked</param>
        /// <param name="objectType">Object type</param>
        /// <param name="identifier">Code name or GUID of object</param>
        internal static bool ActivityLinkedToObject(ActivityInfo activityInfo, string objectType, string identifier)
        {
            if (activityInfo == null)
            {
                return false;
            }

            int id = -1;
            switch (objectType)
            {
                case PredefinedObjectType.POLL:
                case PredefinedObjectType.NEWSLETTER:
                case PredefinedObjectType.BIZFORM:
                case PredefinedObjectType.FORUM:
                case PredefinedObjectType.GROUPMEMBER:
                case PredefinedObjectType.BOARDMESSAGE:
                case PredefinedObjectType.BOARD:
                case PredefinedObjectType.SKU:
                    id = activityInfo.ActivityItemID;
                    break;
                case PredefinedObjectType.NEWSLETTERISSUE:
                    var issue = ProviderHelper.GetInfoById(PredefinedObjectType.NEWSLETTERISSUE, activityInfo.ActivityItemDetailID);
                    if (issue != null)
                    {
                        id = issue.GetIntegerValue("IssueVariantOfIssueID", activityInfo.ActivityItemDetailID);
                    }
                    break;
                case "cms.blog":
                    id = activityInfo.ActivityItemDetailID;
                    objectType = PredefinedObjectType.NODE;
                    break;
                case PredefinedObjectType.DOCUMENT:
                case PredefinedObjectType.NODE:
                    id = activityInfo.ActivityNodeID;
                    break;
                default:
                    id = activityInfo.ActivityItemID;
                    break;
            }

            Guid objectGuid;
            var baseObject = Guid.TryParse(identifier, out objectGuid) ?
                ProviderHelper.GetInfoByGuid(objectType, objectGuid, activityInfo.ActivitySiteID) :
                ProviderHelper.GetInfoByName(objectType, identifier, activityInfo.ActivitySiteID);

            return (baseObject != null) && (id == baseObject.Generalized.ObjectID);
        }
    }
}