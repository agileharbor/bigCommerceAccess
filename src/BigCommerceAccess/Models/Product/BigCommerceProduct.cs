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

		[DataMember(Name = "main_images")]
		public List<BigCommerceImage> Main_Images { get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[DataMember(Name = "availability")]
		public string Availability { get; set; }  

		[DataMember(Name = "condition")]
		public string Condition { get; set; } 

		[ DataMember( Name = "description" ) ]
		public string Description{ get; set; }

		[ DataMember( Name = "price" ) ]
		public decimal? Price{ get; set; }

		[DataMember(Name = "is_visible")]
		public bool IsVisible { get; set; }

		[DataMember(Name = "type")]
		public string Type { get; set; }

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

		[ DataMember( Name = "thumbnail_image") ]
		public BigCommerceProductPrimaryImages ThumbnailImageURL { get; set; }

		[DataMember(Name = "custom_url")]
		public string Product_URL { get; set; }

		[DataMember(Name = "categories")]
		public int[] Categories { get; set; }

		public BigCommerceProduct()
		{
			this.ProductOptions = new List< BigCommerceProductOption >();
			this.Main_Images = new List<BigCommerceImage>();
		}
	}
}