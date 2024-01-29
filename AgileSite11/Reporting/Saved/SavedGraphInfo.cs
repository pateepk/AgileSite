using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Reporting;
using System.Collections.Generic;

[assembly: RegisterObjectType(typeof(SavedGraphInfo), SavedGraphInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// SavedGraphInfo data container class.
    /// </summary>
    public class SavedGraphInfo : AbstractInfo<SavedGraphInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "reporting.savedgraph";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SavedGraphInfoProvider), OBJECT_TYPE, "Reporting.SavedGraph", "SavedGraphID", "SavedGraphLastModified", "SavedGraphGUID", null, null, "SavedGraphBinary", null, "SavedGraphSavedReportID", SavedReportInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            ModuleName = "cms.reporting",
            MimeTypeColumn = "SavedGraphMimeType",
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                SeparatedFields = new List<SeparatedField> {
                    new SeparatedField("SavedGraphBinary")
                    {
                        IsBinaryField = true,
                        FileExtension = "png",
                        FileName = "graph"
                    }
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Saved report ID.
        /// </summary>
        public virtual int SavedGraphSavedReportID
        {
            get
            {
                return GetIntegerValue("SavedGraphSavedReportID", 0);
            }
            set
            {
                SetValue("SavedGraphSavedReportID", value);
            }
        }


        /// <summary>
        /// Mime type.
        /// </summary>
        public virtual string SavedGraphMimeType
        {
            get
            {
                return GetStringValue("SavedGraphMimeType", "");
            }
            set
            {
                SetValue("SavedGraphMimeType", value);
            }
        }


        /// <summary>
        /// Saved graph GUID.
        /// </summary>
        public virtual Guid SavedGraphGUID
        {
            get
            {
                return GetGuidValue("SavedGraphGUID", Guid.Empty);
            }
            set
            {
                SetValue("SavedGraphGUID", value);
            }
        }


        /// <summary>
        /// Saved graph ID.
        /// </summary>
        public virtual int SavedGraphID
        {
            get
            {
                return GetIntegerValue("SavedGraphID", 0);
            }
            set
            {
                SetValue("SavedGraphID", value);
            }
        }


        /// <summary>
        /// Binary data.
        /// </summary>
        public virtual byte[] SavedGraphBinary
        {
            get
            {
                return ValidationHelper.GetBinary(GetValue("SavedGraphBinary"), null);
            }
            set
            {
                SetValue("SavedGraphBinary", value);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime SavedGraphLastModified
        {
            get
            {
                return GetDateTimeValue("SavedGraphLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SavedGraphLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SavedGraphInfoProvider.DeleteSavedGraphInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SavedGraphInfoProvider.SetSavedGraphInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SavedGraphInfo object.
        /// </summary>
        public SavedGraphInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SavedGraphInfo object from the given DataRow.
        /// </summary>
        public SavedGraphInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}