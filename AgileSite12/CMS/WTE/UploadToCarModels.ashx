<%@ WebHandler Language="C#"  Class="UploadToCarModels" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for UploadToCarmeets
/// </summary>
public class UploadToCarModels : Telerik.Web.UI.CloudUploadHandler
{

    public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
    {
        var customFolder = "carmeets/";
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
