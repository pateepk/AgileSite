using System;
using System.Data;
using System.Globalization;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(RuleInfo), RuleInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(RuleInfo), RuleInfo.OBJECT_TYPE_PERSONA)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// RuleInfo data container class.
    /// </summary>
    public class RuleInfo : AbstractInfo<RuleInfo>
    {
        #region "Fields"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.rule";


        /// <summary>
        /// Object type for persona rule
        /// </summary>
        public const string OBJECT_TYPE_PERSONA = "om.personarule";


        private string mRuleWhereCondition;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RuleInfoProvider), OBJECT_TYPE, "OM.Rule", "RuleID", "RuleLastModified", "RuleGUID", "RuleName", "RuleDisplayName", null, null, "RuleScoreID", ScoreInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.SCORING,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
            },
            Feature = FeatureEnum.LeadScoring,
            TypeCondition = new TypeCondition().WhereEquals("RuleBelongsToPersona", false),
        };



        /// <summary>
        /// Persona rule type information.
        /// </summary>
        public static ObjectTypeInfo PERSONARULETYPEINFO = new ObjectTypeInfo(typeof(RuleInfoProvider), OBJECT_TYPE_PERSONA, "OM.Rule", "RuleID", "RuleLastModified", "RuleGUID", "RuleName", "RuleDisplayName", null, null, "RuleScoreID", ScoreInfo.OBJECT_TYPE_PERSONA)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.PERSONAS,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
            },
            Feature = FeatureEnum.Personas,
            TypeCondition = new TypeCondition().WhereEquals("RuleBelongsToPersona", true),
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Rule value.
        /// </summary>
        [DatabaseField]
        public virtual int RuleValue
        {
            get
            {
                return GetIntegerValue("RuleValue", 0);
            }
            set
            {
                SetValue("RuleValue", value);
            }
        }


        /// <summary>
        /// Unique identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid RuleGUID
        {
            get
            {
                return GetGuidValue("RuleGUID", Guid.Empty);
            }
            set
            {
                SetValue("RuleGUID", value);
            }
        }


        /// <summary>
        /// Rule display name.
        /// </summary>
        [DatabaseField]
        public virtual string RuleDisplayName
        {
            get
            {
                return GetStringValue("RuleDisplayName", "");
            }
            set
            {
                SetValue("RuleDisplayName", value);
            }
        }


        /// <summary>
        /// Specifies time periods - minute, hour, day, week, month, year. In database, this value is saved as underlying int value of the ValidityEnum.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))] // ValueType is string, because database column is nvarchar for some reason
        public virtual ValidityEnum RuleValidity
        {
            get
            {
                return (ValidityEnum)GetIntegerValue("RuleValidity", (int)ValidityEnum.Until);
            }
            set
            {
                SetValue("RuleValidity", ((int)value).ToString(CultureInfo.InvariantCulture));
            }
        }


        /// <summary>
        /// Last modification time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RuleLastModified
        {
            get
            {
                return GetDateTimeValue("RuleLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("RuleLastModified", value);
            }
        }


        /// <summary>
        /// Number of time periods the rule is valid for.
        /// </summary>
        [DatabaseField]
        public virtual int RuleValidFor
        {
            get
            {
                return GetIntegerValue("RuleValidFor", 0);
            }
            set
            {
                SetValue("RuleValidFor", value);
            }
        }


        /// <summary>
        /// Rule code name.
        /// </summary>
        [DatabaseField]
        public virtual string RuleName
        {
            get
            {
                return GetStringValue("RuleName", "");
            }
            set
            {
                SetValue("RuleName", value);
            }
        }


        /// <summary>
        /// FK to Score (OM_Score table).
        /// </summary>
        [DatabaseField]
        public virtual int RuleScoreID
        {
            get
            {
                return GetIntegerValue("RuleScoreID", 0);
            }
            set
            {
                SetValue("RuleScoreID", value);
            }
        }


        /// <summary>
        /// RuleID.
        /// </summary>
        [DatabaseField]
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
        /// Indicates rule type - activity type or attribute.
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public virtual RuleTypeEnum RuleType
        {
            get
            {
                return (RuleTypeEnum)GetIntegerValue("RuleType", 1);
            }
            set
            {
                SetValue("RuleType", (int)value);
            }
        }


        /// <summary>
        /// Rule condition in XML format.
        /// </summary>
        [DatabaseField]
        public virtual string RuleCondition
        {
            get
            {
                return GetStringValue("RuleCondition", "");
            }
            set
            {
                SetValue("RuleCondition", value);
            }
        }


        /// <summary>
        /// Indicates if the rule is recurring.
        /// </summary>
        [DatabaseField]
        public virtual bool RuleIsRecurring
        {
            get
            {
                return GetBooleanValue("RuleIsRecurring", false);
            }
            set
            {
                SetValue("RuleIsRecurring", value);
            }
        }


        /// <summary>
        /// Limits number of recurrences of the rule.
        /// </summary>
        [DatabaseField]
        public virtual int RuleMaxPoints
        {
            get
            {
                return GetIntegerValue("RuleMaxPoints", 0);
            }
            set
            {
                SetValue("RuleMaxPoints", value);
            }
        }


        /// <summary>
        /// Name of the rule type - activity type name or attribute name.
        /// </summary>
        [DatabaseField]
        public virtual string RuleParameter
        {
            get
            {
                return GetStringValue("RuleParameter", "");
            }
            set
            {
                SetValue("RuleParameter", value);
            }
        }


        /// <summary>
        /// Specific day until the rule is valid.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RuleValidUntil
        {
            get
            {
                return GetDateTimeValue("RuleValidUntil", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("RuleValidUntil", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets string representation of the where condition.
        /// </summary>
        [DatabaseField]
        public virtual string RuleWhereCondition
        {
            get
            {
                if (string.IsNullOrEmpty(mRuleWhereCondition) && (!string.IsNullOrEmpty(RuleCondition)))
                {
                    // Initialize RuleCondition object
                    RuleCondition condition = new RuleCondition(RuleCondition);
                    mRuleWhereCondition = condition.WhereCondition;
                }

                return mRuleWhereCondition;
            }
        }


        /// <summary>
        /// Gets or sets the RuleIsPersona flag.
        /// </summary>
        [DatabaseField]
        public virtual bool RuleBelongsToPersona
        {
            get
            {
                return GetBooleanValue("RuleBelongsToPersona", true);
            }
            set
            {
                SetValue("RuleBelongsToPersona", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (RuleBelongsToPersona)
                {
                    return PERSONARULETYPEINFO;
                }
                else
                {
                    return TYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RuleInfoProvider.DeleteRuleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RuleInfoProvider.SetRuleInfo(this);
        }


        /// <summary>
        /// Clears RuleWhereCondition property - it is used in SetRuleInfo method reset the property.
        /// </summary>
        internal void ClearWhereCondition()
        {
            mRuleWhereCondition = null;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RuleInfo object.
        /// </summary>
        public RuleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RuleInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public RuleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}