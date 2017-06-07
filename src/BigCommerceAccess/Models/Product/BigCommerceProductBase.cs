using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProductBase: BigCommerceObjectBase
	{
		[ DataMember( Name = "inventory_level" ) ]
		public string Quantity{ get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku{ get; set; }
	}

	public enum InventoryTrackingEnum
	{
		none,
		simple,
		sku
	}
}