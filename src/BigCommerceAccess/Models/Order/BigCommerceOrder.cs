using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using BigCommerceAccess.Models.Address;

namespace BigCommerceAccess.Models.Order
{
	[ DataContract ]
	public class BigCommerceOrder: BigCommerceObjectBase
	{
		[ DataMember( Name = "status_id" ) ]
		public int StatusId{ get; set; }

		[ DataMember( Name = "date_created" ) ]
		public DateTime DateCreated{ get; set; }

		[ DataMember( Name = "date_shipped" ) ]
		public DateTime? DateShipped{ get; set; }

		[ DataMember( Name = "products" ) ]
		public BigCommerceReferenceObject ProductsReference{ get; set; }

		[ DataMember( Name = "shipping_addresses" ) ]
		public BigCommerceReferenceObject ShippingAddressesReference{ get; set; }

		[ DataMember( Name = "billing_address" ) ]
		public BigCommerceBillingAddress BillingAddress{ get; set; }

		[ DataMember( Name = "customer_message" ) ]
		public string CustomerMessage{ get; set; }

		[ DataMember( Name = "staff_notes" ) ]
		public string StaffNotes{ get; set; }

		[ DataMember( Name = "total_inc_tax" ) ]
		public string Total{ get; set; }

		[ DataMember( Name = "is_deleted" ) ]
		public bool IsDeleted{ get; set; }

		[ DataMember( Name = "shipping_cost_ex_tax" ) ]
		public string ShippingCostExTax{ get; set; }

		[ DataMember( Name = "handling_cost_ex_tax" ) ]
		public string HandlingCostExTax{ get; set; }

		[ DataMember( Name = "wrapping_cost_ex_tax" ) ]
		public string WrappingCostExTax{ get; set; }

		private List< BigCommerceOrderProduct > _products;
		private List< BigCommerceShippingAddress > _shippingAddresses;

		[ DataMember( Name = "discount_amount" ) ]
		public string DiscountAmountValue{ get; set; }

		[ DataMember( Name = "total_tax" ) ]
		public string TotalTaxValue{ get; set; }

		[ DataMember( Name = "currency_code" ) ]
		public string CurrencyCode{ get; set; }

		public List< BigCommerceOrderProduct > Products
		{
			get { return this._products; }
			set
			{
				if( value != null )
					this._products = value;
			}
		}

		public List< BigCommerceShippingAddress > ShippingAddresses
		{
			get { return this._shippingAddresses; }
			set
			{
				if( value != null )
					this._shippingAddresses = value;
			}
		}

		public bool IsShipped
		{
			get { return this.DateShipped != DateTime.MinValue; }
		}

		public BigCommerceOrderStatusEnum OrderStatus
		{
			get { return ( BigCommerceOrderStatusEnum )this.StatusId; }
		}

		public decimal ShippingCharge
		{
			get
			{
				decimal baseShippingCost;
				decimal.TryParse( this.ShippingCostExTax, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out baseShippingCost );

				decimal baseHandlingCost;
				decimal.TryParse( this.HandlingCostExTax, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out baseHandlingCost );

				decimal baseWrappingCost;
				decimal.TryParse( this.WrappingCostExTax, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out baseWrappingCost );
				return baseShippingCost + baseHandlingCost + baseWrappingCost;
			}
		}

		public decimal DiscountAmount
		{
			get
			{	
				decimal discountAmount;
				decimal.TryParse( this.DiscountAmountValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out discountAmount );
				return discountAmount;
			}
		}

		public decimal TotalTax
		{
			get
			{	
				decimal totalTax;
				decimal.TryParse( this.TotalTaxValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out totalTax );
				return totalTax;
			}
		}

		public BigCommerceOrder()
		{
			this._products = new List< BigCommerceOrderProduct >();
			this._shippingAddresses = new List< BigCommerceShippingAddress >();
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
		ManualVerificationRequired,
		Disputed,
		PartiallyRefunded
	}
}