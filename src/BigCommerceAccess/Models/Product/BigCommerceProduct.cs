using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProduct: BigCommerceProductBase
	{
		[ DataMember( Name = "inventory_tracking" ) ]
		public InventoryTrackingEnum InventoryTracking{ get; set; }

		[ DataMember( Name = "skus" ) ]
		public BigCommerceReferenceObject ProductOptionsReference{ get; set; }

		public List< BigCommerceProductOption > ProductOptions{ get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

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

		public string WeightUnit{ get; set; }

		[ DataMember( Name = "brand_id" ) ]
		public long? BrandId{ get; set; }

		public string BrandName{ get; set; }

		[ DataMember( Name = "primary_image" ) ]
		public BigCommerceProductPrimaryImages ImageUrls{ get; set; }

		public BigCommerceProduct()
		{
			this.ProductOptions = new List< BigCommerceProductOption >();
		}
	}

	public enum InventoryTrackingEnum
	{
		none,
		simple,
		sku
	}
}