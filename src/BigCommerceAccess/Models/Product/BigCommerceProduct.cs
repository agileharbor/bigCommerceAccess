using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProduct
	{
		[ DataMember( Name = "id" ) ]
		public long Id { get; set; }

		[ DataMember( Name = "inventory_level" ) ]
		public string Quantity { get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku { get; set; }
	}
}