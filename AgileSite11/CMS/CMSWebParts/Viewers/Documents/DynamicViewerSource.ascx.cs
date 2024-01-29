using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.PortalControls;
using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.Controls;
using CMS.SiteProvider;
using System.Text;
using CMS.IO;
using CMS.PortalEngine;

public partial class CMSWebParts_Viewers_Documents_DynamicViewerSource : CMSAbstractWebPart
{
    #region "Properties"

    public string PageType
    {
        get
        {
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(GetValue("PageType"), "CMS.MenuItem"), "CMS.MenuItem");
        }
        set
        {
            SetValue("PageType", value);
        }
    }

    public string Path
    {
        get
        {
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(GetValue("Path"), "/%"), "/%");
        }
        set
        {
            SetValue("Path", value);
        }
    }

    public int NestingLevel
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("NestingLevel"), -1);
        }
        set
        {
            SetValue("NestingLevel", value);
        }
    }

    public string Transformation
    {
        get
        {
            return ValidationHelper.GetString(GetValue("Transformation"), "");
        }
        set
        {
            SetValue("Transformation", value);
        }
    }

    public int TopN
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("TopN"), 0);
        }
        set
        {
            SetValue("TopN", value);
        }
    }

    public string Where
    {
        get
        {
            return ValidationHelper.GetString(GetValue("Where"), "");
        }
        set
        {
            SetValue("Where", value);
        }
    }

    public string OrderBy
    {
        get
        {
            return ValidationHelper.GetString(GetValue("OrderBy"), "");
        }
        set
        {
            SetValue("OrderBy", value);
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            br.ItemTemplate = CMSDataProperties.LoadTransformation(this, Transformation);
            DataSet ds = DocumentHelper.GetDocuments()
                              .TopN(TopN)
                              .Type(PageType)
                              .Path(Path)
                              .NestingLevel(NestingLevel)
                              .WithCoupledColumns()
                              .Where(Where)
                              .OrderBy(OrderBy)
                              .Page(QueryHelper.GetInteger("i", 0), QueryHelper.GetInteger("s", 100));

            // Assigns the DataSet as the data source of the BasicRepeater control
            br.DataSource = ds;
            br.DataBind();

            if (QueryHelper.GetInteger("i", -1)!=-1)
            {
                // Keep current response
                HttpResponse response = Context.Response;

                // Render XML
                response.Clear();
                response.ClearContent();
                response.ContentType = "text/plain";
                response.ContentEncoding = Encoding.UTF8;

                // Render control
                StringBuilder stringBuilder = new StringBuilder();
                using (StringWriter textWriter = new StringWriter(stringBuilder))
                {
                    using (HtmlTextWriter writer = new HtmlTextWriter(textWriter))
                    {
                        RenderChildren(writer);
                    }
                }

                // Add to the response an end response
                response.Write(stringBuilder.ToString());
                RequestHelper.EndResponse();
            }
        }
    }


    /// <summary>
    /// Reloads the control data.
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();

        SetupControl();
    }

    #endregion
}



