using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Represents a boolean expression for macro designer control.
    /// </summary>
    public abstract class MacroBoolExpression : CMSUserControl
    {
        /// <summary>
        /// Gets or sets the left part of the expression.
        /// </summary>
        public virtual string LeftExpression
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Gets or sets the left part of the expression.
        /// </summary>
        public virtual string RightExpression
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Gets or sets the left part of the expression.
        /// </summary>
        public virtual string Operator
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Gets or sets resolver to use.
        /// </summary>
        public virtual string ResolverName
        {
            get;
            set;
        }
    }
}