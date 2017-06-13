using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProductOption: BigCommerceProductBase
	{
		[ DataMember( Name = "product_id" ) ]
		public long ProductId{ get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "price" ) ]
		public decimal? Price{ get; set; }

		[ DataMember( Name = "adjusted_price" ) ]
		public decimal? AdjustedPrice{ get; set; }

		[ DataMember( Name = "cost_price" ) ]
		public decimal? CostPrice{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public decimal? Weight{ get; set; }

		[ DataMember( Name = "adjusted_weight" ) ]
		public decimal? AdjustedWeight{ get; set; }

		[ DataMember( Name = "image_file" ) ]
		public string ImageFile{ get; set; }
	}
}