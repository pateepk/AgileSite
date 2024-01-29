namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Localizable form engine user control 
    /// </summary>
    public abstract class LocalizableFormEngineUserControl : FormEngineUserControl
    {
        /// <summary>
        /// Saves translation for given resource string.
        /// </summary>
        public virtual bool Save()
        {
            return false;
        }
    }
}
