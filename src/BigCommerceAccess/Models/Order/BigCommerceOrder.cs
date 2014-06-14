using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BigCommerceAccess.Models.Address;

namespace BigCommerceAccess.Models.Order
{
	[ DataContract ]
	public class BigCommerceOrder : BigCommerceObjectBase
	{
		[ DataMember( Name = "status_id" ) ]
		public int StatusId { get; set; }

		[ DataMember( Name = "date_created" ) ]
		public DateTime DateCreated { get; set; }

		[ DataMember( Name = "date_shipped" ) ]
		public DateTime DateShipped { get; set; }

		[ DataMember( Name = "products" ) ]
		public BigCommerceReferenceObject ProductsReference { get; set; }

		[ DataMember( Name = "shipping_addresses" ) ]
		public BigCommerceReferenceObject ShippingAddressesReference { get; set; }

		[ DataMember( Name = "billing_address" ) ]
		public BigCommerceBillingAddress BillingAddress { get; set; }

		[ DataMember( Name = "total_inc_tax" ) ]
		public string Total { get; set; }

		[ DataMember( Name = "is_deleted" ) ]
		public bool IsDeleted { get; set; }

		public IList< BigCommerceOrderProduct > Products { get; set; }
		public IList< BigCommerceShippingAddress > ShippingAddresses { get; set; }

		public bool IsShipped
		{
			get { return this.DateShipped != DateTime.MinValue; }
		}

		public BigCommerceOrderStatusEnum OrderStatus
		{
			get { return ( BigCommerceOrderStatusEnum )this.StatusId; }
		}

		public BigCommerceOrder()
		{
			this.Products = new List< BigCommerceOrderProduct >();
			this.ShippingAddresses = new List< BigCommerceShippingAddress >();
		}
	}

	public enum BigCommerceOrderStatusEnum
	{
		Incomplete,
		Pending,
		Shipped,
		PartiallyShipped,
		Refunded,
		Canceled,
		Declined,
		AwaitingPayment,
		AwaitingPickup,
		AwaitingShipment,
		Completed,
		AwaitingFulfillment,
		ManualVerificationRequired
	}
}