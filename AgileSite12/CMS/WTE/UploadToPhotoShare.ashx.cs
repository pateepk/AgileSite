﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


/// <summary>
/// Summary description for UploadToPhotoShare
/// </summary>
public class UploadToPhotoShare : Telerik.Web.UI.CloudUploadHandler
{

    public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
    {
        var customFolder = "photoshare/";
        // TODO: Clean the file name here
        e.KeyName = string.Format("{0}{1}_{2}", customFolder, Guid.NewGuid(), e.OriginalFileName);
    }
}
