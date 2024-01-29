using CMS.Base;

namespace CMS.OnlineForms
{
    /// <summary>
    /// BizForm item handler
    /// </summary>
    public class BizFormItemHandler : AdvancedHandler<BizFormItemHandler, BizFormItemEventArgs>, IRecursionControlHandler<BizFormItemEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BizFormItemHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public BizFormItemHandler(BizFormItemHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="item">Handled item</param>
        public BizFormItemHandler StartEvent(BizFormItem item)
        {
            BizFormItemEventArgs e = new BizFormItemEventArgs
                {
                    Item = item
                };

            return StartEvent(e);
        }


        /// <summary>
        /// Gets the recursion key of the class to identify recursion
        /// </summary>
        public string GetRecursionKey(BizFormItemEventArgs e)
        {
            if (e == null)
            {
                return null;
            }

            var obj = e.Item;
            if (obj != null)
            {
                return obj.Generalized.GetObjectKey();
            }

            return null;
        }
    }
}