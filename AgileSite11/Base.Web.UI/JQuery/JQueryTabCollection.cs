using System;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// JQuery tabs collection class.
    /// </summary>
    public class JQueryTabCollection : ControlCollection
    {
        #region "Properties"

        /// <summary>
        /// Tabs collection.
        /// </summary>
        public new JQueryTab this[int index]
        {
            get
            {
                return (JQueryTab)base[index];
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// JQuery tab collection constructor.
        /// </summary>
        public JQueryTabCollection(Control owner)
            : base(owner)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds the specified System.Web.UI.Control object to the collection at the specified index location.
        /// </summary>
        public override void Add(Control child)
        {
            if (!(child is JQueryTab))
            {
                throw new ArgumentException("JQueryTabCollection can only contain JQueryTab controls.");
            }
            base.Add(child);
        }


        /// <summary>
        /// Adds the specified System.Web.UI.Control object to the collection at the specified index location.
        /// </summary>
        public override void AddAt(int index, Control child)
        {
            if (!(child is JQueryTab))
            {
                throw new ArgumentException("JQueryTabCollection can only contain JQueryTab controls.");
            }
            base.AddAt(index, child);
        }

        #endregion
    }
}