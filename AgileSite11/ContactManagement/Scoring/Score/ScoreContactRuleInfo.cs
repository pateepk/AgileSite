using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(ScoreContactRuleInfo), ScoreContactRuleInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Info object for binding score - contact - rule.
    /// </summary>
    public class ScoreContactRuleInfo : AbstractInfo<ScoreContactRuleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.scorecontactrule";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ScoreContactRuleInfoProvider), OBJECT_TYPE, "OM.ScoreContactRule", "ScoreContactRuleID", null, null, null, null, null, null, "ContactID", ContactInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
                            {
                                new ObjectDependency("ScoreID", ScoreInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding), 
                                new ObjectDependency("RuleID", RuleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                                new ObjectDependency("RuleID", RuleInfo.OBJECT_TYPE_PERSONA, ObjectDependencyEnum.Binding)
                            },
            SupportsCloning = false,
            AllowTouchParent = false,
            ModuleName = ModuleName.CONTACTMANAGEMENT,
            IsBinding = true,
            Feature = FeatureEnum.LeadScoring,
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ScoreInfo ID.
        /// </summary>
        public virtual int ScoreID
        {
            get
            {
                return GetIntegerValue("ScoreID", 0);
            }
            set
            {
                SetValue("ScoreID", value);
            }
        }


        /// <summary>
        /// ContactInfo ID.
        /// </summary>
        public virtual int ContactID
        {
            get
            {
                return GetIntegerValue("ContactID", 0);
            }
            set
            {
                SetValue("ContactID", value);
            }
        }


        /// <summary>
        /// RuleInfo ID.
        /// </summary>
        public virtual int RuleID
        {
            get
            {
                return GetIntegerValue("RuleID", 0);
            }
            set
            {
                SetValue("RuleID", value);
            }
        }


        /// <summary>
        /// Total value of score gained (value of a rule can be recurring and can be limited).
        /// </summary>
        public virtual int Value
        {
            get
            {
                return GetIntegerValue("Value", 0);
            }
            set
            {
                SetValue("Value", value);
            }
        }


        /// <summary>
        /// Expiration gets refreshed each time that value is increased.
        /// </summary>
        public virtual DateTime Expiration
        {
            get
            {
                return GetDateTimeValue("Expiration", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("Expiration", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// ScoreContactRule ID.
        /// </summary>
        public virtual int ScoreContactRuleID
        {
            get
            {
                return GetIntegerValue("ScoreContactRuleID", 0);
            }
            set
            {
                SetValue("ScoreContactRuleID", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ScoreContactRuleInfo object.
        /// </summary>
        public ScoreContactRuleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ScoreContactRuleInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ScoreContactRuleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ScoreContactRuleInfoProvider.SetScoreContactRuleInfo(this);
        }

        #endregion
    }
}