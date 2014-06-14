using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProduct : BigCommerceProductBase
	{
		[ DataMember( Name = "inventory_tracking" ) ]
		public InventoryTrackingEnum InventoryTracking { get; set; }

		[ DataMember( Name = "skus" ) ]
		public BigCommerceReferenceObject ProductSkusReference { get; set; }

		public IList< BigCommerceProductOption > ProductSkus { get; set; }
	}

	public enum InventoryTrackingEnum
	{
		none,
		simple,
		sku
	}
}