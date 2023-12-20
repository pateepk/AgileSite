using System;
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

namespace CMSApp.CMSWebParts.WTE.Custom
{

    /// <summary>
    /// Cloud upload control
    /// </summary>
    public partial class WTE_CloudImageUploader : CMSAbstractWebPart
    {

        #region "Properties"

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
                return ValidationHelper.GetInteger(GetValue("UploadMode"), 0);
            }
            set
            {
                SetValue("UploadMode", value);
            }
        }

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

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //RadCloudUpload1.MultipleFileSelection = MultipleFileSelection.Automatic;
            //CloudUploadFileInfoCollection temp =  RadCloudUpload1.UploadedFiles;
        }

        /// <summary>
        /// Test Clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbTest_Click(object sender, EventArgs e)
        {
            //lblTest.Text = "Rad Text Box: " + rtbDoc.Text + "<br\\>" + "TextBox:" + tbDoc.Text;
        }

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
            string url = "https://rideology.s3.amazonaws.com/" + keyName;
            Response.Redirect(url);
        }

        protected void RadCloudUpload1_FileUploaded(object sender, Telerik.Web.UI.CloudFileUploadedEventArgs args)
        {
            // The following line is added only for the purposes of the demo.
            // The uploaded files need to be removed from the storage by the control after a certain time.
            //lblTest.Text = "upload";
            //args.IsValid = false;
            //You could access the information about the uploaded file, using the FileInfo object. 

            long contentLenght = args.FileInfo.ContentLength;
            string contentType = args.FileInfo.ContentType;
            string keyName = args.FileInfo.KeyName;
            string originalName = args.FileInfo.OriginalFileName;

            //You could manage the IsValid state of the uploaded file.
            args.IsValid = true;
        }
    }
}