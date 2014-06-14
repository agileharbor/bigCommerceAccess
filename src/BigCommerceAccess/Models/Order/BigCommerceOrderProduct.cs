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
		public string Price { get; set; }
	}
}