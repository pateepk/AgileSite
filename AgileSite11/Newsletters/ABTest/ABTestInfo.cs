using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.Scheduler;

[assembly: RegisterObjectType(typeof(ABTestInfo), ABTestInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// ABTestInfo data container class.
    /// </summary>
    public class ABTestInfo : AbstractInfo<ABTestInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.abtest";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ABTestInfoProvider), OBJECT_TYPE, "Newsletter.ABTest", "TestID", "TestLastModified", "TestGUID", null, null, null, null, "TestIssueID", IssueInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("TestWinnerIssueID", IssueInfo.OBJECT_TYPE_VARIANT),
                new ObjectDependency("TestWinnerScheduledTaskID", TaskInfo.OBJECT_TYPE)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ExcludedStagingColumns = new List<string>
                {
                    "TestWinnerIssueID",
                    "TestWinnerScheduledTaskID",
                    "TestWinnerSelected"
                }
            },
            LogEvents = true,
            AllowTouchParent = true,
            TouchCacheDependencies = true,
            MaxCodeNameLength = 50,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = true,
                IsExportable = true,
                IsAutomaticallySelected = true
            },
            Feature = FeatureEnum.NewsletterABTesting,
            ContainsMacros = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "TestWinnerIssueID",
                    "TestWinnerScheduledTaskID",
                    "TestWinnerSelected"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Time interval (in minutes) after a winner will be selected.
        /// </summary>
        [DatabaseField]
        public virtual int TestSelectWinnerAfter
        {
            get
            {
                return GetIntegerValue("TestSelectWinnerAfter", 0);
            }
            set
            {
                SetValue("TestSelectWinnerAfter", value);
            }
        }


        /// <summary>
        /// Gets or sets the date when winner was selected.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TestWinnerSelected
        {
            get
            {
                return GetDateTimeValue("TestWinnerSelected", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TestWinnerSelected", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets the date and time the object was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TestLastModified
        {
            get
            {
                return GetDateTimeValue("TestLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TestLastModified", value);
            }
        }


        /// <summary>
        /// Determines how to select winner.
        /// </summary>
        [DatabaseField]
        public virtual ABTestWinnerSelectionEnum TestWinnerOption
        {
            get
            {
                return (ABTestWinnerSelectionEnum)GetIntegerValue("TestWinnerOption", 0);
            }
            set
            {
                SetValue("TestWinnerOption", (int)value);
            }
        }


        /// <summary>
        /// Winner issue ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestWinnerIssueID
        {
            get
            {
                return GetIntegerValue("TestWinnerIssueID", 0);
            }
            set
            {
                SetValue("TestWinnerIssueID", value, value > 0);
            }
        }


        /// <summary>
        /// GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid TestGUID
        {
            get
            {
                return GetGuidValue("TestGUID", Guid.Empty);
            }
            set
            {
                SetValue("TestGUID", value);
            }
        }


        /// <summary>
        /// Parent issue (in case of A/B test).
        /// </summary>
        [DatabaseField]
        public virtual int TestIssueID
        {
            get
            {
                return GetIntegerValue("TestIssueID", 0);
            }
            set
            {
                SetValue("TestIssueID", value);
            }
        }


        /// <summary>
        /// ID of A/B test.
        /// </summary>
        [DatabaseField]
        public virtual int TestID
        {
            get
            {
                return GetIntegerValue("TestID", 0);
            }
            set
            {
                SetValue("TestID", value);
            }
        }


        /// <summary>
        /// Size of test group in percents.
        /// </summary>
        [DatabaseField]
        public virtual int TestSizePercentage
        {
            get
            {
                return GetIntegerValue("TestSizePercentage", 0);
            }
            set
            {
                SetValue("TestSizePercentage", value);
            }
        }


        /// <summary>
        /// Number of e-mail addresses to send per each variant.
        /// </summary>
        [DatabaseField]
        public virtual int TestNumberPerVariantEmails
        {
            get
            {
                return GetIntegerValue("TestNumberPerVariantEmails", 0);
            }
            set
            {
                SetValue("TestNumberPerVariantEmails", value, value > 0);
            }
        }


        /// <summary>
        /// ID of scheduled task to select winner.
        /// </summary>
        [DatabaseField]
        public virtual int TestWinnerScheduledTaskID
        {
            get
            {
                return GetIntegerValue("TestWinnerScheduledTaskID", 0);
            }
            set
            {
                SetValue("TestWinnerScheduledTaskID", value, value > 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ABTestInfoProvider.DeleteABTestInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ABTestInfoProvider.SetABTestInfo(this);
        }


        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete winner selection task
            if (TestWinnerScheduledTaskID > 0)
            {
                TaskInfoProvider.DeleteTaskInfo(TestWinnerScheduledTaskID);
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Reset A/B test as new (TestIssueID is set to new (clonned) issue)
            ABTestInfo originalABTest = originalObject as ABTestInfo;

            TestWinnerIssueID = 0;
            TestWinnerSelected = DateTimeHelper.ZERO_TIME;
            TestWinnerScheduledTaskID = 0;
            TestNumberPerVariantEmails = 0;
            Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ABTestInfo object.
        /// </summary>
        public ABTestInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ABTestInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ABTestInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
