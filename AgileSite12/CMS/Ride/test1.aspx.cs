using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;


public partial class test1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }
	
	/// <summary>
	/// Test Clicked.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void rbTest_Click(object sender, EventArgs e)
	{
		//lblTest.Text = "Rad Text Box: " + rtbDoc.Text + "<br\\>" + "TextBox:" +  tbDoc.Text;
	}	
	
	    protected void RadCloudUpload1_FileUploaded(object sender, Telerik.Web.UI.CloudFileUploadedEventArgs args)
        {
        //You could access the information about the uploaded file, using the FileInfo object. 

        long contentLenght = args.FileInfo.ContentLength;
        string contentType = args.FileInfo.ContentType;
        string keyName = args.FileInfo.KeyName;
        string originalName = args.FileInfo.OriginalFileName;

        //You could manage the IsValid state of the uploaded file.
        args.IsValid = true;

        string url = "https://rideology.s3.amazonaws.com/" + keyName;
        Response.Redirect(url);
        }
}