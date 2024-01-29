namespace CMS.Modules
{
    /// <summary>
    /// Encapsulates the result of <see cref="UIElementInfo"/> access check.
    /// </summary>
    public sealed class ElementAccessCheckResult
    {
        /// <summary>
        /// Element access check status.
        /// </summary>
        public ElementAccessCheckStatus ElementAccessCheckStatus
        {
            get;
            private set;
        }


        /// <summary>
        /// <see cref="UIElementInfo"/> for which access check was made.
        /// </summary>
        public UIElementInfo UIElementInfo
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates access check result for given <see cref="UIElementInfo"/>.
        /// </summary>
        /// <param name="elementAccessCheckStatus">Status of element access check</param>
        /// <param name="uiElementInfo"><see cref="UIElementInfo"/> for which access check was made</param>
        public ElementAccessCheckResult(ElementAccessCheckStatus elementAccessCheckStatus, UIElementInfo uiElementInfo)
        {
            ElementAccessCheckStatus = elementAccessCheckStatus;
            UIElementInfo = uiElementInfo;
        }
    }
}
