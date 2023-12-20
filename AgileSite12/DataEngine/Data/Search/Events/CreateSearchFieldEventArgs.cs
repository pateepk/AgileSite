using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Arguments of event represented by <see cref="CreateSearchFieldHandler"/>.
    /// </summary>
    public class CreateSearchFieldEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Search field created by the factory.
        /// </summary>
        public ISearchField SearchField
        {
            get;
            set;
        }


        /// <summary>
        /// Creation process option.
        /// </summary>
        public CreateSearchFieldOption CreateOption
        {
            get;
            set;
        }
    }
}
