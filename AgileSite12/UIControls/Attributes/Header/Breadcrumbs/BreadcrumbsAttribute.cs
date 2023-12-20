using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Tabs definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BreadcrumbsAttribute : AbstractAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BreadcrumbsAttribute()
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender</param>
        public void Apply(object sender)
        {
            if (sender is CMSPage)
            {
            }
            else
            {
                throw new Exception("[TabAttribute.Apply]: The attribute [Breadcrumbs] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}