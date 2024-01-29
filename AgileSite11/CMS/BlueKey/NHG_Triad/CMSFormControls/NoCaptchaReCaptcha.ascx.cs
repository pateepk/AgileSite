using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.SiteProvider;

using Newtonsoft.Json;

using CMS.FormEngine.Web.UI;

//using BlueKey;

namespace NHG_T
{
    public partial class BlueKey_CMSFormControls_NoCaptchaReCaptcha : FormEngineUserControl
    {
        private bool IsCaptchaValid = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                StringBuilder s = new StringBuilder();

                string key = "6LfeagoTAAAAAMY4ScPe4C9L2WpiYasBcaV1kp5K";

                s.Append("<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>" + Environment.NewLine);
                s.Append("<form action=\"?\" method=\"POST\">" + Environment.NewLine);
                s.Append("  <div class=\"g-recaptcha\" data-sitekey=\"" + key + "\"></div>" + Environment.NewLine);
                s.Append("</form>" + Environment.NewLine);

                ltlOutput.Text = s.ToString();
            }
            else
            {
                string EncodedResponse = Request.Form["g-Recaptcha-Response"];
                IsCaptchaValid = (ReCaptchaClass.Validate(EncodedResponse) == "True" ? true : false);
            }
        }

        public override object Value
        {
            get
            {
                if (IsCaptchaValid)
                {
                    return "Not a robot";
                }
                else
                {
                    return null;
                }
            }

            set { }
        }
    }


    public class ReCaptchaClass
    {
        public static string Validate(string EncodedResponse)
        {
            var client = new System.Net.WebClient();

            string PrivateKey = "6LfeagoTAAAAABOxIX6A1n0TDIdbXyIG0SSueoRS";

            var GoogleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse));

            var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptchaClass>(GoogleReply);

            return captchaResponse.Success;
        }

        [JsonProperty("success")]
        public string Success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        private string m_Success;
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes
        {
            get { return m_ErrorCodes; }
            set { m_ErrorCodes = value; }
        }

        private List<string> m_ErrorCodes;
    }
}
