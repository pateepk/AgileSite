using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Container to wrap the UI form structure
    /// </summary>
    public class UIFormMacroContainer : FormMacroContainer<UIForm>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">Wrapped form</param>
        public UIFormMacroContainer(UIForm form)
            : base(form)
        {
        }


        /// <summary>
        /// Constructor for virtual resolvers
        /// </summary>
        public UIFormMacroContainer()
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers columns
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("Data", f => (f != null) ? f.Data : null);
            RegisterColumn<FormModeEnum>("FormMode", f => (f != null) ? f.Mode : FormModeEnum.Insert);
            RegisterColumn<int>("ObjectSiteID", f => (f != null) ? f.ObjectSiteID : 0);
            RegisterColumn("ObjectType", f => (f != null) ? f.ObjectType : null);
            RegisterColumn("AlternativeFormName", f => (f != null) ? f.AlternativeFormName : null);
        }

        #endregion
    }
}