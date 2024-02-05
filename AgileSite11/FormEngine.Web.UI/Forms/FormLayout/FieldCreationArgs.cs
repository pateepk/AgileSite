namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Data container of objects necessary for rendering form layout.
    /// </summary>
    public class FieldCreationArgs
    {
        /// <summary>
        /// Form field settings.
        /// </summary>
        public FormFieldInfo FormFieldInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Form category settings.
        /// </summary>
        public FormCategoryInfo FormCategoryInfo
        {
            get;
            set;
        }


        /// <summary>
        /// ID of form group (category) anchor.
        /// </summary>
        public string AnchorID
        {
            get;
            set;
        }


        /// <summary>
        /// Image used for collapsing form groups (categories).
        /// </summary>
        public CollapsibleImage CollapsibleImage
        {
            get;
            set;
        }
    }
}