using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the transformation selection. 
    /// </summary>
    [ToolboxData("<{0}:SelectHierarchicalTransformation runat=server></{0}:SelectHierarchicalTransformation>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectHierarchicalTransformation : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectHierarchicalTransformation()
        {
            FormControlName = "HierarchicalTransformationSelector";
        }
    }
}
