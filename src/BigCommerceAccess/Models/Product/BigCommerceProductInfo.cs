using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProductInfo
	{
		[ DataMember( Name = "id" ) ]
		public int Id{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku{ get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "description" ) ]
		public string Description{ get; set; }

		[ DataMember( Name = "price" ) ]
		public decimal? Price{ get; set; }

		[ DataMember( Name = "sale_price" ) ]
		public decimal? SalePrice{ get; set; }

		[ DataMember( Name = "retail_price" ) ]
		public decimal? RetailPrice{ get; set; }

		[ DataMember( Name = "cost_price" ) ]
		public decimal? CostPrice{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public decimal? Weight{ get; set; }

		[ DataMember( Name = "brand_id" ) ]
		public int? BrandId{ get; set; }

		[ DataMember( Name = "images" ) ]
		public List< BigCommerceImage > Images{ get; set; }

		[DataMember(Name = "categories")]
		public int[] Categories { get; set; }

		[DataMember(Name = "custom_url")]
		public BigCommerceCustomURL Product_URL { get; set; }

		[ DataMember( Name = "variants" ) ]
		public List< BigCommerceVariant > Variants{ get; set; }

		[ DataMember( Name = "inventory_tracking" ) ]
		public string InventoryTracking{ get; set; }

		[ DataMember( Name = "inventory_level" ) ]
		public string Quantity{ get; set; }

		public BigCommerceProductInfo()
		{
			this.Images = new List< BigCommerceImage >();
			this.Variants = new List< BigCommerceVariant >();
		}
	}
}