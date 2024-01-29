using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.StrandsRecommender.Web.UI;

[assembly: RegisterModule(typeof(StrandsRecommenderWebUIModule))]

namespace CMS.StrandsRecommender.Web.UI
{
    /// <summary>
    /// Represents StrandsRecommender Web UI module. This module handles integration with the Strands Recommender (http://recommender.strands.com/) 
    /// and provides online store administrator the ability to enhance sales by automatically his recommending products to his customers.
    /// </summary>
    internal class StrandsRecommenderWebUIModule : Module
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StrandsRecommenderWebUIModule() 
            : base(new ModuleMetadata("CMS.StrandsRecommender.Web.UI"))
        {
        }


        /// <summary>
        /// Handles the module initialization.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            StrandsRecommenderWebUIHandlers.Init();
        }
    }
}
