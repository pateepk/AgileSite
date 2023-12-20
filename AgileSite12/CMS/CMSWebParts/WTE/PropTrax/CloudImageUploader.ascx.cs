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

namespace CMSApp.CMSWebParts.WTE.PropTrax
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
                // 0 root
                // 1 carmeets
                // 2 rides
                // 3 mods
                // 4 carmodels
				// 5 photoshare
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

        #endregion "Settings"

        #region IDS

        /// <summary>
        /// Car Meets ID
        /// </summary>
        public int MeetsID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MeetsID"), 0);
            }
            set
            {
                SetValue("MeetsID", value);
            }
        }

        /// <summary>
        /// Car meets date id
        /// </summary>
        public int MeetsDateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MeetsDateID"), 0);
            }
            set
            {
                SetValue("MeetsDateID", value);
            }
        }

        /// <summary>
        /// Mods ID
        /// </summary>
        public int ModsID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ModsID"), 0);
            }
            set
            {
                SetValue("ModsID", value);
            }
        }

        /// <summary>
        /// Rides ID
        /// </summary>
        public int RidesID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("RidesID"), 0);
            }
            set
            {
                SetValue("RidesID", value);
            }
        }

        /// <summary>
        /// The owner id
        /// </summary>
        public int OwnerID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("OwnerID"), 0);
            }
            set
            {
                SetValue("OwnerID", value);
            }
        }
        /// <summary>
        /// The Building id
        /// </summary>
        public int BuildingID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("BuildingID"), 0);
            }
            set
            {
                SetValue("BuildingID", value);
            }
        }
        /// <summary>
        /// The Asset id
        /// </summary>
        public int AssetID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AssetID"), 0);
            }
            set
            {
                SetValue("AssetID", value);
            }
        }
        /// <summary>
        /// The Structure id
        /// </summary>
        public int StructureID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("StructureID"), 0);
            }
            set
            {
                SetValue("StructureID", value);
            }
        }
        /// <summary>
        /// The Construction id
        /// </summary>
        public int ConstructionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConstructionID"), 0);
            }
            set
            {
                SetValue("ConstructionID", value);
            }
        }
        /// <summary>
        /// Car modles ID for brand images.
        /// </summary>
        public int CarModelID
        {
            get 
            {
                return ValidationHelper.GetInteger(GetValue("CarModelID"), 0);
            }
            set
            {
                SetValue("CarModelID", value);
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
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToPropBuilding.ashx";
                    break;

                case 2:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToRides.ashx";
                    break;

                case 3:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToMods.ashx";
                    break;

                case 4:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToCarModels.ashx";
                    break;
				 case 5:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToPhotoShare.ashx";
                    break;
                case 6:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToPropAsset.ashx";
                    break;
                case 7:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToPropStructure.ashx";
                    break;
                case 8:
                    rcuMain.HttpHandlerUrl = "~/WTE/UploadToPropConstruction.ashx";
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
            //string url = "https://rideology.s3.amazonaws.com/" + keyName;
            //Response.Redirect(url);

            SaveLog(keyName);
        }

        /// <summary>
        /// Add image association.
        /// </summary>
        /// <returns></returns>
        protected string SaveLog(string p_filename)
        {
            string ret = String.Empty;
            DataSet ds = null;
            GeneralConnection conn = ConnectionHelper.GetConnection();
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Mode", UploadMode);
            parameters.Add("@BuildingID", BuildingID);
            parameters.Add("@AssetID", AssetID);
            parameters.Add("@StrucureID", StructureID);
            parameters.Add("@ConstructionID", ConstructionID);
            parameters.Add("@MeetsDateID", MeetsDateID);
            parameters.Add("@ModsID", ModsID);
            parameters.Add("@RidesID", RidesID);
            parameters.Add("@OwnerID", OwnerID);
            parameters.Add("@ImageKeyName", p_filename);
         

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