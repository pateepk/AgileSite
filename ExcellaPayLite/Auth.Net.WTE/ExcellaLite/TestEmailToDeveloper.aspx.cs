﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PaymentProcessor.Web.Applications;
using System.Net;
using System.Text;
using System.IO;
using System.ServiceModel.Channels;
using WTE.Communication;

namespace ExcellaLite
{
    public partial class TestEmailToDeveloper : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            EmailManager em = new EmailManager();
            em.TestEmailToDeveloper();
        }
    }
}