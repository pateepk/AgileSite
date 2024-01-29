using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Conditional event execute in before handler
    /// </summary>
    public class ConditionalEvent<TArgs> : ConditionalEventBase<ConditionalEvent<TArgs>, TArgs>
        where TArgs : EventArgs
    {
    }
}
