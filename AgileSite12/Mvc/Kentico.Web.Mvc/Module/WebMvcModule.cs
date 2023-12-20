using CMS;
using CMS.DataEngine;
using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: RegisterModule(typeof(WebMvcModule))]

namespace Kentico.Web.Mvc
{
    internal class WebMvcModule : Module
    {
        public WebMvcModule()
            : base("Kentico.Web.Mvc")
        {
        }


        protected override void OnInit()
        {
            base.OnInit();

            ResponseFilter.Init();
        }
    }
}
