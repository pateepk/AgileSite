using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Newsletters;
using CMS.StrandsRecommender;

[assembly: RegisterModule(typeof(StrandsRecommenderModule))]

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Represents StrandsRecommender module. This module handles integration with the Strands Recommender (http://recommender.strands.com/) 
    /// and provides online store administrator the ability to enhance sales by automatically his recommending products to his customers.
    /// </summary>
    internal class StrandsRecommenderModule : Module
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StrandsRecommenderModule() 
            : base(new StrandsRecommenderModuleMetadata())
        {
        }


        /// <summary>
        /// Handles the module initialization.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            StrandsRecommenderHandlers.Init();
        }
    }
}
