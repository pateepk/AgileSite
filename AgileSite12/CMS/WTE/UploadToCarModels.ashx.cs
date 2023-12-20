using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Web.UI;


/// <summary>
/// Summary description for UploadToRides
/// </summary>
public class UploadToCarModels : CloudUploadHandler
{

    public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
    {
        var customFolder = "carmodels/";
        // TODO: Clean the file name here
        e.KeyName = string.Format("{0}{1}_{2}", customFolder, Guid.NewGuid(), e.OriginalFileName);
    }

    //    public override void SetCustomProvider(object sender, Telerik.Web.UI.CloudUpload.CustomProviderSetupEventArgs e)
    //    {
    //        //Check for some condition and select Custom Provider
    //        if (true)
    //        {
    //            e.Name = "CustomAmazonProvider2";
    //        }
    //        else
    //        {
    //            e.Name = "CustomAmazonProvider1";
    //        }
    //    } 
}
