using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceVariant
	{
		[ DataMember( Name = "id" ) ]
		public long Id{ get; set; }

		[ DataMember( Name = "product_id" ) ]
		public long ProductId{ get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku{ get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "price" ) ]
		public decimal? Price{ get; set; }

		[ DataMember( Name = "cost_price" ) ]
		public decimal? CostPrice{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public decimal? Weight{ get; set; }

		[ DataMember( Name = "image_url" ) ]
		public string ImageUrl{ get; set; }

		[ DataMember( Name = "inventory_level" ) ]
		public string Quantity{ get; set; }
	}
}