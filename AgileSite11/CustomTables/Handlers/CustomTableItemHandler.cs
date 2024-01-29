using CMS.Base;

namespace CMS.CustomTables
{
    /// <summary>
    /// Custom table item handler
    /// </summary>
    public class CustomTableItemHandler : AdvancedHandler<CustomTableItemHandler, CustomTableItemEventArgs>, IRecursionControlHandler<CustomTableItemEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CustomTableItemHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public CustomTableItemHandler(CustomTableItemHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="item">Handled custom table item</param>
        public CustomTableItemHandler StartEvent(CustomTableItem item)
        {
            CustomTableItemEventArgs e = new CustomTableItemEventArgs
                {
                    Item = item
                };

            return StartEvent(e);
        }


        /// <summary>
        /// Gets the recursion key for this handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        public string GetRecursionKey(CustomTableItemEventArgs e)
        {
            if (e != null)
            {
                var obj = e.Item;
                if (obj != null)
                {
                    return obj.Generalized.GetObjectKey();
                }
            }

            return null;
        }
    }
}