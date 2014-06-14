using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	public class BigCommerceProductBase : BigCommerceObjectBase
	{
		[ DataMember( Name = "inventory_level" ) ]
		public string Quantity { get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku { get; set; }
	}
}