using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.SiteProvider;

using CMS.FormEngine.Web.UI;

using Newtonsoft.Json;

namespace NHG_C
{
    public partial class BlueKey_CMSFormControls_NoCaptchaReCaptcha : FormEngineUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            StringBuilder s = new StringBuilder();

            string key = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSReCaptchaPublicKey");

            s.Append("<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>" + Environment.NewLine);
            s.Append("<form action=\"?\" method=\"POST\">" + Environment.NewLine);
            s.Append("  <div class=\"g-recaptcha\" data-sitekey=\"" + key + "\"></div>" + Environment.NewLine);
            s.Append("</form>" + Environment.NewLine);

            ltlOutput.Text = s.ToString();
        }

        public override object Value
        {
            get
            {
                string EncodedResponse = Request.Form["g-Recaptcha-Response"];
                bool IsCaptchaValid = (ReCaptchaClass.Validate(EncodedResponse) == "True" ? true : false);

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

            string PrivateKey = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSReCaptchaPrivateKey");

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