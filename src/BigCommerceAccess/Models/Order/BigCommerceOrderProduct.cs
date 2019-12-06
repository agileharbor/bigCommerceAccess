using System.Globalization;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Order
{
	[ DataContract ]
	public class BigCommerceOrderProduct : BigCommerceObjectBase
	{
		[ DataMember( Name = "name" ) ]
		public string Name { get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku { get; set; }

		[ DataMember( Name = "quantity" ) ]
		public int Quantity { get; set; }

		[ DataMember( Name = "price_inc_tax" ) ]
		public string PriceIncTax{ get; set; }

		[ DataMember( Name = "price_ex_tax" ) ]
		public string PriceExclTax{ get; set; }

		[ DataMember( Name = "base_price" ) ]
		public string BasePrice{ get; set; }

		public decimal Tax
		{
			get
			{
				decimal priceIncludingTax;
				decimal.TryParse( this.PriceIncTax, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out priceIncludingTax );

				decimal priceExcludingTax;
				decimal.TryParse( this.PriceExclTax, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out priceExcludingTax );

				return  priceIncludingTax - priceExcludingTax;
			}
		}
	}
}