using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;

using Telerik.Web.UI;

namespace CMSApp.CMSWebParts.WTE.KofcNC
{
    /// <summary>
    /// Cloud upload control
    /// </summary>
    public partial class CloudImageUploader : CMSAbstractWebPart
    {
        #region "Properties"

        #region "Settings"

        /// <summary>
        /// Get the mode.
        /// </summary>
        public int UploadMode
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UploadMode"), 0);
            }
            set
            {
                SetValue("UploadMode", value);
            }
        }

        /// <summary>
        /// Allowed File Extension
        /// </summary>
        public string AllowedFileExtension
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AllowedFileExtension"), "jpg,jpeg,gif,png");
            }
            set
            {
                SetValue("AllowedFileExtension", value);
            }
        }

        /// <summary>
        /// The button text
        /// </summary>
        public string ButtonText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ButtonText"), "Select");
            }
            set
            {
                SetValue("ButtonText", value);
            }
        }

        /// <summary>
        /// Button CSS class
        /// </summary>
        public string ButtonCSS
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ButtonCSS"), "");
            }
            set
            {
                SetValue("ButtonCSS", value);
            }
        }

        /// <summary>
        /// Text to display in the drop zone
        /// </summary>
        public string DropZoneText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DropZoneText"), "");
            }
            set
            {
                SetValue("DropZoneText", value);
            }
        }

        /// <summary>
        /// Test mode
        /// </summary>
        public bool TestMode
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("TestMode"), false);
            }
            set
            {
                SetValue("DropZoneText", value);
            }
        }

        #endregion "Settings"

        #region IDS

        /// <summary>
        /// The ItemID of the associated object.
        /// </summary>
        public int ObjectID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ObjectID"), 0);
            }
            set
            {
                SetValue("ObjectID", value);
            }
        }

        /// <summary>
        /// User ID (of the user that uploaded the image)
        /// </summary>
        public int UserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserID"), 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        #endregion IDS

        #endregion "Properties"

        protected void Page_Load(object sender, EventArgs e)
        {
            string[] allowedFile = AllowedFileExtension.Split(',').ToArray();

            rcuMain.AllowedFileExtensions = allowedFile;

            if (!String.IsNullOrWhiteSpace(DropZoneText))
            {
                rcuMain.Localization.DropZone = DropZoneText;
            }

            if (!String.IsNullOrWhiteSpace(ButtonText))
            {
                rcuMain.Localization.SelectButtonText = ButtonText;
            }

            if (!String.IsNullOrWhiteSpace(ButtonCSS))
            {
                divContainer.Attributes["class"] = ButtonCSS;
            }

            switch (UploadMode)
            {
                case 1:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToAssembly.ashx";
                    break;
                case 2:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToCouncil.ashx";
                    break;
                case 3:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToEvent.ashx";
                    break;
                case 4:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToMember.ashx";
                    break;
				 case 5:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToParish.ashx";
                    break;
                case 6:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToRegion.ashx";
                    break;
                case 7:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToStateCouncil.ashx";
                    break;
                case 8:
                    rcuMain.HttpHandlerUrl = "~/KofcNC/UploadToDistrict.ashx";
                    break;
                case 0:
                default:
                    // no adjustment needed
                    break;
            }
        }

        /// <summary>
        /// a file is uploaded (note this event trigger for each file, but only when a postback occurs)
        /// Post back is trigger through the client handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void OnCloudFileUploaded(object sender, CloudFileUploadedEventArgs args)
        {
            //You could access the information about the uploaded file, using the FileInfo object.

            long contentLenght = args.FileInfo.ContentLength;
            string contentType = args.FileInfo.ContentType;
            string keyName = args.FileInfo.KeyName;
            string originalName = args.FileInfo.OriginalFileName;

            //You could manage the IsValid state of the uploaded file.
            args.IsValid = true;

            // Store the file to the database.
            SaveLog(keyName, originalName);

            if (TestMode)
            {
                //https://kofcnc.s3.amazonaws.com/events/DSC00292.JPG

                string url = "https://kofcnc.s3.amazonaws.com/" + keyName;
                URLHelper.ResponseRedirect(url);
            }
        }

        /// <summary>
        /// Add image association.
        /// </summary>
        /// <param name="p_filename"></param>
        /// <param name="p_orignalName"></param>
        /// <returns></returns>
        protected string SaveLog(string p_filename, string p_orignalName)
        {
            string ret = String.Empty;
            DataSet ds = null;
            GeneralConnection conn = ConnectionHelper.GetConnection();
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Mode", UploadMode);
            parameters.Add("@ObjectID", ObjectID);
            parameters.Add("@UserID",UserID);
            parameters.Add("@ImageKeyName", p_filename);
            parameters.Add("@OriginalName", p_orignalName);
         
            QueryParameters qp = new QueryParameters("custom_Add_CloudUpload_Log", parameters, QueryTypeEnum.StoredProcedure, false);
            ds = conn.ExecuteQuery(qp);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                DataTable table = DataHelper.GetDataTable(ds);
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    if (row != null)
                    {
                        ret = (string)row["RedirectURL"];
                    }
                }
            }

            return ret;
        }
    }
}