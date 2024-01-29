
namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Class that stores specified data needed for Strands purchased event.
    /// </summary>
    public class StrandsPurchasedEventData
    {
        /// <summary>
        /// ID of purchased item.
        /// </summary>
        public string ItemID
        {
            get;
            set;
        }


        /// <summary>
        /// Price of item.
        /// </summary>
        public decimal Price
        {
            get;
            set;
        }


        /// <summary>
        /// Quantity of purchased items.
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }
    }
}
