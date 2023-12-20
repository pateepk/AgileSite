using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Web.UI;


/// <summary>
/// Summary description for UploadToMods
/// </summary>
public class UploadToMods : CloudUploadHandler
{

    public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
    {
        var customFolder = "mods/";
        // TODO: Clean the file name here
        e.KeyName = string.Format("{0}{1}_{2}", customFolder, Guid.NewGuid(), e.OriginalFileName);
    }
}
