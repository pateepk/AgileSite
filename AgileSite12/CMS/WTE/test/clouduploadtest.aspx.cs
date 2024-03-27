using System;
using System.Web;
using System.Web.UI;
using Telerik.Web.UI;

namespace CMSApp.WTE
{
    public partial class clouduploadtest : System.Web.UI.Page
    {
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