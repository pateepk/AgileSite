using System;
using System.Linq;
using System.Text;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Data type for one web template given from all templates feed.
    /// </summary>
    public class StrandsWebTemplateData
    {
        /// <summary>
        /// ID of template.
        /// </summary>
        public string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Title of template.
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// Type of template.
        /// </summary>
        public StrandsWebTemplateTypeEnum Type
        {
            get;
            set;
        }
    }
}
