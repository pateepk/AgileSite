using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// The default implementation of <see cref="IDeliveryBuilder"/>. Creates delivery objects based on the calculation request.
    /// </summary>
    /// <remarks>
    /// This is the default implementation of <see cref="IDeliveryBuilder"/>. You can derive your own implementation from this class. 
    /// The following code illustrates how to register custom implementation.
    /// <code>
    /// [assembly:RegisterImplementation(typeof(IDeliveryBuilder), typeof(MyDeliveryBuilder))]
    /// public class MyDeliveryBuilder : DefaultDeliveryBuilder
    /// {
    ///     ...
    /// }
    /// </code>
    /// </remarks>
    /// <seealso cref="IDeliveryBuilder"/>
    /// <seealso cref="Ecommerce.Delivery"/>
    /// <seealso cref="DeliveryItem"/>
    public class DefaultDeliveryBuilder : IDeliveryBuilder
    {
        private Delivery Delivery
        {
            get;
        } = new Delivery();


        private List<DeliveryItem> Items
        {
            get;
        } = new List<DeliveryItem>();


        private DataContainer CustomData
        {
            get;
            set;
        } = new DataContainer();


        private List<IDataContainer> CustomDataContainers
        {
            get;
        } = new List<IDataContainer>();


        /// <summary>
        /// Sets the currently constructed <see cref="Ecommerce.Delivery"/> according to the supplied calculation request data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The following data is taken from the calculation request:
        /// <list type="bullet">
        /// <item><term>Shipping option</term>
        /// <description>The shipping option taken from <see cref="CalculationRequest.ShippingOption"/>.</description>
        /// </item>
        /// <item><term>Delivery address</term>
        /// <description>Taken from <see cref="CalculationRequest.ShippingAddress"/> or <see cref="CalculationRequest.BillingAddress"/> when not present.</description>
        /// </item>
        /// <item><term>Items</term>
        /// <description>All <see cref="CalculationRequestItem">items</see> representing products are included. 
        /// The <paramref name="itemSelector"/> allows you to further narrow the subset of items added to the constructed constructed delivery.</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// Please note that <see cref="CMS.Ecommerce.Delivery.CustomData"/> are not populated from the calculation request. The same applies to <see cref="DeliveryItem.CustomData"/> created from calculation request items.
        /// To include some custom data into <see cref="Ecommerce.Delivery"/>, inherit your custom delivery builder.
        /// </para>
        /// </remarks>
        /// <param name="request">The <see cref="CalculationRequest">Calculation request data</see> to be packed in the constructed delivery.</param>
        /// <param name="itemSelector">The predicate deciding which <see cref="CalculationRequestItem">items</see> are added to <see cref="Ecommerce.Delivery"/>. 
        /// When <c>null</c>, all items are added.</param>
        public void SetFromCalculationRequest(CalculationRequest request, Func<CalculationRequestItem, bool> itemSelector = null)
        {
            SetShippingOption(request.ShippingOption);
            SetDeliveryAddress(request.ShippingAddress ?? request.BillingAddress);
            AddItems(request);
            SetWeight((decimal)request.TotalItemsWeight);

            InitCustomData(request);
        }


        /// <summary>
        /// Adds calculation request items selected by the <paramref name="itemSelector"/> predicate to the constructed <see cref="Ecommerce.Delivery"/>.
        /// </summary>
        /// <remarks>
        /// All <see cref="CalculationRequestItem">items</see> representing products are included. 
        /// The <paramref name="itemSelector"/> allows you to further narrow the subset if items placed into constructed delivery.
        /// </remarks>
        /// <param name="request">The <see cref="CalculationRequest"/> object from which the items are added.</param>
        /// <param name="itemSelector">The predicate deciding which items are added to <see cref="Ecommerce.Delivery"/>. 
        /// When <c>null</c>, all items are added.</param>
        protected virtual void AddItems(CalculationRequest request, Func<CalculationRequestItem, bool> itemSelector = null)
        {
            itemSelector = itemSelector ?? (item => true);

            var items = request.Items
                            .Where(itemSelector)
                            .Where(item => !item.SKU.IsProductOption || item.SKU.IsAccessoryProduct);

            foreach (var item in items)
            {
                AddItem(item);
            }
        }


        /// <summary>
        /// Adds a calculation request item to the constructed <see cref="Ecommerce.Delivery"/>.
        /// </summary>
        /// <param name="item">The item to be added to the constructed <see cref="Ecommerce.Delivery"/>.</param>
        protected virtual void AddItem(CalculationRequestItem item)
        {
            Items.Add(CreateDeliveryItem(item));
        }


        /// <summary>
        /// Creates a new <see cref="DeliveryItem"/> object for the given calculation request item.
        /// </summary>
        /// <param name="requestItem">The request item for which the delivery item is created.</param>
        protected virtual DeliveryItem CreateDeliveryItem(CalculationRequestItem requestItem)
        {
            var deliveryItem = new DeliveryItem(requestItem);

            InitItemCustomData(deliveryItem, requestItem);

            return deliveryItem;
        }


        /// <summary>
        /// Sets an address to which the currently constructed <see cref="Ecommerce.Delivery"/> will be shipped.
        /// </summary>
        /// <param name="address">The target address.</param>
        public void SetDeliveryAddress(IAddress address)
        {
            Delivery.DeliveryAddress = address;
        }


        /// <summary>
        /// Sets the shipping option used for the currently constructed <see cref="Ecommerce.Delivery"/>.
        /// </summary>
        /// <param name="shippingOption">The used shipping option.</param>
        public void SetShippingOption(ShippingOptionInfo shippingOption)
        {
            Delivery.ShippingOption = shippingOption;
        }


        /// <summary>
        /// Sets weight of the package with the whole <see cref="Ecommerce.Delivery"/>.
        /// </summary>
        /// <param name="weight">Weight of the <see cref="Ecommerce.Delivery"/>.</param>
        public void SetWeight(decimal weight)
        {
            Delivery.Weight = weight;
        }


        /// <summary>
        /// Sets the date of shipping.
        /// </summary>
        /// <param name="shippingDate">The date of shipping.</param>
        public void SetShippingDate(DateTime shippingDate)
        {
            Delivery.ShippingDate = shippingDate;
        }

        
        /// <summary>
        /// Override this method to add custom data to the constructed <see cref="Ecommerce.Delivery"/>.
        /// </summary>
        /// <remarks>
        /// The following code illustrates possibilities of adding custom data to <see cref="Ecommerce.Delivery"/>.
        /// <code>
        /// protected override void InitCustomData(CalculationRequest request)
        /// {
        ///     base.InitCustomData(request);
        ///     
        ///     // Single value
        ///     SetCustomData("RegisteredCustomer", request.Customer.CustomerIsRegistered);
        ///     
        ///     // Add complete calculation request's custom data to delivery
        ///     AddContainerToCustomData(request.RequestCustomData);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">The calculation request data from which the custom data is gotten.</param>
        protected virtual void InitCustomData(CalculationRequest request)
        {
            // No custom data by default
        }


        /// <summary>
        /// Sets custom data under the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">Custom data to be stored under given <paramref name="key"/>.</param>
        public void SetCustomData(string key, object data)
        {
            CustomData[key] = data;
        }


        /// <summary>
        /// Adds the data container to the custom data of the constructed <see cref="Ecommerce.Delivery"/>.
        /// </summary>
        /// <param name="container">The data container to be added.</param>
        protected void AddContainerToCustomData(IDataContainer container)
        {
            CustomDataContainers.Add(container);
        }


        /// <summary>
        /// Adds custom data to the constructed <see cref="DeliveryItem"/>.
        /// </summary>
        /// <param name="deliveryItem">The delivery item to which custom data is set.</param>
        /// <param name="requestItem">The calculation request item from which the deliver item is constructed.</param>
        protected virtual void InitItemCustomData(DeliveryItem deliveryItem, CalculationRequestItem requestItem)
        {
            var containers = new List<IDataContainer>
                             {
                                 new DataContainer()
                             };

            GetItemCustomDataContainers(containers, requestItem);

            deliveryItem.CustomData = new AggregatedDataContainer(containers.ToArray());
        }




        /// <summary>
        /// Override this method to populate the supplied list with the custom data containers.
        /// </summary>
        /// <remarks>
        /// The following code illustrates how to add custom data to <see cref="DeliveryItem"/>.
        /// <code>
        /// protected override void GetItemCustomDataContainers(List&lt;IDataContainer&gt; containers, CalculationRequestItem requestItem)
        /// {
        ///     base.GetItemCustomDataContainers(containers, requestItem);
        ///     
        ///     containers.Add(requestItem.ItemCustomData);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="containers">The list of containers to be populated.</param>
        /// <param name="requestItem">The calculation request item for which the custom data is processed.</param>
        protected virtual void GetItemCustomDataContainers(List<IDataContainer> containers, CalculationRequestItem requestItem)
        {
            // No item custom data by default
        }


        /// <summary>
        /// Creates a new <see cref="Ecommerce.Delivery"/> object based on builder settings.
        /// </summary>
        /// <returns>A new <see cref="Ecommerce.Delivery"/> object.</returns>
        public virtual Delivery BuildDelivery()
        {
            var containers = new List<IDataContainer>(CustomDataContainers);
            containers.Insert(0, CustomData);
            Delivery.CustomData = new AggregatedDataContainer(containers.ToArray());

            Delivery.Items = new List<DeliveryItem>(Items);

            EnsureDefaultValues(Delivery);

            EnsureNewCustomDataInstance();

            return Delivery.Clone();
        }


        /// <summary>
        /// Sets the shipping date of the delivery to now when no specific date was set. 
        /// Override this method to ensure the default data in the constructed <see cref="Ecommerce.Delivery"/>
        /// </summary>
        /// <remarks>
        /// This method is called before new delivery is returned.
        /// </remarks>
        /// <param name="delivery">The constructed <see cref="Ecommerce.Delivery"/>.</param>
        protected virtual void EnsureDefaultValues(Delivery delivery)
        {
            if (delivery.ShippingDate == default(DateTime))
            {
                SetShippingDate(DateTime.Now);
            }
        }


        /// <summary>
        /// Creates new instance of custom data container containing same values as original one.
        /// </summary>
        private void EnsureNewCustomDataInstance()
        {
            if (CustomData.ColumnNames.Any())
            {
                var newCustomData = new DataContainer();

                foreach (var column in CustomData.ColumnNames)
                {
                    newCustomData[column] = CustomData[column];
                }

                CustomData = newCustomData;
            }
        }
    }
}
